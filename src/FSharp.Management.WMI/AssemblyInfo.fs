namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Management.WMI")>]
[<assembly: AssemblyProductAttribute("FSharp.Management.WMI")>]
[<assembly: AssemblyDescriptionAttribute("Various type providers for management of the machine.")>]
[<assembly: AssemblyVersionAttribute("0.0.6")>]
[<assembly: AssemblyFileVersionAttribute("0.0.6")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.6"
