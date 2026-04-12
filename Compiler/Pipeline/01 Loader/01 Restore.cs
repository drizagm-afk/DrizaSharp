using System.Collections.Immutable;
using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Loader;

public partial class LoaderProcess
{
    public partial void Restore()
    {
        RestoreDependencies();
        RestoreModules();
        RestoreRuleset();
    }
    public void RestoreDependencies()
    {
        //CORELIB
        Context.LoadDependency(@"C:\Driza\DrizaSharp\packages\System.Private.CoreLib.dll");
    }
    public void RestoreModules()
    {
        var builder = ImmutableArray.CreateBuilder<DzModule>();

        //MAIN MODULE
        builder.Add(RestoreModuleDependencies(0, VAssemblyEdit.GlobalNspaceId));

        Project.Modules = builder.ToImmutable();
    }
    public DzModule RestoreModuleDependencies(int id, int nspaceId)
    {
        DzModule module = new(id, nspaceId);
        var builder = ImmutableArray.CreateBuilder<GlobalId>();

        //SELF DEPENDENCY
        builder.Add(new(-1, nspaceId));

        module.Dependencies = builder.ToImmutable();

        return module;
    }
    public void RestoreRuleset()
    {
        //RULESET
        Context.BindRuleset("", -1, 0);
    }
}