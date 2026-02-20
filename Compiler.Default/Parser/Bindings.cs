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
    private static void VirtualPhase(ParserProcess process)
    {
        foreach (var site in process.Sites)
            process.EvalMatch(site);
        //foreach (var site in _sites) EvalValidate(project, phaseCode, site);
    }
    private static void LogicPhase(ParserProcess process)
    {
        foreach (var site in process.Sites)
            process.EvalMatch(site);
        //foreach (var site in _sites) EvalValidate(project, phaseCode, site);
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