module Shaver.Tests.Resources

open FsUnit
open NUnit.Framework
open System.Globalization

[<Test>]
let ``Value from default resource should be read`` () =
    Shaver.Resources.folder <- "./"
    Shaver.Resources.getValue "All" "Value" None
    |> should equal (Some "Value for non-specified culture")

[<Test>]
let ``Value from default resource for non-existent culture should be read`` () =
    Shaver.Resources.folder <- "./"
    Shaver.Resources.getValue "All" "Value" (Some (new CultureInfo("fr")))
    |> should equal (Some "Value for non-specified culture")

[<Test>]
let ``Value from two letter ISO resource should be read`` () =
    Shaver.Resources.folder <- "./"
    Shaver.Resources.getValue "All" "Value" (Some (new CultureInfo("cs")))
    |> should equal (Some "Value for two letter ISO culture")

[<Test>]
let ``Value from exact resource should be read`` () =
    Shaver.Resources.folder <- "./"
    Shaver.Resources.getValue "All" "Value" (Some (new CultureInfo("cs-CZ")))
    |> should equal (Some "Value for exact culture")

[<Test>]
let ``Value from the nearest resource should be read`` () =
    Shaver.Resources.folder <- "./"
    Shaver.Resources.getValue "Some" "Value" (Some (new CultureInfo("cs")))
    |> should equal (Some "Value for the nearest culture")