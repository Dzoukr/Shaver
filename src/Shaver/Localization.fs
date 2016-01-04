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

/// Localizes thread culture by first language defined in accept-language header
let localizeCulture = (fun (ctx:HttpContext) -> 
    let langs = getAcceptedLanguages ctx.request
    match langs with
    | Some(l) -> CultureInfo.DefaultThreadCurrentCulture <- new CultureInfo(l.[0])
    | None -> ()
    ctx
    )

/// Localizes thread UI culture by first language defined in accept-language header
let localizeUICulture = (fun (ctx:HttpContext) -> 
    let langs = getAcceptedLanguages ctx.request
    match langs with
    | Some(l) -> CultureInfo.DefaultThreadCurrentUICulture <- new CultureInfo(l.[0])
    | None -> ()
    ctx
    )