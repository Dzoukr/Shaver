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

type PageModel = { Message : string}

[<Test>]
let ``Empty partial should compile to empty string`` () =
    let f = async {
        let! content = Razor.partialEmpty null
        (OK content)
        |> runWithConfig
        |> req HttpMethod.GET "/" None
        |> should equal System.String.Empty
    }
    f |> ignore
    
[<Test>]
let ``Single page should compile with model data`` () =
    Razor.page HTTP_200 "page.html" { Message = "Hello Razor"}
    |> runWithConfig
    |> req HttpMethod.GET "/" None
    |> should equal "<h1>Hello Razor</h1>"
    

