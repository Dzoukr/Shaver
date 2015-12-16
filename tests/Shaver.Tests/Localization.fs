module Localization

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
open System.Net.Http.Headers
open System.Net.Http
open Shaver.Tests.Razor
open System
open System.Net


let private setSingleAcceptLanguageHeaders (r : HttpRequestMessage) =
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("cs-CZ"))
    r

let private setMoreAcceptLanguageHeaders (r : HttpRequestMessage) =
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("cs-CZ"))
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"))
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru"))
    r

[<Test>]
let ``List of accepted languages should be parsed from context`` () =
    let ``assert`` = fun (x:HttpContext) -> 
        Shaver.Localization.getAcceptedLanguages(x.request) 
        |> should equal (Some [|"cs-CZ"; "en"; "ru"|])
        x
    ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.GET "/"  "" None None DecompressionMethods.None setMoreAcceptLanguageHeaders contentString
    |> ignore


[<Test>]
let ``Calling localizeUICulture should change thread UI culture`` () =
    let ``assert`` = fun (x) -> 
        System.Threading.Thread.CurrentThread.CurrentUICulture.Name |> should equal "cs-CZ"
        x
    
    Shaver.Localization.localizeUICulture >> ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.GET "/"  "" None None DecompressionMethods.None setSingleAcceptLanguageHeaders contentString
    |> ignore
    

[<Test>]
let ``Calling localizeCulture should change thread culture`` () =
    let ``assert`` = fun (x) -> 
        System.Threading.Thread.CurrentThread.CurrentCulture.Name |> should equal "cs-CZ"
        x
    
    Shaver.Localization.localizeCulture >> ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.POST "/"  "" None None DecompressionMethods.None setSingleAcceptLanguageHeaders contentString
    |> ignore

