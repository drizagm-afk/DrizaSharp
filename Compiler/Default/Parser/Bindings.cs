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

        //LOGIC
        ctx.BindRule<RepeatStmtRule>(Realms.Logic);
        ctx.BindRule<IfStmtRule>(Realms.Logic);

        ctx.BindRule<VarDeclRule>(Realms.Logic);
        ctx.BindRule<VarSetRule>(Realms.Logic);
        ctx.BindRule<PrintRule>(Realms.Logic);

        ctx.BindRuleClass<ExprRule>(Realms.Logic);
        ctx.BindRuleClass<MonoExprRule, ExprRule>();
        ctx.BindRuleClass<ChainExprRule, ExprRule>();

        ctx.BindRule<VarGetRule, MonoExprRule>(isAbstract: true);
        ctx.BindRule<NumberLitRule, MonoExprRule>(isAbstract: true);

        ctx.BindRule<AddExprRule, ChainExprRule>(isAbstract: true);
        ctx.BindRule<SubExprRule, ChainExprRule>(isAbstract: true);
        ctx.BindRule<MulExprRule, ChainExprRule>(isAbstract: true);

        ctx.BindRule<GreaterExprRule, ChainExprRule>(isAbstract: true);
        ctx.BindRule<LessExprRule, ChainExprRule>(isAbstract: true);
        ctx.BindRule<EqualExprRule, ChainExprRule>(isAbstract: true);

        /*
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
        */
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