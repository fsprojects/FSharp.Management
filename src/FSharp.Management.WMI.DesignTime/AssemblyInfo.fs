namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Management.WMI.DesignTime")>]
[<assembly: AssemblyProductAttribute("FSharp.Management.WMI.DesignTime")>]
[<assembly: AssemblyDescriptionAttribute("Various type providers for management of the machine.")>]
[<assembly: AssemblyVersionAttribute("0.4.0")>]
[<assembly: AssemblyFileVersionAttribute("0.4.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.0"
    let [<Literal>] InformationalVersion = "0.4.0"
