using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Lowerer;

//VIRTUAL
public static partial class Virtual
{
    public static void Bind()
    {
        EntryPoint.Id = Binding.BindRule(VirtualRules.EntryPoint);
        InitASMMethod.Id = Binding.BindRule(VirtualRules.InitASMMethod);
    }
}

//LOGIC
public static partial class Logic
{
    public static void Bind()
    {
        NewLoc.Id = Binding.BindRule(LogicRules.NewLoc);
        LdLoc.Id = Binding.BindRule(LogicRules.LdLoc);
        StLoc.Id = Binding.BindRule(LogicRules.StLoc);

        NewBr.Id = Binding.BindRule(LogicRules.NewBr);
        Br.Id = Binding.BindRule(LogicRules.Br);
        BrTrue.Id = Binding.BindRule(LogicRules.BrTrue);
        BrFalse.Id = Binding.BindRule(LogicRules.BrFalse);

        Ceq.Id = Binding.BindRule(LogicRules.Ceq);
        Cgt.Id = Binding.BindRule(LogicRules.Cgt);
        Clt.Id = Binding.BindRule(LogicRules.Clt);

        Add.Id = Binding.BindRule(LogicRules.Add);
        Sub.Id = Binding.BindRule(LogicRules.Sub);
        Mul.Id = Binding.BindRule(LogicRules.Mul);
        Div.Id = Binding.BindRule(LogicRules.Div);

        LdcI4.Id = Binding.BindRule(LogicRules.LdcI4);
        LdStr.Id = Binding.BindRule(LogicRules.LdStr);

        Print.Id = Binding.BindRule(LogicRules.Print);
        Ret.Id = Binding.BindRule(LogicRules.Ret);
    }
}