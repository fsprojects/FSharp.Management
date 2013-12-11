namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharpx.ManagementProviders")>]
[<assembly: AssemblyProductAttribute("FSharpx.ManagementProviders")>]
[<assembly: AssemblyDescriptionAttribute("Various type providers for management of the machine.")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
