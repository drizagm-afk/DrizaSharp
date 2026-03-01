using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

public static class Bindings
{
    public static void Bind()
    {
        BindRealms();
        BindRules();
    }

    //REALMS
    private static void BindRealms()
    {
        Realms.ASMLogic = Binding.AddRealm("ASMLogic");
        Realms.Logic = Binding.AddRealm("Logic");
    }

    //RULES
    private static void BindRules()
    {
        Binding.BindRule<EntryPointRule>();

        Binding.BindRule<ASMPrintRule>();
        Binding.BindRule<ASMLoadStrRule>();
        Binding.BindRule<ASMReturnRule>();
    }
}