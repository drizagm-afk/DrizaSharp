namespace DrzSharp.Compiler.Parser;

public interface SemanticView : IReadOnlyAttrs
{
    
}
public interface SemanticContext : SemanticView, IAttrs, ITags
{
    
}
public partial class ParserProcess : SemanticContext
{
    
}