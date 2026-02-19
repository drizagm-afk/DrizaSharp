namespace DrzSharp.Compiler.Parser;

public partial class ParserSite
{
    private readonly HashSet<ScopeKey> _scope = [];
    public bool StoreScopeVar(int nodeId, string varType, string var)
    => _scope.Add(new(nodeId, varType, var));
    public bool HasScopeVar(int nodeId, string varType, string var)
    => _scope.Contains(new(nodeId, varType, var));
}
internal readonly record struct ScopeKey
(int NodeId, string VarType, string Var);