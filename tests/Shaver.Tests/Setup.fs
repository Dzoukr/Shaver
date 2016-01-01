module Shaver.Tests.Setup

open System
open Suave.Testing
open System.IO
open Shaver
open Suave.Web

let path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
let runWithConfig = runWith { defaultConfig with homeFolder = Some(path) }
Resources.folder <- "."