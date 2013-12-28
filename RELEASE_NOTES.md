#### 0.1.0-alpha1 - 24.12.2013
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