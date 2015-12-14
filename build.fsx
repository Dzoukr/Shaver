// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"
open Fake

// Directories
let buildDir = "./build/"
let projectDir = "./src/TestFx/"
let debugDir = projectDir + "bin/Debug/"
let wwwBuildDir = projectDir + ".wwwbuild/"

// Targets
Target "?" (fun _ ->
    printfn ""
    printfn "  Please specify the target by calling 'build <Target>'"
    printfn ""
    printfn "  Building:"
    printfn "  * TODO"
    printfn ""
    printfn "  Releasing:"
    printfn "  * Nuget"
    printfn ""
)

// start build
RunTargetOrDefault "?"