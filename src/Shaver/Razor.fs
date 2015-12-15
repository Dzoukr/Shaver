module Shaver.Razor

open System
open System.Text.RegularExpressions
open Suave.Types
open Suave.Http
open Suave.Http.Files
open Suave.Utils
open Suave.Razor
open RazorEngine.Templating


let partialEmpty<'a> = 
    fun _ -> 
        async { 
            return String.Empty
        }

let partial<'a> path (model : 'a) =
    fun r ->
        async {
            let templatePath = resolvePath r.runtime.homeDirectory path
            let! writeTime, razorTemplate = loadTemplateCached templatePath
            let cacheKey = writeTime.Ticks.ToString() + "_" + templatePath
            return razorService.RunCompile(razorTemplate, cacheKey, typeof<'a>, model)
        }

let page<'a> code path (model : 'a) = 
    fun r ->
        async {
            let! content = partial path model r
            return! Response.response code (UTF8.bytes content) r
        }
