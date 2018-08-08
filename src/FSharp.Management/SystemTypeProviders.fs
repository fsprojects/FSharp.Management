module FSharp.Management.NamespaceProvider

open System
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open FSharp.Management.Helper

#if false
/// this TP is not included because it currently throws on non-windows systems with an exception I haven't been able to track down
[<TypeProvider>]
/// [omit]
type public RegistrySystemProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces(cfg)
    do this.AddNamespace(rootNamespace, [ RegistryProvider.createTypedRegistry() ])
#endif

[<TypeProvider>]
/// [omit]
type public FileSystemProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces(cfg)
    let ctx = new Context(this.Invalidate)
    do
        this.Disposing.Add(fun _ -> (ctx :> IDisposable).Dispose())
        this.AddNamespace(rootNamespace, [FilesTypeProvider.createTypedFileSystem ctx])

[<TypeProvider>]
/// [omit]
type public RelativeFileSystemProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces(cfg)
    let ctx = new Context(this.Invalidate)
        
    do
        this.Disposing.Add(fun _ -> (ctx :> IDisposable).Dispose())
        this.AddNamespace(rootNamespace, [FilesTypeProvider.createRelativePathSystem cfg.ResolutionFolder ctx])


[<TypeProvider>]
/// [omit]
type public SystemTimeZonesProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces(cfg)
        
    do
        let root = erasedType<obj> thisAssembly rootNamespace "SystemTimeZones"
        root.AddMembersDelayed <| fun() -> 
            [   
                for x in TimeZoneInfo.GetSystemTimeZones() |> Seq.distinctBy (fun tz -> tz.StandardName) ->
                    let id = x.Id
                    ProvidedProperty(x.DisplayName, typeof<TimeZoneInfo>, isStatic = true, getterCode = fun _ -> <@ TimeZoneInfo.FindSystemTimeZoneById(id) @>.Raw)
            ]
        this.AddNamespace(rootNamespace, [root])

[<TypeProvider>]
/// [omit]
type public StringReaderProvider(cfg : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(cfg)
    do this.AddNamespace(rootNamespace, [ StringReaderProvider.createTypedStringReader cfg.ResolutionFolder ])

[<TypeProviderAssembly>]
do()