module PoGen.Core

open System
open System.Runtime.CompilerServices

module Say =
    let hello name = sprintf "Hello, %s" name

module Extensions =
    [<Extension>]
    type StringExtensions =

        [<Extension>]
        static member inline IsNullOrEmpty(_str: string) = String.IsNullOrEmpty(_str)

        [<Extension>]
        static member inline IsNullOrWhiteSpace(_str: string) = String.IsNullOrWhiteSpace(_str)
