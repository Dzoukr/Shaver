module Shaver.Tests.Setup

open System
open Suave
open Suave.Testing
open Shaver

let path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
let runWithConfig = runWith { defaultConfig with homeFolder = Some(path) }
Resources.folder <- "."