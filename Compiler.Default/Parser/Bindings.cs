using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

public static class Bindings
{
    public static void Bind()
    {
        BindPhases();
        BindRealms();
        BindRealms();
    }

    //PHASES
    private static void BindPhases()
    {
        Binding.SetPhases(
            new ParserPhase(VirtualPhase),
            new ParserPhase(LogicPhase)
        );
    }
    private static void VirtualPhase(ParserProcess proc)
    {
        proc.ForeachSite(proc.Match);
        proc.ForeachSite(proc.Validate);
    }
    private static void LogicPhase(ParserProcess proc)
    {
        proc.ForeachSite(s =>
        {
            proc.Match(s);
            proc.Validate(s);
        });
    }

    //REALMS
    private static void BindRealms()
    {
        Realms.Virtual = Binding.AddRealm(0);

        Realms.ASMLogic = Binding.AddRealm(1);
        Realms.Logic = Binding.AddRealm(1);
    }

    //RULES
    private static void BindRules()
    {

    }
}