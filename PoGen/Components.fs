namespace PoGen

open Avalonia
open Avalonia.Controls
open Avalonia.Dialogs
open Avalonia.Input
open Avalonia.FuncUI
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish
open Avalonia.Layout

open PoGen.Models
open PoGen.Messages

module Components =

    let getAlertDialog(title:string) (text:string) (parentWindow:Window) =                
        let dialog = AboutAvaloniaDialog()
        dialog.Title<-title
        dialog.Content<- TextBlock.create[TextBlock.text text]
        dialog.ShowDialog(parentWindow)|> Async.AwaitTask
        dialog
        
    let textEntry text textChanged =
        TextBox.create
            [ TextBox.text text
              TextBox.onTextChanged textChanged ]

    let formLabel text =
        TextBlock.create [ TextBlock.text text ]

    let formButton (text: string) cmd (dock: Dock) (isEnabled: bool) =
        Button.create
            [ Button.dock dock
              Button.onClick cmd
              Button.content text
              Button.isEnabled (isEnabled) ]

    let formButtonEnabled (text: string) cmd dock =
        formButton text cmd dock true

    let formButtonDisabled (text: string) dock =
        Button.create
            [ Button.dock dock
              Button.content text
              Button.isEnabled (false) ]

    let verticalStack children =
        StackPanel.create
            [ StackPanel.verticalAlignment VerticalAlignment.Top
              StackPanel.horizontalAlignment HorizontalAlignment.Center
              StackPanel.orientation Orientation.Vertical
              StackPanel.dock Dock.Top
              StackPanel.children children ]

    let horizontalStack children =
        StackPanel.create
            [ StackPanel.verticalAlignment VerticalAlignment.Top
              StackPanel.horizontalAlignment HorizontalAlignment.Center
              StackPanel.orientation Orientation.Vertical
              StackPanel.dock Dock.Top
              StackPanel.children children ]

    let progressBar valueNum isEnabled =
        ProgressBar.create
            [ ProgressBar.value valueNum
              ProgressBar.isIndeterminate true
              ProgressBar.isEnabled isEnabled ]

    let connectionTestView (model: Model) (dispatch) =
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
