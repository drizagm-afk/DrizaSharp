namespace DrzSharp.Compiler.Parser;

//ADD VirtualView AND VirtualContext
public interface SemanticView : Context, AttrsView { }
public interface SemanticContext : SemanticView, Attrs, Tags { }
public partial class ParserProcess : SemanticContext { }