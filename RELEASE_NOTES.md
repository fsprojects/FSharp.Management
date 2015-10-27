#### 0.3.1 - 28.10.2015
* PowerShell module script support https://github.com/fsprojects/FSharp.Management/pull/62
* PowerShell TypeProviders compiled for .NET 4.5 due to bug in Win10 - https://connect.microsoft.com/VisualStudio/feedback/details/1817986/unable-to-use-system-management-automation-dll-assembly-in-a-net-4-0-application-on-windows-10

#### 0.3.0 - 27.07.2015
* Added SystemTimeZonesProvider

#### 0.2.1  - 07.07.2015
* Added support of loading modules in PowerShellProvider
* Added ability to call Azure PowerShell cmdlets. https://github.com/fsprojects/FSharp.Management/issues/47
* New type inference approach for PS TP and new tests
* Added paket dependency to FSharp.TypeProviders.StarterPack

#### 0.2.0 - 03.01.2015
* Use latest type provider sources
* Use paket

#### 0.1.1 - 25.06.2014
* Fix memory leak - https://github.com/fsprojects/FSharp.Management/pull/42

#### 0.1.0 - 31.12.2013
* Initial release of FileSystemProvider
* Initial release of RegistryProvider
* Initial release of PowershellProvider
* Initial release of WMI provider

#### 0.1.0-alpha2 - 28.12.2013
* Moved to fsprojects - https://github.com/fsprojects/FSharp.Management
* New optional parameter watch in the file system provider allows to configure the FileSystemWatcher

#### 0.1.0-alpha1 - 27.12.2013
* Fixed FileSystem type provider invalidation
* Splitted all type providers
* Change "Parent" folder to ".." in relative type provider
* Updated docs

#### 0.0.13-alpha - 24.12.2013
* Parent property in FileSystem type provider is lazy

#### 0.0.12-alpha - 21.12.2013
* Allows specification of a "root" folder in FileSystem type provider

#### 0.0.11-alpha - 20.12.2013
* Separate project for the powershell provider

#### 0.0.10-alpha - 20.12.2013
* Caching of the WMI provider

#### 0.0.9-alpha - 20.12.2013
* Delay for invalidation of the relative path type provider

#### 0.0.8-alpha - 20.12.2013
* Fixed bug in relative path type provider

#### 0.0.7-alpha - 20.12.2013
* FileSystem type provider invalidates itself whenever any child dirs/files changed in any way

#### 0.0.6-alpha - 19.12.2013
* Relative path type provider returns relative paths

#### 0.0.5-alpha - 19.12.2013
* Unified FileSystem type provider and relative path type provider
* Made the FileSystem type provider robust against access problems

#### 0.0.4-alpha - 19.12.2013 
* Relative path type provider allows to go up
* Using latest provided types API from Sample pack

#### 0.0.3-alpha - 19.12.2013 
* First version of the relative path type provider

#### 0.0.2-alpha - 17.12.2013 
* Include FSharp.Management.PowerShell.ExternalRuntime

#### 0.0.1-alpha - 17.12.2013 
* Initial release of FileSystemProvider
* Initial release of RegistryProvider
* Initial release of PowershellProvider
* Initial release of WMI provider