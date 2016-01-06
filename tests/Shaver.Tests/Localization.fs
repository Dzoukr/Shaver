module Shaver.Tests.Localization

open FsUnit
open NUnit.Framework
open Suave
open Suave.Http
open Suave.Testing
open System.Net.Http.Headers
open System.Net.Http
open System.Net
open Shaver.Tests.Setup
open Suave.Successful
open System.Globalization

let setWrongAcceptedLanguage (r : HttpRequestMessage) =  
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("null"))
    r

let setSingleAcceptLanguageHeaders lang (r : HttpRequestMessage)  =
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(lang))
    r

let private setMoreAcceptLanguageHeaders (r : HttpRequestMessage) =
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("cs-CZ"))
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"))
    r.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru"))
    r

[<Test>]
let ``List of accepted languages should be parsed from context`` () =
    let ``assert`` x =
        Shaver.Localization.getAcceptedLanguages(x.request) 
        |> should equal (Some [|"cs-CZ"; "en"; "ru"|])
        x
    ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.GET "/"  "" None None DecompressionMethods.None setMoreAcceptLanguageHeaders contentString
    |> ignore


[<Test>]
let ``Calling localizeUICulture should change thread UI culture`` () =
    let ``assert`` x =
        System.Threading.Thread.CurrentThread.CurrentUICulture.Name |> should equal "jp-JP"
        x
    
    Shaver.Localization.localizeUICulture >> ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.GET "/"  "" None None DecompressionMethods.None (setSingleAcceptLanguageHeaders "jp-JP") contentString
    |> ignore
    

[<Test>]
let ``Calling localizeCulture should change thread culture`` () =
    let ``assert`` x = 
        System.Threading.Thread.CurrentThread.CurrentCulture.Name |> should equal "ru-RU"
        x
    
    Shaver.Localization.localizeCulture >> ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.GET "/"  "" None None DecompressionMethods.None (setSingleAcceptLanguageHeaders "ru-RU") contentString
    |> ignore

[<Test>]
let ``Calling localizeCulture with wrong header should not change anything`` () =
    let mutable currentCulture = CultureInfo.InvariantCulture
    
    let storeCurrentCulture x =
        currentCulture <- System.Threading.Thread.CurrentThread.CurrentCulture
        x

    let ``assert`` x = 
        System.Threading.Thread.CurrentThread.CurrentCulture.Name |> should equal currentCulture.Name
        x
    
    storeCurrentCulture >> Shaver.Localization.localizeCulture >> ``assert`` >> (OK "Hi")
    |> runWithConfig
    |> reqResp HttpMethod.GET "/"  "" None None DecompressionMethods.None setWrongAcceptedLanguage contentString
    |> ignore