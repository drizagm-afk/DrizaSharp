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
        //LOAD CONSTANTS
        Load.CStr.Id = Binding.BindRule(Load.CStr.Rule);

        Print.Id = Binding.BindRule(Print.Rule);
        Return.Id = Binding.BindRule(Return.Rule);
    }
}