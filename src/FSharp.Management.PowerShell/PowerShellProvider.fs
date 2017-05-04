namespace FSharp.Management.PowerShellProvider

open System.Reflection
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open FSharp.Management.Helper

[<TypeProvider>]
type public PowerShellProvider(_cfg: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    // Get the assembly and namespace used to host the provided types
    let baseTy = typeof<obj>
    let staticParams = [ProvidedStaticParameter("Modules", typeof<string>, parameterDefaultValue = "")
                        ProvidedStaticParameter("PSSnapIns", typeof<string>, parameterDefaultValue = "")
                        ProvidedStaticParameter("Is64BitRequired", typeof<bool>, parameterDefaultValue = false)]

    // Expose all available cmdlets as methods
    let shell = ProvidedTypeDefinition(thisAssembly, rootNamespace, "PowerShellProvider", Some(baseTy))
    let helpText =
        """<summary>Typed representation of a PowerShell runspace</summary>
            <param name='Modules'>List of Modules that will be loaded at the start (separated by semicolon).</param>
            <param name='PSSnapIns'>List of PSSnapIn that will be added at the start (separated by semicolon).</param>
            <param name='Is64BitRequired'>Mark that 64bit runtime should be used for PowerShell</param>"""
    do shell.AddXmlDoc helpText
    do shell.DefineStaticParameters(
        parameters=staticParams,
        instantiationFunction=(fun typeName parameterValues ->
            let modules   = parameterValues.[0] :?> string
            let psSnapIns = parameterValues.[1] :?> string
            let is64bitRequired = parameterValues.[2] :?> bool
            
            let createMembers areStatic = [
                let runtime = Runtime.Current(psSnapIns.Split(';'), modules.Split(';'), is64bitRequired, true)
                for cmdlet in runtime.AllCommands() do
                    let paramList =
                        [for (name, isMandatory, ty) in cmdlet.ParametersInfo ->
                            let newTy, defValue =
                                match ty with
                                | _ when ty = typeof<System.Management.Automation.SwitchParameter> ->
                                    typeof<bool>, box false
                                | _ when ty.IsValueType ->
                                    ty, box None //System.Activator.CreateInstance(ty)
                                | _ -> ty, null
                            match isMandatory with
                            | true  -> ProvidedParameter(TypeInference.toCamelCase name, newTy)
                            | false -> ProvidedParameter(TypeInference.toCamelCase name, newTy, optionalValue=defValue)]
                    let providedMethod =
                        ProvidedMethod(
                            methodName = cmdlet.Name,
                            parameters = paramList,
                            returnType = cmdlet.ResultType,
                            IsStaticMethod = areStatic,
                            InvokeCode =
                                fun args ->
                                    let mapToParameters (args:Quotations.Expr list) = 
                                        if args.Length <> paramList.Length then
                                            failwithf "Expected %d arguments but received %d" paramList.Length args.Length

                                        let elements = [ for i in 0..(paramList.Length-1) ->
                                                            Quotations.Expr.Coerce(args.[i], typeof<obj>) ]
                                        Quotations.Expr.NewArray(typeof<obj>, elements)
                                        
                                    let uid = cmdlet.UniqueID
                                    if areStatic then 
                                        let parameters = mapToParameters args
                                        <@@ Runtime.Current(psSnapIns.Split(';'), modules.Split(';'), is64bitRequired, false)
                                                .Execute(uid, (%%parameters : obj[])) @@>
                                    else 
                                        let constructorExpression, args = 
                                            match args with
                                            | head :: tail -> head, tail
                                            | [] -> failwith "Constructor should create runspace"

                                        let runspace = Quotations.Expr.Coerce(constructorExpression, typeof<HostedRuntime.IPSRuntime>) 
                                        let parameters = mapToParameters args
                                        <@@ (%%runspace : HostedRuntime.IPSRuntime).Execute(uid, (%%parameters : obj[])) @@>)
                                    

                    providedMethod.AddXmlDocDelayed(fun() ->runtime.GetXmlDoc(cmdlet.Name))
                    yield providedMethod :> MemberInfo
                ]

            let pty = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseTy))
            pty.AddMembersDelayed(fun() -> createMembers true)

            let customRunspace = ProvidedTypeDefinition("CustomRunspace", Some(typeof<obj>))
            customRunspace.AddMember <| ProvidedConstructor([], InvokeCode = fun args -> <@@ new HostedRuntime.PSRuntimeHosted(psSnapIns.Split(';'), modules.Split(';')) :> HostedRuntime.IPSRuntime @@>)
            customRunspace.AddInterfaceImplementation <| typeof<System.IDisposable>
            customRunspace.AddMemberDelayed <| fun _ -> ProvidedMethod("Dispose", [], typeof<unit>, 
                                                            InvokeCode = 
                                                                fun args -> 
                                                                let runspace = Quotations.Expr.Coerce(args |> Seq.head, typeof<HostedRuntime.IPSRuntime>) 
                                                                <@@ (%%runspace : System.IDisposable).Dispose() @@>)
            customRunspace.AddMemberDelayed <| fun _ -> ProvidedProperty("Runspace", typeof<System.Management.Automation.Runspaces.Runspace>, 
                                                            GetterCode = 
                                                                fun args -> 
                                                                let runspace = Quotations.Expr.Coerce(args |> Seq.head, typeof<HostedRuntime.IPSRuntime>) 
                                                                <@@ (%%runspace : HostedRuntime.IPSRuntime).Runspace @@>)
            customRunspace.AddMembersDelayed <| fun _ -> createMembers false         
            
            pty.AddMember(customRunspace)

            pty))

    do this.AddNamespace(rootNamespace, [ shell ])
    do this.Disposing.Add(fun _ -> Runtime.disposeAll())

[<TypeProviderAssembly>]
do()