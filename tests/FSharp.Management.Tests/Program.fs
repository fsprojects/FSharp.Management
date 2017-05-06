module Program

open Expecto

[<EntryPoint>]
let main args =
    let config =
        { defaultConfig with
            ``parallel`` = false
            verbosity = Logging.LogLevel.Verbose }
    runTestsInAssembly config args