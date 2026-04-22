using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Loader;

public partial class LoaderProcess
{
    public partial bool Restore()
    {
        RestoreDependencies();
        RestoreModules();
        RestoreRuleset();

        return !HasError();
    }
    public void RestoreDependencies()
    {
        //CORELIB
        Context.LoadCoreLib();
    }
    public void RestoreModules()
    {
        var builder = ArrayBuilder.Create<DzModule>();

        //MAIN MODULE
        builder.Add(RestoreModuleDependencies(0, VAssemblyEdit.GlobalNspaceId));

        Project.Modules = builder.MoveToView();
    }
    public DzModule RestoreModuleDependencies(int id, int nspaceId)
    {
        DzModule module = new(id, nspaceId);
        var builder = ArrayBuilder.Create<GlobalId>();

        //SELF DEPENDENCY
        builder.Add(new(-1, nspaceId));

        module.Dependencies = builder.MoveToView();
        return module;
    }
    public void RestoreRuleset()
    {
        //RULESET
        Context.BindRuleset("", -1, 0);
    }
}