module Shaver.Resources

open System
open System.Collections.Generic
open System.IO
open System.Globalization
open FSharp.Data


/// Root folder for localized .json files
let mutable folder = "strings/"

let private cache = new Dictionary<_, _>()

let private memoize f =    
    (fun x -> 
        let succ, v = cache.TryGetValue(x)
        if succ then v else 
            let v = f(x) 
            cache.Add(x, v)
            v)

let private categorizeFile file =
    let parts = FileInfo(file).Name.Split('.');
    match parts.Length with
    | 3 -> (parts.[0], Some(new CultureInfo(parts.[1])), JsonValue.Load(file))
    | 2 -> (parts.[0], None, JsonValue.Load(file))
    | _ -> failwith (sprintf "Unexpected file name '%s' for globalization file" <| file)

let private scanFiles folder =
    Directory.GetFiles(folder, "*.json")
    |> Array.map categorizeFile

let private filterByResource resource tuple =
    let (res,_,_) = tuple
    res = resource

let private filterByCultureStrict (culture:CultureInfo option) tuple =
    let (_,cult,_) = tuple
    cult = culture

let private filterByCultureISO (culture:CultureInfo option) tuple =
    let (_,(cult:CultureInfo option),_) = tuple
    cult.IsSome 
        && culture.IsSome 
        && cult.Value.TwoLetterISOLanguageName = culture.Value.TwoLetterISOLanguageName

let private getValueFromJson (json:JsonValue) value =
    match json.TryGetProperty(value) with
    | Some(v) -> Some(v.AsString())
    | None -> None

let private getClosest culture tuples =
    let stricts = tuples |> Array.filter (filterByCultureStrict culture)

    match stricts.Length with
    | 1 -> Some(stricts.[0])
    | _ -> 
        let isos = tuples |> Array.filter (filterByCultureISO culture)
        match isos.Length > 0 with
        | true -> Some(isos.[0])
        | false -> 
            let invariant = tuples |> Array.filter (filterByCultureStrict None)
            match invariant.Length > 0 with
            | true -> Some(invariant.[0])
            | false when tuples.Length > 0 -> Some(tuples.[0])
            | false -> None

    
let getValue (resource:string) (value:string) (culture:CultureInfo option) =
    let langFiles = 
        memoize scanFiles folder
        |> Array.filter (filterByResource resource)
    
    match langFiles.Length with
    | 0 -> None
    | 1 -> 
        let (_,_,json) = langFiles.[0]
        getValueFromJson json value 
    | _ ->
        match getClosest culture langFiles with
        | Some (_,_,json) -> getValueFromJson json value
        | None -> None
    
   