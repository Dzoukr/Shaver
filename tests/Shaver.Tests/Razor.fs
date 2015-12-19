module Shaver.Tests.Razor

open FsUnit
open NUnit.Framework
open Suave
open Suave.Types
open Suave.Testing
open Shaver
open System.IO
open System.Net
open Shaver.Tests.Setup

type ModelMaster = { Master : string }
type ModelNested = { Nested : string }
type ModelOne = { One : string }
type ModelTwo = { Two : string }

[<Test>]
let ``Single page should compile with model data`` () =
    Razor.singlePage "page.html" { Master = "Hello Razor"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "<h1>Hello Razor</h1>"

[<Test>]
let ``Master page using empties should compile with empty sections`` () =
    [
        ("SectionOne", Razor.empty);
        ("SectionTwo", Razor.empty)
    ] 
    |> Razor.masterPage "masterPage.html" { Master = "Hello Master"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "Hello Master<br/>"
    

[<Test>]
let ``Master page using partials should compile with filled sections`` () =
    [
        ("SectionOne", Razor.partial "partialOne.html" { One = "Hello One" });
        ("SectionTwo", Razor.partial "partialTwo.html" { Two = "Hi Two" })
    ] 
    |> Razor.masterPage "masterPage.html" { Master = "Hello Master"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "Hello Master<h1>Hello One</h1><br/><h1>Hi Two</h1>"

[<Test>]
let ``Master page using nested master should compile with filled sections`` () =
    [    
        ("SectionOne", Razor.partial "partialOne.html" { One = "Hello One" });
        ("SectionTwo",
            [("NestedOne", Razor.partial "partialOne.html" { One = "Hello Nested One" });
                ("NestedTwo", Razor.partial "partialTwo.html" { Two = "Hi Nested Two" })
            ] |> Razor.nested "nestedMasterPage.html" {Nested = "Hi Nested"});
    ]
     
    |> Razor.masterPage "masterPage.html" { Master = "Hello Master"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "Hello Master<h1>Hello One</h1><br/>Hi Nested<h1>Hello Nested One</h1><hr/><h1>Hi Nested Two</h1>"

[<Test>]
let ``Single page should compile localized`` () =
    Shaver.Localization.localizeCulture >> 
    Razor.singlePage "pageLocalized.html" { Master = "Hello Razor"}
    |> runWithConfig
    |> reqResp HttpMethod.POST "/"  "" None None DecompressionMethods.None Localization.setSingleAcceptLanguageHeaders contentString
    |> should equal "<h1>Hello Razor</h1>Value for exact culture"