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

        Binding.BindRule<ASMLocalsRule>();
        Binding.BindRule<ASMVarDeclRule>();
        Binding.BindRule<ASMVarUseRule>();

        Binding.BindRule<ASMLabelRule>();
        Binding.BindRule<ASMBrRule>();

        Binding.BindRule<ASMCompareRule>();
        Binding.BindRule<ASMArithmeticRule>();

        Binding.BindRule<ASMLdcI4Rule>();
        Binding.BindRule<ASMLdstrRule>();

        Binding.BindRule<ASMPrintRule>();
        Binding.BindRule<ASMReturnRule>();
    }
}