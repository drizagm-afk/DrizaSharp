using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Lowerer;

//VIRTUAL
public static partial class Virtual
{
    public static void Bind()
    {
        EntryPoint.Id = Binding.BindRule(EntryPoint.Rule);
    }
}

//LOGIC
public static partial class Logic
{
    public static void Bind()
    {
        Ldstr.Id = Binding.BindRule(Rules.ASMLdstr);

        Print.Id = Binding.BindRule(Rules.ASMPrint);
        Ret.Id = Binding.BindRule(Rules.ASMRet);
    }
}