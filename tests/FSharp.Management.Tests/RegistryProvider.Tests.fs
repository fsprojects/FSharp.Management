module FSharp.Management.Tests.RegistryProviderTests
#if false

open FSharp.Management
open Expecto

let [<Tests>] registryProviderTests =
    testList "RegistryProvider Tests" [
        test "Can create type for HKEY_CURRENT_USER" {
            Expect.equal Registry.HKEY_CURRENT_USER.Path
                         "HKEY_CURRENT_USER" ""
        }
        test "Can create subtype for HKEY_LOCAL_MACHINE" {
            // ensure it exists first (xplat stuff)
            if Microsoft.Win32.Registry.LocalMachine.GetValue "SOFTWARE" = null
            then Microsoft.Win32.Registry.LocalMachine.SetValue("SOFTWARE" , "", Microsoft.Win32.RegistryValueKind.String)

            Expect.equal Registry.HKEY_LOCAL_MACHINE.SOFTWARE.Path
                         @"HKEY_LOCAL_MACHINE\SOFTWARE" ""
        }
    ]
#endif