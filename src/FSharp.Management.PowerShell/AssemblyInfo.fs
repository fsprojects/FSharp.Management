namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Management.PowerShell")>]
[<assembly: AssemblyProductAttribute("FSharp.Management.PowerShell")>]
[<assembly: AssemblyDescriptionAttribute("Various type providers for management of the machine.")>]
[<assembly: AssemblyVersionAttribute("0.0.13")>]
[<assembly: AssemblyFileVersionAttribute("0.0.13")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.13"
