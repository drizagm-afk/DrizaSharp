namespace DrzSharp.Compiler;

//>>>> IDENTIFIERS <<<<
public readonly record struct GlobalId
(int AssemblyId, int LocalId);
public readonly record struct RuleId
(int AssemblyId, int NspaceId, int LocalId);