(*** hide ***)
#I "../../bin"

(**
The SystemTimeZones type provider
==========================

This tutorial shows the use of the SystemTimeZones type provider.

![alt text](img/SystemTimeZonesProvider.png "Intellisense for the system time zones")

*)

#r "FSharp.Management.dll"
open FSharp.Management
open System

(**

Answer to a (question from stackoverflow.com)[http:///q/2961848/1603572].

*)

let utc = DateTime(2010, 6, 2, 16, 37, 19, DateTimeKind.Utc)
let ctsTZ = SystemTimeZones.``(UTC-06:00) Central Time (US & Canada)``
printfn "Central Standard Time: %A" <| TimeZoneInfo.ConvertTimeFromUtc(utc, ctsTZ)
// Central Standard Time: 6/2/2010 11:37:19 AM