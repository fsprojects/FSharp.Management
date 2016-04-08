module FSharp.Management.Tests.SystemTimeZonesProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnitTyped
open System

[<Test>]
let ``SystemTimeZones provider has correct UTC zone``() =
    SystemTimeZones.``(UTC) Coordinated Universal Time``
    |> shouldEqual <| TimeZoneInfo.FindSystemTimeZoneById "UTC"


