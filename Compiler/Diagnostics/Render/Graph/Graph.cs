namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    const string GRAPH_CONN = " ├─ ";
    const string GRAPH_TAB = " │  ";

    private void PrintGConn(string cont, int tabs)
    {
        if (tabs == 0)
            WriteLine(cont);
        else
        {
            WriteLine(
                GRAPH_TAB.Repeat(tabs - 1) + GRAPH_CONN + cont
            );
        }
    }
    private void PrintGTab(string cont, int tabs)
    => WriteLine(GRAPH_TAB.Repeat(tabs) + cont);
}