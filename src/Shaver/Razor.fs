module Shaver.Razor

open System
open System.Text.RegularExpressions
open Suave.Utils.AsyncExtensions
open System.Threading
open Suave
open System.IO
open RazorEngine.Configuration
open RazorEngine.Templating

/// Opening tag, default value = {{{
let mutable openTag = "{{{"

/// Closing tag, default value = }}}
let mutable closeTag = "}}}"

let private getPartialKey name = sprintf "%s%s%s" openTag name closeTag
let private getResourceKey name value = sprintf "%s$:%s:%s%s" openTag name value closeTag

let private replaceResources s =
    let regexString = (getResourceKey "(.+)" "(.+)").Replace("$","\$")
    let regex = new Regex(regexString, RegexOptions.Multiline)
    let mutable result = s
    for regMatch in regex.Matches(s) do
        match regMatch.Success with
        | true -> 
            let name = regMatch.Groups.[1].Value
            let value = regMatch.Groups.[2].Value
            match Resources.getValue name value (Some(Thread.CurrentThread.CurrentUICulture)) with
            | Some(replaceValue) -> 
                let replaceKey = getResourceKey name value
                result <- result.Replace(replaceKey, replaceValue.ToString())
            | None -> ()
        | false -> ()
    result

let private loadTemplate templatePath =
    async {
      let writeTime = File.GetLastWriteTime(templatePath)
      use file = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
      use reader = new StreamReader(file)
      let! razorTemplate = reader.ReadToEndAsync()
      return writeTime, razorTemplate
    }

let private cachingProvider = new InvalidatingCachingProvider(fun t -> ())

/// Renders partial content as empty string
let empty<'a> = 
    fun _ -> 
        async { 
            return String.Empty
        }

/// Renders partial content
let partial<'a> (path:string) (model : 'a) =
    fun r ->
        async {
            let serviceConfiguration = TemplateServiceConfiguration()
            serviceConfiguration.DisableTempFileLocking <- true
            serviceConfiguration.CachingProvider <- cachingProvider
            serviceConfiguration.TemplateManager <-
              { new ITemplateManager with
                  member x.AddDynamic(_, _) = failwith "not implemented"
                  member x.Resolve key =
                    let _, readTemplate = loadTemplate key.Name |> Async.RunSynchronously
                    LoadedTemplateSource(readTemplate, key.Name) :> _
                  member x.GetKey (name, resolveType, context) =
                    NameOnlyTemplateKey(name, resolveType, context) :> ITemplateKey }

            let razorService = RazorEngineService.Create(serviceConfiguration)
            return razorService.RunCompile(path, typeof<'a>, model) |> replaceResources
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
let singlePageWithCode<'a> code path (model : 'a) = 
    fun r ->
        async {
            let! content = partial path model r
            return! Response.response code (System.Text.Encoding.UTF8.GetBytes content) r
        }

/// Renders webpart with defined template and model
let singlePage<'a> = singlePageWithCode<'a> HTTP_200

/// Renders webpart with defined HttpCode, template and model using partial renders
let masterPageWithCode<'a> code path (model:'a) (partials:(string * 'b) list)  =
    fun r ->
        async {
            let mutable content = String.Empty
            let! templateContent = partial path model r
            content <- templateContent
            for (key, part) in partials do
                let! partContent = part r
                content <- content.Replace(getPartialKey(key), partContent)
                ()
            return! Response.response code (System.Text.Encoding.UTF8.GetBytes content) r
        }

/// Renders webpart with defined template and model using partial renders
let masterPage<'a> = masterPageWithCode<'a> HTTP_200