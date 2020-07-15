module PoGen.CsharpFormatting

open System
open System.Text.RegularExpressions
open System.Data
open System.Collections.Generic

type PropertyValues =
    { TypeName: string
      TypeValue: Type
      IsNullable: bool
      ColumnName: string
      Accessor: string }

let typeLookup name =
    match name with
    | "int" -> typeof<int>
    | "short" -> typeof<int16>
    | "byte" -> typeof<byte>
    | "byte[]" -> typeof<byte []>
    | "long" -> typeof<int64>
    | "double" -> typeof<double>
    | "decimal" -> typeof<decimal>
    | "float" -> typeof<float>
    | "bool" -> typeof<bool>
    | "string" -> typeof<string>
    | _ -> typeof<Object>

let getNullableTypes =
    let nullables = HashSet<Type>()
    nullables.Add(typeof<int>) |> ignore
    nullables.Add(typeof<int16>) |> ignore
    nullables.Add(typeof<double>) |> ignore
    nullables.Add(typeof<decimal>) |> ignore
    nullables.Add(typeof<float>) |> ignore
    nullables.Add(typeof<bool>) |> ignore
    nullables.Add(typeof<DateTime>) |> ignore
    nullables


let formatTypeName (dataType: string): string =
    let replaceSystem (item: string): string = item.Replace("System.", String.Empty)

    let capitalizeFirstLetter (item: string): string =
        match item with
        | s when String.IsNullOrWhiteSpace(s) -> String.Empty
        | _ ->
            let first = item.[0].ToString().ToUpper()
            let rest = item.Substring(1)
            first + rest

    let replaceUnderscoreWithCapitalLetters (item: string): string =
        match item with
        | s when String.IsNullOrWhiteSpace(s) -> item
        | _ ->
            let flags =
                RegexOptions.Compiled ||| RegexOptions.IgnoreCase

            let rx = Regex(@"_([\w\d]){1}", flags)

            let replace (rxMatch: Match) =
                rxMatch.Value.Substring(rxMatch.Value.Length - 1).ToUpper()

            match rx.IsMatch(item) with
            | false -> item
            | true -> rx.Replace(item, MatchEvaluator(replace))

    dataType
    |> replaceSystem
    |> capitalizeFirstLetter
    |> replaceUnderscoreWithCapitalLetters


let buildClass (className: string) (dataRows: DataRow []): string =

    let buildProperty (row: DataRow): PropertyValues =
        let typeNameDetails = row.["DataType"] |> string
        let typeVal = typeLookup (typeNameDetails)

        let isNullableVal =
            row.["AllowDBNull"].ToString() |> bool.Parse

        let isNullable =
            isNullableVal
            && getNullableTypes.Contains(typeVal)

        { TypeName = typeNameDetails
          TypeValue = typeVal
          IsNullable = isNullable
          ColumnName = row.["ColumnName"] |> string
          Accessor = "public" }

    let classDeclaration className =
        "public class " + className + "\r\n" + "{"

    let appendClassTermination classVal = sprintf "%s\r\n{" classVal

    let convertToPropertyString propValues =
        let typeValue =
            if propValues.IsNullable then propValues.TypeName + "?" else propValues.TypeName

        sprintf "%s %s %s  {get;set;}" propValues.Accessor typeValue propValues.ColumnName

    let classBody =
        dataRows
        |> Array.map buildProperty
        |> Array.map convertToPropertyString
        |> Array.fold (+) ""

    let classHeader = className |> classDeclaration

    classHeader + classBody |> appendClassTermination
