using Mono.Cecil;
using DrzSharp.Compiler.Project;
using DrzSharp.Compiler.Text;

namespace DrzSharp.Compiler.Evaluator;

public partial class EvalProcess
{
    private static DzProjectType GetProjectType(string path)
    {
        return Path.GetExtension(path) switch
        {
            ".dz" => DzProjectType.SINGLE_FILE,
            ".dzproj" => DzProjectType.MULTI_FILE,
            _ => throw new Exception("INVALID TARGET FILE")
        };
    }

    private DzProject Project = null!;
    public DzProject EvalProject(string root, string path)
    {
        Project = new DzProject(path, GetProjectType(path));

        //LOAD FILES
        if (Project.Type == DzProjectType.SINGLE_FILE)
            EvalFile(path);
        if (Project.Type == DzProjectType.MULTI_FILE)
            throw new Exception("MULTI-FILE PROJECTS ARE UNSUPPORTED");

        //ADD MSCORELIB
        BindAssembly(AssemblyDefinition.ReadAssembly(@"C:\Driza\DrizaSharp\packages\System.Private.CoreLib.dll"));

        //ADD ASSEMBLIES


        //LOAD ASSEMBLIES
        LoadVWorld();

        return Project;
    }
    private void EvalFile(string path)
    {
        //TAST INITIATION
        var content = new SourceSpan(File.ReadAllText(path));
        var dzFile = new DzFile(Project.Files.Count, path, content);

        Project.Files.Add(dzFile);
    }

    //>>>> LOAD VWORLD <<<<
    private void LoadVWorld()
    {
        LoadAssemblies();

        VWorld.SetReadOnly();
        VWorld.AddAssembly();
    }
}