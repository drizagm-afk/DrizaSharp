using System.Collections.Immutable;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Loader;

public partial class LoaderProcess
{
    public partial void Restore()
    {
        RestoreDependencies();
        RestoreRuleset();
        RestoreModules();
    }
    public void RestoreDependencies()
    {
        var builder = ImmutableArray.CreateBuilder<int>();

        //MSCORELIB
        Context.LazyLoadAssembly(@"C:\Driza\DrizaSharp\packages\System.Private.CoreLib.dll");
        builder.Add(0);

        Project.Dependencies = builder.MoveToImmutable();
    }
    public void RestoreModules()
    {
        var builder = ImmutableArray.CreateBuilder<DzModule>();

        //MAIN MODULE
        DzModule mod = new(0, VAssembly.GlobalNspaceId);
        builder.Add(mod);

        Project.Modules = builder.MoveToImmutable();
    }
    public void RestoreRuleset()
    {
        //RULES
        Project.Ruleset = new();
        Context.LoadRuleset(Project.Ruleset, -1, 0, "");
    }
}