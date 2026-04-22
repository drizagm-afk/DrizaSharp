namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    private partial bool LowerVirtual()
    {
        DefineVirtual();
        if (HasError())
            return false;

        DefineVirtualData();
        return !HasError();
    }

    //STEPS
    private partial void DefineVirtual();
    private partial void DefineVirtualData();
}