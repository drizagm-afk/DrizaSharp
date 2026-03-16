using System.Collections.Immutable;

namespace DrzSharp.Compiler.Virtual;

public partial class VirtualWorld
{
    readonly Dictionary<(int, ImmutableArray<GenericId>), int> _typesFound = [];
    public bool TryFindType(int asmHash, out int typeId, params ImmutableArray<GenericId> nameList)
    {
        var key = (asmHash, nameList);
        if (_typesFound.TryGetValue(key, out typeId))
            return true;

        //LOOP
        typeId = -1;

        var nspaceId = ReadAssembly(asmHash).GlobalNspace;
        foreach (var name in nameList)
        {
            if (nspaceId >= 0)
            {
                if (TryReadNspace(nspaceId, name.Name, out var nspace))
                    nspaceId = nspace.Id;
                else if (TryReadTypeBase<VTypeBase>(nspaceId, name, out var type))
                {
                    typeId = type.Id;
                    nspaceId = -1;
                }
                else return false;
            }
            else
            {
                if (!TryReadTypeBase<VTypeBase>(typeId, name, out var type))
                    return false;

                typeId = type.Id;
            }
        }

        if (typeId < 0)
            return false;

        _typesFound[key] = typeId;
        return true;
    }

    private static bool MethodEquals(IVReadOnlyMethodBase method, ImmutableArray<UMethodParam> parameters)
    {
        if (method.Params.Length != parameters.Length) return false;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (!method.Params[i].Equals(parameters[i]))
                return false;
        }
        return true;
    }
    public bool TryFindMethodName(int typeId, string methodName, out int methodId, params ImmutableArray<UMethodParam> parameters)
    {
        foreach (var method in ReadMethodNameOverloads(typeId, methodName))
        {
            if (MethodEquals(method, parameters))
            {
                methodId = method.Id;
                return true;
            }
        }

        methodId = -1;
        return false;
    }
    public bool TryFindMethod(int typeId, GenericId methodName, out int methodId, params ImmutableArray<UMethodParam> parameters)
    {
        foreach (var method in ReadMethodOverloads(typeId, methodName))
        {
            if (MethodEquals(method, parameters))
            {
                methodId = method.Id;
                return true;
            }
        }

        methodId = -1;
        return false;
    }

    public bool TryFindCtor(int typeId, out int ctorId, params ImmutableArray<UMethodParam> parameters)
    {
        foreach (var ctor in ReadCtorOverloads(typeId))
        {
            if (MethodEquals(ctor, parameters))
            {
                ctorId = ctor.Id;
                return true;
            }
        }

        ctorId = -1;
        return false;
    }
}