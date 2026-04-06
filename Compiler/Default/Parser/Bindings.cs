using DrzSharp.Compiler.Rules;

namespace DrzSharp.Compiler.Default.Parser;

public static class Bindings
{
    public static void Bind(ParserBinding ctx)
    {
        Realms.Bind(ctx);
        BindRules(ctx);
    }
    private static void BindRules(ParserBinding ctx)
    {
        //VIRTUAL
        ctx.BindRule<EntryPointRule>(Realms.VIRTUAL);

        //ASM LOGIC
        ctx.BindRule<ASMLocalsRule>(Realms.ASMLogic);
        ctx.BindRule<ASMVarDeclRule>(Realms.ASMLogic, true);
        ctx.BindRule<ASMVarUseRule>(Realms.ASMLogic);

        ctx.BindRule<ASMLabelRule>(Realms.ASMLogic);
        ctx.BindRule<ASMBrRule>(Realms.ASMLogic);

        ctx.BindRule<ASMCompareRule>(Realms.ASMLogic);
        ctx.BindRule<ASMArithmeticRule>(Realms.ASMLogic);

        ctx.BindRule<ASMLdcI4Rule>(Realms.ASMLogic);
        ctx.BindRule<ASMLdstrRule>(Realms.ASMLogic);

        ctx.BindRule<ASMPrintRule>(Realms.ASMLogic);
        ctx.BindRule<ASMReturnRule>(Realms.ASMLogic);
    }
}
public static class Realms
{
    internal static void Bind(ParserBinding ctx)
    {
        ctx.AddRealm(Logic);
        ctx.AddRealm(ASMLogic);
    }

    public const string VIRTUAL = Model.Realms.VIRTUAL;

    public const string Logic = "Logic";
    public const string ASMLogic = "ASMLogic";
}