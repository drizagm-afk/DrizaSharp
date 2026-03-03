using DrzSharp.Compiler.Lowerer;

namespace DrzSharp.Compiler.Default.Lowerer;

public static class Bindings
{
    public static void Bind()
    {
        Virtual.Bind();
        Logic.Bind();
    }
}

//VIRTUAL
public static partial class Virtual
{
    public static void Bind()
    {
        EntryPoint_Id = Binding.BindRule(VirtualRules.EntryPoint);
        InitASMMethod_Id = Binding.BindRule(VirtualRules.InitASMMethod);
    }
}

//LOGIC
public static partial class Logic
{
    public static void Bind()
    {
        NewLoc_Id = Binding.BindRule(LogicRules.NewLoc);
        LdLoc_Id = Binding.BindRule(LogicRules.LdLoc);
        StLoc_Id = Binding.BindRule(LogicRules.StLoc);

        NewBr_Id = Binding.BindRule(LogicRules.NewBr);
        Br_Id = Binding.BindRule(LogicRules.Br);
        BrTrue_Id = Binding.BindRule(LogicRules.BrTrue);
        BrFalse_Id = Binding.BindRule(LogicRules.BrFalse);

        Ceq_Id = Binding.BindRule(LogicRules.Ceq);
        Cgt_Id = Binding.BindRule(LogicRules.Cgt);
        Clt_Id = Binding.BindRule(LogicRules.Clt);

        Add_Id = Binding.BindRule(LogicRules.Add);
        Sub_Id = Binding.BindRule(LogicRules.Sub);
        Mul_Id = Binding.BindRule(LogicRules.Mul);
        Div_Id = Binding.BindRule(LogicRules.Div);

        LdcI4_Id = Binding.BindRule(LogicRules.LdcI4);
        LdStr_Id = Binding.BindRule(LogicRules.LdStr);

        Print_Id = Binding.BindRule(LogicRules.Print);
        Ret_Id = Binding.BindRule(LogicRules.Ret);
    }
}