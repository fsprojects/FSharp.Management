module FSharp.Management.NamespaceProvider

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Text.RegularExpressions
open FSharp.Management.Helper

[<TypeProvider>]
/// [omit]
type public RegistrySystemProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()
    
    do this.AddNamespace(rootNamespace, [RegistryProvider.createTypedRegistry()])

[<TypeProvider>]
/// [omit]
type public FileSystemProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()
    let ctx = new Context(this.Invalidate)
    
    do
        this.Disposing.Add(fun _ -> (ctx :> IDisposable).Dispose())
        this.AddNamespace(rootNamespace, [FilesTypeProvider.createTypedFileSystem ctx])

[<TypeProvider>]
/// [omit]
type public RelativeFileSystemProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()
    let ctx = new Context(this.Invalidate)
        
    do
        this.Disposing.Add(fun _ -> (ctx :> IDisposable).Dispose())
        this.AddNamespace(rootNamespace, [FilesTypeProvider.createRelativePathSystem cfg.ResolutionFolder ctx])

[<assembly:TypeProviderAssembly()>]
do ()