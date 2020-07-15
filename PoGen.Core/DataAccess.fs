namespace PoGen

module DataAccess =

    open System.Data
    open Dapper
    open Dapper.FSharp
    open Dapper.FSharp.MSSQL
    open System.Data.SqlClient
    open PoGen.Models
    open System
    open Extensions

    let initDbMapper () =
        Dapper.FSharp.OptionTypes.register ()

    let testConnectionAsync (connectionString: ConnectionStringValue): Async<ConnectionTestResult> =
        async {
            try
                match connectionString.IsNullOrWhiteSpace() with
                | true ->
                    let ex =
                        ArgumentException("Missing required argument")

                    return { ConnectionTestResult.Message = "SQL Error " + ex.Message
                             ConnectionTestResult.State = Fail ex.Message }
                | false ->
                    use conn = new SqlConnection(connectionString)
                    let! connOpenResult = conn.OpenAsync() |> Async.AwaitTask

                    match conn.State with
                    | ConnectionState.Open ->
                        return { ConnectionTestResult.Message = "Success"
                                 ConnectionTestResult.State = Pass }
                    | _ ->
                        let connVal =
                            Enum.GetName(typeof<ConnectionState>, conn.State)

                        let err =
                            sprintf "Could not connect: Connection state of %s" connVal

                        return { ConnectionTestResult.Message = "Fail"
                                 ConnectionTestResult.State = Fail err }
            with :? Exception as ex ->
                return { ConnectionTestResult.Message = "Error :" + ex.Message
                         ConnectionTestResult.State = Fail ex.Message }
        }

    let getTableNamesAsync (database: DbItem) (conString: ConnectionStringItem): Async<Table list> =
        async {
            try
                use connection = new SqlConnection(conString.Value)
                do! connection.OpenAsync() |> Async.AwaitTask

                let sql = @"SELECT [t0].[TABLE_NAME]
            FROM [INFORMATION_SCHEMA].[TABLES] AS [t0]
            WHERE [t0].[TABLE_CATALOG] = @Name"

                let! result =
                    connection.QueryAsync<string>(sql, dict [ "Name", box database.Name ]) |> Async.AwaitTask

                return result
                       |> Seq.map (fun tableValues ->
                           { Table.Name = tableValues
                             Table.Database = database })
                       |> Seq.toList
            with :? Exception as ex ->
                printf "Error thrown by getTableNamesAsync: %s" ex.Message
                return []
        }

    let getDatabaseNamesAsync (connString: ConnectionStringItem): Async<DbItem list> =
        async {
            try
                use connection = new SqlConnection(connString.Value)

                let! dbNames =
                    select { table "[SYS].[DATABASES]" }
                    |> connection.SelectAsync<SysDatabase>
                    |> Async.AwaitTask

                return dbNames
                       |> Seq.map (fun databaseVal -> { DbItem.Name = databaseVal.Name })
                       |> Seq.toList
            with :? Exception as ex ->
                printf "Error thrown by getDatabaseNamesAsync: %s" ex.Message
                return []
        }
