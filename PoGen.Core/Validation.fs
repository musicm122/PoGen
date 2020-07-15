module PoGen.Validation

open System
open System.Data.Common
open System.Data.SqlClient
open System.IO
open PoGen
open PoGen.Core
open PoGen.Extensions
open PoGen.Models

let HasAConnectionString (model: Model): bool =
    model.ConnectionString.Value.IsNullOrWhiteSpace()

let IsConnectionStringInFormat (model: Model): bool =
    try
        SqlConnectionStringBuilder(model.ConnectionString.Value)
        |> ignore
        true
    with :? Exception -> false

let HasRequiredSaveFields (conStr: ConnectionStringItem): bool =
    conStr.Name.IsNullOrWhiteSpace()
    && conStr.Value.IsNullOrWhiteSpace()

let IsValidPath (path: FileOutputPath) =
    try
        let dir = DirectoryInfo(path.FilePath)
        dir.Exists
    with
    | :? ArgumentException -> false
    | :? PathTooLongException -> false
    | :? NotSupportedException -> false
    | _ -> false

let hasADatabaseSelected (m: Model): PageState =
    match m.SelectedDatabase.IsSome with
    | false -> MissingRequiredFields "Selected database is required"
    | _ -> ValidForm

let hasAtLeastOneTableSelected (m: Model): PageState =
    match m.SelectedTables |> List.isEmpty with
    | true -> ValidForm
    | _ -> MissingRequiredFields "At least one table is required"

let hasValidOutputFolder (m: Model): PageState =
    match IsValidPath m.OutputLocation with
    | true -> ValidForm
    | _ -> MissingRequiredFields "Invalid output path"

let hasRequiredFields = hasADatabaseSelected
