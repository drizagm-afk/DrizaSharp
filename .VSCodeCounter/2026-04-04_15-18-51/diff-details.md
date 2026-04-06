# Diff Details

Date : 2026-04-04 15:18:51

Directory c:\\Driza\\DrizaSharp

Total : 56 files,  -314 codes, 388 comments, -118 blanks, all -44 lines

[Summary](results.md) / [Details](details.md) / [Diff Summary](diff.md) / Diff Details

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [Compiler/Compiler.cs](/Compiler/Compiler.cs) | C# | 15 | -11 | 9 | 13 |
| [Compiler/Context/Loader.cs](/Compiler/Context/Loader.cs) | C# | 3 | 1 | 1 | 5 |
| [Compiler/Context/Loader/03 Rules.cs](/Compiler/Context/Loader/03%20Rules.cs) | C# | -17 | -6 | -6 | -29 |
| [Compiler/Context/Loader/03 Ruleset.cs](/Compiler/Context/Loader/03%20Ruleset.cs) | C# | 21 | 6 | 5 | 32 |
| [Compiler/Default/Bindings.cs](/Compiler/Default/Bindings.cs) | C# | -1 | 0 | 0 | -1 |
| [Compiler/Default/Lexer/Bindings.cs](/Compiler/Default/Lexer/Bindings.cs) | C# | 19 | 2 | 3 | 24 |
| [Compiler/Default/Lexer/Shared/TokenTypes.cs](/Compiler/Default/Lexer/Shared/TokenTypes.cs) | C# | -20 | -4 | -6 | -30 |
| [Compiler/Default/Lowerer/Bindings.cs](/Compiler/Default/Lowerer/Bindings.cs) | C# | -42 | -2 | -9 | -53 |
| [Compiler/Default/Lowerer/Instructions/Logic.cs](/Compiler/Default/Lowerer/Instructions/Logic.cs) | C# | -178 | -12 | -32 | -222 |
| [Compiler/Default/Lowerer/Instructions/Virtual.cs](/Compiler/Default/Lowerer/Instructions/Virtual.cs) | C# | -45 | -2 | -10 | -57 |
| [Compiler/Default/Parser/Bindings.cs](/Compiler/Default/Parser/Bindings.cs) | C# | 0 | 0 | -2 | -2 |
| [Compiler/Default/Parser/Patterns/Groups.cs](/Compiler/Default/Parser/Patterns/Groups.cs) | C# | 57 | 4 | 6 | 67 |
| [Compiler/Default/Parser/Patterns/Shortcuts.cs](/Compiler/Default/Parser/Patterns/Shortcuts.cs) | C# | 9 | 0 | 2 | 11 |
| [Compiler/Default/Parser/Patterns/TokenTables.cs](/Compiler/Default/Parser/Patterns/TokenTables.cs) | C# | 52 | 1 | 9 | 62 |
| [Compiler/Default/Parser/Patterns/Tokens.cs](/Compiler/Default/Parser/Patterns/Tokens.cs) | C# | 36 | 4 | 6 | 46 |
| [Compiler/Default/Parser/Realms.cs](/Compiler/Default/Parser/Realms.cs) | C# | 7 | 0 | 2 | 9 |
| [Compiler/Default/Parser/Rules.cs](/Compiler/Default/Parser/Rules.cs) | C# | -23 | 0 | -18 | -41 |
| [Compiler/Default/Parser/Shared/Patterns.cs](/Compiler/Default/Parser/Shared/Patterns.cs) | C# | -138 | -14 | -21 | -173 |
| [Compiler/Default/Parser/Shared/Realms.cs](/Compiler/Default/Parser/Shared/Realms.cs) | C# | -8 | -2 | -3 | -13 |
| [Compiler/Default/Parser/Shared/Tags.cs](/Compiler/Default/Parser/Shared/Tags.cs) | C# | -7 | 0 | -2 | -9 |
| [Compiler/Default/Parser/Tags.cs](/Compiler/Default/Parser/Tags.cs) | C# | 7 | 0 | 2 | 9 |
| [Compiler/Diagnostics/Config.cs](/Compiler/Diagnostics/Config.cs) | C# | -23 | 29 | -4 | 2 |
| [Compiler/Diagnostics/Diagnostics.cs](/Compiler/Diagnostics/Diagnostics.cs) | C# | 6 | -1 | -3 | 2 |
| [Compiler/Diagnostics/Render.cs](/Compiler/Diagnostics/Render.cs) | C# | -415 | 494 | -92 | -13 |
| [Compiler/Models/TASI.cs](/Compiler/Models/TASI.cs) | C# | 42 | 0 | 8 | 50 |
| [Compiler/Pipeline/01 Loader/01 Restore.cs](/Compiler/Pipeline/01%20Loader/01%20Restore.cs) | C# | 5 | 1 | 0 | 6 |
| [Compiler/Pipeline/01 Loader/02 Load.cs](/Compiler/Pipeline/01%20Loader/02%20Load.cs) | C# | -1 | 0 | 0 | -1 |
| [Compiler/Pipeline/01 Loader/Process.cs](/Compiler/Pipeline/01%20Loader/Process.cs) | C# | 1 | 0 | 0 | 1 |
| [Compiler/Pipeline/02 Lexer/01 Lex/Phase.cs](/Compiler/Pipeline/02%20Lexer/01%20Lex/Phase.cs) | C# | -1 | 0 | 0 | -1 |
| [Compiler/Pipeline/02 Lexer/Process.cs](/Compiler/Pipeline/02%20Lexer/Process.cs) | C# | -1 | 0 | 0 | -1 |
| [Compiler/Pipeline/03 Parser/04 Emit/Context.cs](/Compiler/Pipeline/03%20Parser/04%20Emit/Context.cs) | C# | 7 | 0 | 2 | 9 |
| [Compiler/Pipeline/03 Parser/Context/Mutate.cs](/Compiler/Pipeline/03%20Parser/Context/Mutate.cs) | C# | 0 | -76 | 0 | -76 |
| [Compiler/Pipeline/03 Parser/Process.cs](/Compiler/Pipeline/03%20Parser/Process.cs) | C# | 1 | 0 | 0 | 1 |
| [Compiler/Pipeline/03 Parser/Process/Mutate.cs](/Compiler/Pipeline/03%20Parser/Process/Mutate.cs) | C# | 0 | -48 | 0 | -48 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Context.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Context.cs) | C# | 13 | 0 | 2 | 15 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/Context.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/Context.cs) | C# | 76 | 2 | 9 | 87 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/InstrList/Constants.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/InstrList/Constants.cs) | C# | 57 | 5 | 6 | 68 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/InstrList/Flow.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/InstrList/Flow.cs) | C# | 51 | 4 | 9 | 64 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/InstrList/Math.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/InstrList/Math.cs) | C# | 59 | 7 | 7 | 73 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/InstrList/Memory.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/InstrList/Memory.cs) | C# | 41 | 3 | 7 | 51 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/InstrList/Special.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/InstrList/Special.cs) | C# | 56 | 6 | 9 | 71 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Instructions/Step.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Instructions/Step.cs) | C# | 118 | 7 | 10 | 135 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Phase.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Phase.cs) | C# | 23 | 1 | 4 | 28 |
| [Compiler/Pipeline/04 Lowerer/01 Lower/Virtual/Step.cs](/Compiler/Pipeline/04%20Lowerer/01%20Lower/Virtual/Step.cs) | C# | 8 | 0 | 3 | 11 |
| [Compiler/Pipeline/04 Lowerer/Context/Logic.cs](/Compiler/Pipeline/04%20Lowerer/Context/Logic.cs) | C# | -22 | 0 | -3 | -25 |
| [Compiler/Pipeline/04 Lowerer/Context/Virtual.cs](/Compiler/Pipeline/04%20Lowerer/Context/Virtual.cs) | C# | -18 | -1 | -3 | -22 |
| [Compiler/Pipeline/04 Lowerer/Lowerer.cs](/Compiler/Pipeline/04%20Lowerer/Lowerer.cs) | C# | -35 | -2 | -6 | -43 |
| [Compiler/Pipeline/04 Lowerer/LowererBindings.cs](/Compiler/Pipeline/04%20Lowerer/LowererBindings.cs) | C# | -10 | 0 | -2 | -12 |
| [Compiler/Pipeline/04 Lowerer/LowererContext.cs](/Compiler/Pipeline/04%20Lowerer/LowererContext.cs) | C# | -49 | -3 | -8 | -60 |
| [Compiler/Pipeline/04 Lowerer/LowererProcess.cs](/Compiler/Pipeline/04%20Lowerer/LowererProcess.cs) | C# | -81 | -8 | -13 | -102 |
| [Compiler/Pipeline/04 Lowerer/Process.cs](/Compiler/Pipeline/04%20Lowerer/Process.cs) | C# | 24 | 3 | 6 | 33 |
| [Compiler/Project.cs](/Compiler/Project.cs) | C# | 7 | 0 | 0 | 7 |
| [Compiler/Rules/Bindings.cs](/Compiler/Rules/Bindings.cs) | C# | 4 | 0 | 0 | 4 |
| [Compiler/Rules/Context.cs](/Compiler/Rules/Context.cs) | C# | -2 | 0 | 0 | -2 |
| [Compiler/Rules/Context/Lexer.cs](/Compiler/Rules/Context/Lexer.cs) | C# | -1 | 0 | -1 | -2 |
| [Compiler/Rules/Context/Parser.cs](/Compiler/Rules/Context/Parser.cs) | C# | -1 | 0 | -1 | -2 |

[Summary](results.md) / [Details](details.md) / [Diff Summary](diff.md) / Diff Details