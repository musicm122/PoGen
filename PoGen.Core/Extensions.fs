namespace PoGen

open System
open System.Runtime.CompilerServices

module Extensions =
    [<Extension>]
    type StringExtensions =

        [<Extension>]
        static member inline IsNullOrEmpty(_str: string) = String.IsNullOrEmpty(_str)

        [<Extension>]
        static member inline IsNullOrWhiteSpace(_str: string) = String.IsNullOrWhiteSpace(_str)