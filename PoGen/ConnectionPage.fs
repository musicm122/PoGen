namespace PoGen

open System

module ConnectionPage =
    open Avalonia.Controls
    open Avalonia.Media.Imaging
    open Avalonia.FuncUI.DSL
    open Avalonia.FuncUI.Components
    open Avalonia.FuncUI.Elmish
    open Elmish
    open PoGen.Components
    open PoGen.Models
    open PoGen.Messages
    open PoGen.Core
    open PoGen.DataAccess
    
    let runConnectionTestAsyncCmd (model: Model): Cmd<Msg> =
        async {
            let! connectionTestResult = DataAccess.testConnectionAsync model.ConnectionString.Value

            let result =
                match connectionTestResult.State with
                | Pass ->
                    { model with
                          CurrentFormState = Valid
                          Output = "Successfully Connected" }
                | Fail ex ->
                    { model with
                          CurrentFormState = InvalidConnectionString
                          Output = connectionTestResult.Message + Environment.NewLine + ex }
                | _ ->
                    { model with
                          CurrentFormState = InvalidConnectionString
                          Output = model.Output }

            let! _ =                        
                Application.Current.MainPage.DisplayAlert("Connection Test Result", result.Output, "Ok")
                |> Async.AwaitTask
            return TestConnectionComplete
        }
        |> Cmd.ofAsyncMsg

    let fetchDatabasesCmd (m: Model): Cmd<Msg> =
        async {
            let! dbs = DataAccess.getDatabaseNamesAsync m.ConnectionString
            let outMessage = sprintf "%i Databases found" dbs.Length
            let! output =
                match dbs.Length with
                | 0 ->
                    Application.Current.MainPage.DisplayAlert("Fetching Databases ", "No Databases found", "Ok")
                    |> Async.AwaitTask
                | _ ->
                    Application.Current.MainPage.DisplayAlert("Fetching Databases ", outMessage, "Ok")
                    |> Async.AwaitTask

            return FetchDatabasesComplete dbs
        }
        |> Cmd.ofAsyncMsg

    let fetchTablesCmd (m: Model): Cmd<Msg> =
        async {
            let db =
                match m.SelectedDatabase with
                | Some db -> db
                | None -> raise (System.ArgumentException("Missing Database"))

            let! tables = DataAccess.getTableNamesAsync db m.ConnectionString
            let outMessage = sprintf "%i Tables found" tables.Length
            let! _ =
                match tables.Length with
                | 0 ->
                    Application.Current.MainPage.DisplayAlert("Fetching Tables ", "No Tables found", "Ok")
                    |> Async.AwaitTask
                | _ ->
                    Application.Current.MainPage.DisplayAlert
                        ("Fetching Tables ", (sprintf "%i Tables found" tables.Length), "Ok") |> Async.AwaitTask

            return FetchTablesComplete tables
        }
        |> Cmd.ofAsyncMsg

    
    let init () =
        DataAccess.initDbMapper ()
        defaultModel (), Cmd.none
        
     let update (msg: Msg) (m: Model): Model * Cmd<_> =
        match msg with
        | UpdateConnectionStringValue conStringVal ->
            { m with
                  ConnectionString =
                      { Id = m.ConnectionString.Id
                        Name = m.ConnectionString.Name
                        Value = conStringVal }
                  CurrentFormState =
                      match Validation.IsConnectionStringInFormat m with
                      | false -> MissingConnStrValue
                      | _ -> Valid }, Cmd.none
        | SetSelectedDatabase dbValue ->
            match dbValue with
            | None -> { m with CurrentFormState = Idle }, Cmd.none
            | Some dbVal ->
                let selectedDb = m.Databases |> List.find (fun db -> db.Name = dbVal)
                { m with
                      CurrentFormState = FetchingData
                      SelectedDatabase = Some(selectedDb) },
                fetchTablesCmd
                    { m with
                          CurrentFormState = FetchingData
                          SelectedDatabase = Some(selectedDb) }
        | SetSelectedLanguage l -> { m with SelectedLanguage = l }, Cmd.none
        | BrowseForOutputFolder f -> { m with OutputLocation = f }, Cmd.none
        | TestConnection -> { m with CurrentFormState = Testing }, runConnectionTestAsyncCmd m
        | TestConnectionComplete -> { m with CurrentFormState = Idle }, Cmd.none
        | FetchDatabases ->
            { m with CurrentFormState = FetchingData }, fetchDatabasesCmd m
        | FetchDatabasesComplete dbs ->
            match dbs with
            | [] -> { m with CurrentFormState = Idle }, Cmd.none
            | dbItems ->
                let selectedDb = Some(dbItems.Head)

                let model =
                    { m with
                          CurrentFormState = FetchingData
                          SelectedDatabase = selectedDb
                          Databases = dbs }
                let retval = model, fetchTablesCmd model
                retval                
        | _ -> m, Cmd.none
    let view (model: Model) (dispatch) =
        let isLoading = (model.CurrentFormState = Testing)
        let updateConnectionStringValue = UpdateConnectionStringValue >> dispatch
        let testConnection = (fun _ -> dispatch TestConnection)

        let testButton =
            (match model.CurrentFormState with
             | Valid -> formButton "Test" testConnection Dock.Left (not isLoading)
             | _ -> formButtonDisabled "Test" Dock.Left)

        let buttonStack =
            horizontalStack [ testButton ]

        verticalStack
            [ progressBar 20.0 isLoading
              formLabel "Connection String Value"
              textEntry model.ConnectionString.Value updateConnectionStringValue
              horizontalStack [ testButton ] ]

    type Host() as this =
        inherit Hosts.HostControl()
        do
            /// You can use `.mkProgram` to pass Commands around
            /// if you decide to use it, you have to also return a Command in the initFn
            /// (init, Cmd.none)
            /// you can learn more at https://elmish.github.io/elmish/basics.html
            let startFn () =
                init
            Elmish.Program.mkSimple startFn update view
            |> Program.withHost this
            |> Program.run