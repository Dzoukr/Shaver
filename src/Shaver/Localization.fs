module Shaver.Localization

open Suave
open System.Globalization

/// Gets list of languages defined in accept-language header
let getAcceptedLanguages (request:HttpRequest) =
    let choic = request.header("accept-language")
    match choic with
    | Choice.Choice1Of2(header) ->
        let values = Array.filter (fun (x:string) -> x.Contains("q=") = false ) (header.Split([|',';';'|]))
        match values.Length > 0 with
            | true -> Some(Array.map (fun (x:string) -> x.Trim()) values)
            | false -> None
    | _ -> None

let private tryGetCultureInfo (name:string) =
    try
        let culture = new CultureInfo(name)
        Some(culture)
    with
    | _ -> None

/// Localizes thread culture by first language defined in accept-language header
let localizeCulture = (fun (ctx:HttpContext) -> 
    match getAcceptedLanguages ctx.request with
    | Some(l) -> 
        match tryGetCultureInfo l.[0] with
        | Some(culture) -> CultureInfo.DefaultThreadCurrentCulture <- culture
        | _ -> ()
    | _ -> ()
    ctx
    )

/// Localizes thread UI culture by first language defined in accept-language header
let localizeUICulture = (fun (ctx:HttpContext) -> 
    match getAcceptedLanguages ctx.request with
    | Some(l) -> 
        match tryGetCultureInfo l.[0] with
        | Some(culture) -> CultureInfo.DefaultThreadCurrentUICulture <- culture
        | _ -> ()
    | _ -> ()
    ctx
    )