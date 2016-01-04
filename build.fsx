// include Fake lib
#I "packages/FAKE/tools/"
#r "FakeLib.dll"

open System
open System.IO
open Fake 
open Fake.AssemblyInfoFile

// Directories
let buildAppDir = "./build/app/"
let buildTestDir = "./build/tests/"
let appSrcDir = "./src/Shaver/"
let testSrcDir = "./tests/Shaver.Tests/"
let nugetBinDir = "./nuget/bin/"

// --------------------------------------------------------------------------------------
// Information about the project to be used at NuGet and in AssemblyInfo files
// --------------------------------------------------------------------------------------

let project = "Shaver"
let title = "Shaver"
let authors = ["Roman Provaznik"]
let summary = "Lightweight F# library for Suave.io web server using Razor engine adding some extra features like template composing, custom return codes or localization resources support."
let description = """Shaver is lightweight F# library for Suave.io web server built on the top of the Razor 
Engine and provides some extra features like template composing, setting custom return codes, localization 
resources support or server thread auto-localization by client Accept-Language header."""
let tags = "F# fsharp suave razor templating http web localization"

// Read release notes & version info from RELEASE_NOTES.md
let release = File.ReadLines "RELEASE_NOTES.md" |> ReleaseNotesHelper.parseReleaseNotes



// Targets
Target "?" (fun _ ->
    printfn " *********************************************************"
    printfn " *        Avaliable options (call 'build <Target>')      *"
    printfn " *********************************************************"
    printfn " [Build]"
    printfn "  > BuildApp"
    printfn "  > BuildTests"
    printfn "  > BuildWithTests"
    printfn " "
    printfn " [Release]"
    printfn "  > Nuget"
    printfn " "
    printfn " [Help]"
    printfn "  > ?"
    printfn " "
    printfn " *********************************************************"
)

Target "Nuget" <| fun () ->
    
    CreateDir nugetBinDir
    let nugetFiles = ["Shaver.dll";"Shaver.xml";"Shaver.pdb"]
    nugetFiles |> List.map (fun f -> buildAppDir + f) |> CopyFiles nugetBinDir
    
    // Format the release notes
    let releaseNotes = release.Notes |> String.concat "\n"
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = project
            Summary = summary
            Description = description
            Version = release.NugetVersion
            ReleaseNotes = releaseNotes
            Tags = tags
            OutputPath = nugetBinDir
            AccessKey = getBuildParamOrDefault "key" ""
            Publish = hasBuildParam "publish"
            References = ["Shaver.dll"]
            Files = nugetFiles |> List.map (fun f -> ("bin/" + f, Some("lib/net45"), None))
            Dependencies =
            [
                "Suave", GetPackageVersion ("./packages") "Suave"
                "RazorEngine", GetPackageVersion ("./packages") "RazorEngine"
                "FSharp.Data", GetPackageVersion ("./packages") "FSharp.Data"
                "FSharp.Core", GetPackageVersion ("./packages") "FSharp.Core"
            ]
        })
        "nuget/Shaver.nuspec"

Target "AssemblyInfo" <| fun () ->
    for file in !! (appSrcDir + "AssemblyInfo*.fs") do
        let version = release.AssemblyVersion
        CreateFSharpAssemblyInfo file
           [ Attribute.Title title
             Attribute.Product project
             Attribute.Description summary
             Attribute.Version version
             Attribute.FileVersion version]

Target "CleanApp" (fun _ ->
    CleanDir buildAppDir
)

Target "CleanTests" (fun _ ->
    CleanDir buildAppDir
)

Target "CleanNugetBin" (fun _ ->
    CleanDir nugetBinDir
)

Target "BuildApp" (fun _ ->
   !! (appSrcDir + "**/*.fsproj")
     |> MSBuildRelease buildAppDir "Build"
     |> Log "AppBuild-Output: "
)

Target "BuildTests" (fun _ ->
    !! (testSrcDir + "**/*.fsproj")
      |> MSBuildDebug buildTestDir "Build"
      |> Log "TestBuild-Output: "
)

Target "BuildWithTests" (fun _ ->
    !! (buildTestDir + "/Shaver.Tests.dll")
      |> NUnit (fun p ->
          {p with
             DisableShadowCopy = true;
             OutputFile = buildTestDir + "TestResults.xml" })
)


// Dependencies
"CleanApp" ==> "AssemblyInfo" ==> "BuildApp"
"CleanTests" ==> "BuildTests"
"BuildApp"  ==> "BuildTests"  ==> "BuildWithTests"
"CleanNugetBin" ==> "BuildApp" ==> "Nuget"

// start build
RunTargetOrDefault "?"