module PoGen.Messages

open PoGen.Models

type Msg =
    | UpdateConnectionStringValue of string
    | UpdateConnectionStringName of string
    | TestConnection
    | TestConnectionComplete 
    | SetSelectedDatabase of string option
    | SetSelectedDatabaseComplete
    | SetSelectedLanguage of Language
    | SetSelectedConnection of ConnectionStringItem
    | BrowseForOutputFolder of FileOutputPath
    | GenerateCode
    | FetchTables of DbItem
    | FetchTablesComplete of Table list
    | FetchDatabases
    | FetchDatabasesComplete of DbItem list
