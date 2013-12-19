namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Management.PowerShell.ExternalRuntime")>]
[<assembly: AssemblyProductAttribute("FSharp.Management.PowerShell.ExternalRuntime")>]
[<assembly: AssemblyDescriptionAttribute("Various type providers for management of the machine.")>]
[<assembly: AssemblyVersionAttribute("0.0.4")>]
[<assembly: AssemblyFileVersionAttribute("0.0.4")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.4"
