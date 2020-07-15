module PoGen.Models

open System

type InformationSchemaTable =
    { Table_Catalog: string
      Table_Schema: string
      Table_Name: string
      Table_Type: string }

type SysDatabase = { Name: string }

type LogMessage =
    | Debug of string
    | Info of string
    | Error of string

type StringError =
    | Missing
    | MustNotBeLongerThan of int
    | DoesntMatchPattern of string

type DomainMessages =
    | MissingConnectionString
    | InvalidConnectionString

type ConnectionName = string

type ConnectionStringValue = string

[<CLIMutable>]
type ConnectionStringItem =
    { Id: int
      Name: string
      Value: string }

[<NoComparison>]
type ConnectionTestState =
    | Pass
    | Fail of string
    | NotStarted

[<Struct>]
[<NoComparison>]
type ConnectionTestResult =
    { State: ConnectionTestState
      Message: string }

type DbItem = { Name: string }

let CreateDbItem (dbVal: string): DbItem = { DbItem.Name = dbVal }

type Table = { Name: string; Database: DbItem }

type Query =
    { Database: DbItem
      Sql: string
      ConnectionString: ConnectionStringItem }

type Language =
    | CSharp
    | VbNet

type GetSetValue =
    | Get
    | Set
    | GetAndSet

type AccessModifier =
    | Public
    | Private
    | Internal
    | Protected

type ValueType =
    { Name: string
      Value: Type
      IsNullable: bool }

type Property =
    { TypeDetail: ValueType
      IsNullable: bool
      ColumnName: string
      Access: AccessModifier
      GetSet: GetSetValue }

type ClassObject =
    { ClassName: string
      Access: AccessModifier
      Properties: Property []
      Language: Language }

type ClassResult = { Raw: string }

type RawRowData =
    { IsNullable: bool
      DataType: string
      ColumnName: string }

type FileOutputPath = { FileName: string; FilePath: string }

let DefaultOutputPath =
    { FileName = String.Empty
      FilePath = @"c:\" }

let EmptyFileOutputPath =
    { FileOutputPath.FileName = String.Empty
      FilePath = String.Empty }

let DefaultLanguages = [ Language.CSharp; Language.VbNet ]

let EmptyConnectionStringItem =
    { ConnectionStringItem.Id = 0
      ConnectionStringItem.Value = String.Empty
      ConnectionStringItem.Name = String.Empty }

type StoreConnectionStringResult =
    | Success
    | Error of string

type PageState =
    | MissingRequiredFields of string
    | ValidForm
    | Init

type FormState =
    | Valid
    | MissingConnStrValue
    | Testing
    | Idle
    | FetchingData
    | InvalidConnectionString


type Model =
    { OutputLocation: FileOutputPath
      Databases: DbItem list
      Languages: Language list
      Tables: Table list
      SelectedDatabase: DbItem option
      SelectedLanguage: Language
      SelectedTables: Table list
      CodeGenPageState: PageState
      ConnectionString: ConnectionStringItem
      CurrentFormState: FormState
      Output: string }
    
let defaultModel () =
        { OutputLocation = DefaultOutputPath
          CurrentFormState = Idle
          Databases = []
          Languages = DefaultLanguages
          Tables = []
          SelectedDatabase = None
          SelectedLanguage = CSharp
          SelectedTables = []
          CodeGenPageState = PageState.Init
          ConnectionString = EmptyConnectionStringItem
          Output = String.Empty }