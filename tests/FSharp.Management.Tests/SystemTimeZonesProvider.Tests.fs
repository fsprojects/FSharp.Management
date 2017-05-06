module FSharp.Management.Tests.SystemTimeZonesProviderTests

open FSharp.Management
open Expecto
open System

let [<Tests>] timezonesTest =
    testCase "SystemTimeZones provider has correct UTC zone" <| fun _ ->
        let expect = TimeZoneInfo.FindSystemTimeZoneById "UTC"
        let actual = SystemTimeZones.``(UTC) Coordinated Universal Time``
        Expect.equal actual expect ""


