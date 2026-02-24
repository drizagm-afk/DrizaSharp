using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Lowerer;

public partial class LowererProcess
{
    //LOWER PROJECT
    public void LowerProject(DzProject project)
    {
        foreach (var file in project.Files) LowerFile(file);
    }

    //LOWER FILE
    private DzFile File = null!;
    private TASI TASI => File.TASI;
    public void LowerFile(DzFile file)
    {
        File = file;
        //LEFT TO BUILD
    }
}