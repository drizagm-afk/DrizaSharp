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
        Realms.Virtual = Binding.AddRealm(Phases.VIRTUAL);

        Realms.ASMLogic = Binding.AddRealm(Phases.LOGIC);
        Realms.Logic = Binding.AddRealm(Phases.LOGIC);
    }

    //RULES
    private static void BindRules()
    {
        Binding.BindRule<TESTRule>();

        Binding.BindRule<EntryPointRule>();

        Binding.BindRule<ASMPrintRule>();
        Binding.BindRule<ASMLoadStrRule>();
        Binding.BindRule<ASMReturnRule>();
    }
}