module FSharp.Management.Tests.RegistryProviderTests

open FSharp.Management
open Expecto

let [<Tests>] registryProviderTests =
    testList "RegistryProvider Tests" [
        test "Can create type for HKEY_CURRENT_USER" {
            Expect.equal Registry.HKEY_CURRENT_USER.Path
                         "HKEY_CURRENT_USER" ""
        }
        test "Can create subtype for HKEY_LOCAL_MACHINE" {
            Expect.equal Registry.HKEY_LOCAL_MACHINE.SOFTWARE.Path
                         @"HKEY_LOCAL_MACHINE\SOFTWARE" ""
        }
    ]