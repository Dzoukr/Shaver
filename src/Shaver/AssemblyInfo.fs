namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Shaver")>]
[<assembly: AssemblyProductAttribute("Shaver")>]
[<assembly: AssemblyDescriptionAttribute("Lightweight templating library for Suave.io")>]
[<assembly: AssemblyVersionAttribute("1.0.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.0"
