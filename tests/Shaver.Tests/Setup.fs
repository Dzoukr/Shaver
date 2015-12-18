module Shaver.Tests.Setup

open System
open Suave.Testing
open System.IO
open Shaver
open Suave.Web

let path = Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).AbsolutePath.Replace("/","\\")
let runWithConfig = runWith { defaultConfig with homeFolder = Some(path) }
Resources.folder <- "."