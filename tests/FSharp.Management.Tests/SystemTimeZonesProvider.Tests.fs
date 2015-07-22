module FSharp.Management.Tests.SystemTimeZonesProviderTests

open FSharp.Management
open NUnit.Framework
open FsUnit
open System

[<Test>] 
let ``SystemTimeZones provider has correct UTC zone``() = 
    SystemTimeZones.``(UTC) Coordinated Universal Time``
      |> should equal <| TimeZoneInfo.FindSystemTimeZoneById "UTC" 


