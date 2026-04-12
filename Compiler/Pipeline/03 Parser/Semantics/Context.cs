namespace DrzSharp.Compiler.Parser;

public interface SemanticView : Context, VirtualView, AttrsView
{
    
}
public interface SemanticContext : SemanticView, VirtualContext, Attrs, Tags
{
    
}
public partial class ParserProcess : SemanticContext
{
    
}