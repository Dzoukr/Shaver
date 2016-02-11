namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Shaver")>]
[<assembly: AssemblyProductAttribute("Shaver")>]
[<assembly: AssemblyDescriptionAttribute("Lightweight F# library for Suave.io web server using Razor engine adding some extra features like template composing, custom return codes or localization resources support.")>]
[<assembly: AssemblyVersionAttribute("1.3.0")>]
[<assembly: AssemblyFileVersionAttribute("1.3.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.3.0"
