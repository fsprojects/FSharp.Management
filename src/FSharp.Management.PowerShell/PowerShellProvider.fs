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
    let staticParams = [ProvidedStaticParameter("PSSnapIns", typeof<string>); //, parameterDefaultValue = "")] // String list will be much better here
                        ProvidedStaticParameter("Modules", typeof<string>);
                        ProvidedStaticParameter("Is64BitRequired", typeof<bool>);]

    // Expose all available cmdlets as methods
    let shell = ProvidedTypeDefinition(thisAssembly, rootNamespace, "PowerShellProvider", Some(baseTy))
    let helpText =
        """<summary>Typed representation of a PowerShell runspace</summary>
            <param name='PSSnapIns'>List of PSSnapIn that will be added at the start (separated by semicolon).</param>
            <param name='Modules'>List of Modules that will be loaded at the start (separated by semicolon).</param>
            <param name='Is64BitRequired'>Mark that 64bit runtime should be used for PowerShell</param>"""
    do shell.AddXmlDoc helpText
    do shell.DefineStaticParameters(
        staticParameters=staticParams,
        apply=(fun typeName parameterValues ->
            let psSnapIns = parameterValues.[0] :?> string
            let modules   = parameterValues.[1] :?> string
            let is64bitRequired = parameterValues.[2] :?> bool

            let pty = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseTy))
            pty.AddMembersDelayed(fun() ->
                [
                let runtime = Runtime.Current(psSnapIns.Split(';'), modules.Split(';'), is64bitRequired, true)
                for cmdlet in runtime.AllCmdlets() do
                    let paramList =
                        [for (name, isMandatory, ty) in cmdlet.ParametersInfo ->
                            let newTy, defValue =
                                match ty with
                                | _ when ty = typeof<System.Management.Automation.SwitchParameter>->
                                    typeof<bool>, box false
                                | _ when ty.IsValueType ->
                                    ty, box None //System.Activator.CreateInstance(ty)
                                | _ -> ty, null
                            match isMandatory with
                            | true  -> ProvidedParameter(TypeInference.toCamelCase name, newTy)
                            | false -> ProvidedParameter(TypeInference.toCamelCase name, newTy, optionalValue=defValue)]
                    let paramCount = paramList.Length
                    let pm =
                        ProvidedMethod(
                            methodName = cmdlet.Name,
                            parameters = paramList,
                            returnType = cmdlet.ResultType,
                            IsStaticMethod = true,
                            InvokeCode =
                                fun args ->
                                    if args.Length <> paramCount then
                                        failwithf "Expected %d arguments and received %d" paramCount args.Length

                                    let namedArgs = [0..(paramCount-1)] |> List.map (fun i ->
                                                        Quotations.Expr.Coerce(args.[i], typeof<obj>))
                                    let namedArgs = Quotations.Expr.NewArray(typeof<obj>, namedArgs)
                                    let uid = cmdlet.UniqueID;

                                    <@@ Runtime.Current(psSnapIns.Split(';'), modules.Split(';'), is64bitRequired, false)
                                                .Execute(uid, (%%namedArgs : obj[])) @@>)
                    pm.AddXmlDocDelayed(fun() ->runtime.GetXmlDoc(cmdlet.Name))
                    yield pm :> MemberInfo
                ])
            pty))
    do this.AddNamespace(rootNamespace, [ shell ])

[<TypeProviderAssembly>]
do()