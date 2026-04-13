namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    const int barSize = 50;

    private void PrintProjectHeader()
    {
        const string inline = " PROJECT ";
        int padd = barSize - inline.Length;
        int left = padd / 2;
        int right = padd - left;

        WriteLine('='.Repeat(left) + inline + '='.Repeat(right));
        WriteLine($"FILE:   {Project.Path}");
        WriteLine($"AUTHOR: devdriz");
        WriteLine('='.Repeat(barSize));
        WriteLine();
    }
    private void PrintFileHeader()
    {
        string inline = $" <{File.Id:D3}> ";
        int padd = barSize - inline.Length;
        int left = padd / 2;
        int right = padd - left;

        WriteLine('/'.Repeat(left) + inline + '/'.Repeat(right));
        WriteLine($"FILE:   {File.Path}");
        WriteLine($"MODULE: <main>");
        WriteLine('/'.Repeat(barSize));
        WriteLine();
    }

    private void PrintSectionHeader(string title)
    {
        int padd = barSize - title.Length;
        int left = padd / 2;
        int right = padd - left;

        WriteLine(
            $"{'='.Repeat(left - 1)} {title} {'='.Repeat(right - 1)}"
        );
    }
}