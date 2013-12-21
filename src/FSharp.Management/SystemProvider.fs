module FSharp.Management.NamespaceProvider

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes
open System.Text.RegularExpressions
open FSharp.Management.Helper

[<TypeProvider>]
/// [omit]
type public SystemProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()
    let disposingEvent = Event<_>()
    let ctx = 
        { Disposing = disposingEvent.Publish
          OnChanged = this.Invalidate }
    
    do this.AddNamespace(
        rootNamespace, 
        [FilesTypeProvider.createTypedFileSystem ctx
         FilesTypeProvider.createRelativePathSystem cfg.ResolutionFolder ctx
         RegistryProvider.createTypedRegistry()])

    interface IDisposable with
        member x.Dispose() = disposingEvent.Trigger()

[<assembly:TypeProviderAssembly()>]
do ()