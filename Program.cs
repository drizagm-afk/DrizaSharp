// See https://aka.ms/new-console-template for more information
using Driza.Compiler;

Compiler.Bind();
Compiler.Compile(@"C:\Driza\DrizaSharp", @"testfiles\Program.dz");
Compiler.Debug();