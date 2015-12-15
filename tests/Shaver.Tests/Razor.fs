module Shaver.Tests.Razor

open FsUnit
open NUnit.Framework
open Suave
open Suave.Http
open Suave.Web
open Suave.Types
open Suave.Http
open Suave.Testing
open Suave.Razor
open Suave.Http.Successful
open Shaver

let path = System.Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).AbsolutePath.Replace("/","\\")
let runWithConfig = runWith { defaultConfig with homeFolder = Some(path) }

type ModelMaster = { Master : string }
type ModelOne = { One : string }
type ModelTwo = { Two : string }

[<Test>]
let ``Single page should compile with model data`` () =
    Razor.page HTTP_200 "page.html" { Master = "Hello Razor"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "<h1>Hello Razor</h1>"

[<Test>]
let ``Master page using empties should compile with empty sections`` () =
    [
        ("SectionOne", Razor.empty);
        ("SectionTwo", Razor.empty)
    ] 
    |> Razor.masterPage HTTP_200 "masterPage.html" { Master = "Hello Master"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "Hello Master<br/>"
    

[<Test>]
let ``Master page using partials should compile with filled sections`` () =
    [
        ("SectionOne", Razor.partial "partialOne.html" { One = "Hello One" });
        ("SectionTwo", Razor.partial "partialTwo.html" { Two = "Hi Two" })
    ] 
    |> Razor.masterPage HTTP_200 "masterPage.html" { Master = "Hello Master"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "Hello Master<h1>Hello One</h1><br/><h1>Hi Two</h1>"