module Shaver.Razor

open System
open System.Text.RegularExpressions
open Suave.Types
open Suave.Http
open Suave.Http.Files
open Suave.Utils
open Suave.Razor
open RazorEngine.Templating

/// Opening tag, default value = {{{
let mutable openTag = "{{{"

/// Closing tag, default value = }}}
let mutable closeTag = "}}}"

let private getPartialKey name = sprintf "%s%s%s" openTag name closeTag

/// Renders partial content as empty string
let empty<'a> = 
    fun _ -> 
        async { 
            return String.Empty
        }

/// Renders partial content
let partial<'a> path (model : 'a) =
    fun r ->
        async {
            let templatePath = resolvePath r.runtime.homeDirectory path
            let! writeTime, razorTemplate = loadTemplateCached templatePath
            let cacheKey = writeTime.Ticks.ToString() + "_" + templatePath
            return razorService.RunCompile(razorTemplate, cacheKey, typeof<'a>, model)
        }

/// Renders nested content
let nested<'a> path (model:'a) (partials:(string * 'b) list)  =
    fun r ->
        async {
            let mutable content = String.Empty
            let! templateContent = partial path model r
            content <- templateContent
            for (key, part) in partials do
                let! partContent = part r
                content <- content.Replace(getPartialKey(key), partContent)
                ()
            return content
        }

/// Renders webpart with defined HttpCode, template and model
let page<'a> code path (model : 'a) = 
    fun r ->
        async {
            let! content = partial path model r
            return! Response.response code (UTF8.bytes content) r
        }

/// Renders webpart with defined HttpCode, template and model using partial renders
let masterPage<'a> code path (model:'a) (partials:(string * 'b) list)  =
    fun r ->
        async {
            let mutable content = String.Empty
            let! templateContent = partial path model r
            content <- templateContent
            for (key, part) in partials do
                let! partContent = part r
                content <- content.Replace(getPartialKey(key), partContent)
                ()
            return! Response.response code (UTF8.bytes content) r
        }