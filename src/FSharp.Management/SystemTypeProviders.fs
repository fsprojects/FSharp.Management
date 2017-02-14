module FSharp.Management.NamespaceProvider

open System
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open FSharp.Management.Helper

[<TypeProvider>]
/// [omit]
type public RegistrySystemProvider(_cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
    do this.AddNamespace(rootNamespace, [ RegistryProvider.createTypedRegistry() ])

[<TypeProvider>]
/// [omit]
type public FileSystemProvider(_cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
    let ctx = new Context(this.Invalidate)
    do
        this.Disposing.Add(fun _ -> (ctx :> IDisposable).Dispose())
        this.AddNamespace(rootNamespace, [FilesTypeProvider.createTypedFileSystem ctx])

[<TypeProvider>]
/// [omit]
type public RelativeFileSystemProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
    let ctx = new Context(this.Invalidate)
        
    do
        this.Disposing.Add(fun _ -> (ctx :> IDisposable).Dispose())
        this.AddNamespace(rootNamespace, [FilesTypeProvider.createRelativePathSystem cfg.ResolutionFolder ctx])


[<TypeProvider>]
/// [omit]
type public SystemTimeZonesProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
        
    do
        let root = erasedType<obj> thisAssembly rootNamespace "SystemTimeZones"
        root.AddMembersDelayed <| fun() -> 
            [ 
                for x in TimeZoneInfo.GetSystemTimeZones() ->
                    let id = x.Id
                    ProvidedProperty(x.DisplayName, typeof<TimeZoneInfo>, [], IsStatic = true, GetterCode = fun _ -> <@@ TimeZoneInfo.FindSystemTimeZoneById id @@>)
            ]
        this.AddNamespace(rootNamespace, [root])

[<TypeProvider>]
/// [omit]
type public StringReaderProvider(cfg : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()
    do this.AddNamespace(rootNamespace, [ StringReaderProvider.createTypedStringReader cfg.ResolutionFolder ])

[<TypeProviderAssembly>]
do()