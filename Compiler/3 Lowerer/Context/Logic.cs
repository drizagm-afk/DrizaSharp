using Mono.Collections.Generic;
using Mono.Cecil.Cil;

namespace DrzSharp.Compiler.Lowerer;

public interface LogicContext
{
    public MethodBody MethodBody { get; }
    public ILProcessor IL { get; }
    public Collection<VariableDefinition> Variables { get; }
    public List<Instruction> Labels { get; }
}
public partial class LowererProcess : LogicContext
{
    public MethodBody MethodBody { get; private set; } = null!;
    public ILProcessor IL => MethodBody.GetILProcessor();
    public Collection<VariableDefinition> Variables => MethodBody.Variables;
    public List<Instruction> Labels { get; private set; } = [];

    internal void ResetLogic()
    {
        MethodBody = null!;
        Labels = [];
    }
}