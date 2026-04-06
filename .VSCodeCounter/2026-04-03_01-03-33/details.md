# Details

Date : 2026-04-03 01:03:33

Directory c:\\Driza\\DrizaSharp

Total : 85 files,  6121 codes, 644 comments, 1199 blanks, all 7964 lines

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [Compiler/Compiler.cs](/Compiler/Compiler.cs) | C# | 34 | 28 | 9 | 71 |
| [Compiler/Context/Context.cs](/Compiler/Context/Context.cs) | C# | 21 | 1 | 5 | 27 |
| [Compiler/Context/Identifiers.cs](/Compiler/Context/Identifiers.cs) | C# | 5 | 0 | 1 | 6 |
| [Compiler/Context/Loader.cs](/Compiler/Context/Loader.cs) | C# | 109 | 5 | 21 | 135 |
| [Compiler/Context/Loader/01 Bind.cs](/Compiler/Context/Loader/01%20Bind.cs) | C# | 130 | 24 | 28 | 182 |
| [Compiler/Context/Loader/02 BindData.cs](/Compiler/Context/Loader/02%20BindData.cs) | C# | 290 | 38 | 51 | 379 |
| [Compiler/Context/Loader/03 Rules.cs](/Compiler/Context/Loader/03%20Rules.cs) | C# | 17 | 6 | 6 | 29 |
| [Compiler/Context/Loader/Loader.cs](/Compiler/Context/Loader/Loader.cs) | C# | 25 | 3 | 4 | 32 |
| [Compiler/Default/Bindings.cs](/Compiler/Default/Bindings.cs) | C# | 10 | 0 | 1 | 11 |
| [Compiler/Default/Lexer/Bindings.cs](/Compiler/Default/Lexer/Bindings.cs) | C# | 39 | 2 | 12 | 53 |
| [Compiler/Default/Lexer/Rules.cs](/Compiler/Default/Lexer/Rules.cs) | C# | 125 | 3 | 22 | 150 |
| [Compiler/Default/Lexer/Shared/TokenTypes.cs](/Compiler/Default/Lexer/Shared/TokenTypes.cs) | C# | 20 | 4 | 6 | 30 |
| [Compiler/Default/Lowerer/Bindings.cs](/Compiler/Default/Lowerer/Bindings.cs) | C# | 42 | 2 | 9 | 53 |
| [Compiler/Default/Lowerer/Instructions/Logic.cs](/Compiler/Default/Lowerer/Instructions/Logic.cs) | C# | 178 | 12 | 32 | 222 |
| [Compiler/Default/Lowerer/Instructions/Virtual.cs](/Compiler/Default/Lowerer/Instructions/Virtual.cs) | C# | 45 | 2 | 10 | 57 |
| [Compiler/Default/Parser/Bindings.cs](/Compiler/Default/Parser/Bindings.cs) | C# | 30 | 2 | 9 | 41 |
| [Compiler/Default/Parser/Rules.cs](/Compiler/Default/Parser/Rules.cs) | C# | 432 | 11 | 62 | 505 |
| [Compiler/Default/Parser/Shared/Patterns.cs](/Compiler/Default/Parser/Shared/Patterns.cs) | C# | 138 | 14 | 21 | 173 |
| [Compiler/Default/Parser/Shared/Realms.cs](/Compiler/Default/Parser/Shared/Realms.cs) | C# | 8 | 2 | 3 | 13 |
| [Compiler/Default/Parser/Shared/Tags.cs](/Compiler/Default/Parser/Shared/Tags.cs) | C# | 7 | 0 | 2 | 9 |
| [Compiler/Diagnostics/Config.cs](/Compiler/Diagnostics/Config.cs) | C# | 23 | 0 | 4 | 27 |
| [Compiler/Diagnostics/Diagnostics.cs](/Compiler/Diagnostics/Diagnostics.cs) | C# | 47 | 1 | 8 | 56 |
| [Compiler/Diagnostics/Exceptions.cs](/Compiler/Diagnostics/Exceptions.cs) | C# | 2 | 0 | 1 | 3 |
| [Compiler/Diagnostics/Render.cs](/Compiler/Diagnostics/Render.cs) | C# | 415 | 27 | 92 | 534 |
| [Compiler/Models/Source.cs](/Compiler/Models/Source.cs) | C# | 42 | 0 | 6 | 48 |
| [Compiler/Models/TASI.cs](/Compiler/Models/TASI.cs) | C# | 165 | 17 | 29 | 211 |
| [Compiler/Models/TAST.cs](/Compiler/Models/TAST.cs) | C# | 460 | 32 | 83 | 575 |
| [Compiler/Pipeline/01 Loader/01 Restore.cs](/Compiler/Pipeline/01%20Loader/01%20Restore.cs) | C# | 26 | 2 | 6 | 34 |
| [Compiler/Pipeline/01 Loader/02 Load.cs](/Compiler/Pipeline/01%20Loader/02%20Load.cs) | C# | 20 | 1 | 4 | 25 |
| [Compiler/Pipeline/01 Loader/Process.cs](/Compiler/Pipeline/01%20Loader/Process.cs) | C# | 24 | 2 | 4 | 30 |
| [Compiler/Pipeline/02 Lexer/01 Lex/Context.cs](/Compiler/Pipeline/02%20Lexer/01%20Lex/Context.cs) | C# | 32 | 3 | 7 | 42 |
| [Compiler/Pipeline/02 Lexer/01 Lex/Phase.cs](/Compiler/Pipeline/02%20Lexer/01%20Lex/Phase.cs) | C# | 56 | 9 | 13 | 78 |
| [Compiler/Pipeline/02 Lexer/Context.cs](/Compiler/Pipeline/02%20Lexer/Context.cs) | C# | 15 | 1 | 2 | 18 |
| [Compiler/Pipeline/02 Lexer/Process.cs](/Compiler/Pipeline/02%20Lexer/Process.cs) | C# | 21 | 3 | 6 | 30 |
| [Compiler/Pipeline/03 Parser/01 Match/Build/Context.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Build/Context.cs) | C# | 47 | 0 | 11 | 58 |
| [Compiler/Pipeline/03 Parser/01 Match/Build/Step.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Build/Step.cs) | C# | 35 | 0 | 7 | 42 |
| [Compiler/Pipeline/03 Parser/01 Match/Match/Context.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Match/Context.cs) | C# | 121 | 13 | 20 | 154 |
| [Compiler/Pipeline/03 Parser/01 Match/Match/Pattern/Definition.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Match/Pattern/Definition.cs) | C# | 48 | 6 | 10 | 64 |
| [Compiler/Pipeline/03 Parser/01 Match/Match/Pattern/Patterns.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Match/Pattern/Patterns.cs) | C# | 140 | 6 | 29 | 175 |
| [Compiler/Pipeline/03 Parser/01 Match/Match/Step.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Match/Step.cs) | C# | 162 | 21 | 32 | 215 |
| [Compiler/Pipeline/03 Parser/01 Match/Match/View.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Match/View.cs) | C# | 176 | 9 | 25 | 210 |
| [Compiler/Pipeline/03 Parser/01 Match/Phase.cs](/Compiler/Pipeline/03%20Parser/01%20Match/Phase.cs) | C# | 16 | 2 | 3 | 21 |
| [Compiler/Pipeline/03 Parser/02 Bind/Phase.cs](/Compiler/Pipeline/03%20Parser/02%20Bind/Phase.cs) | C# | 30 | 2 | 6 | 38 |
| [Compiler/Pipeline/03 Parser/03 Validate/Context.cs](/Compiler/Pipeline/03%20Parser/03%20Validate/Context.cs) | C# | 75 | 4 | 10 | 89 |
| [Compiler/Pipeline/03 Parser/03 Validate/Phase.cs](/Compiler/Pipeline/03%20Parser/03%20Validate/Phase.cs) | C# | 64 | 3 | 14 | 81 |
| [Compiler/Pipeline/03 Parser/04 Emit/Context.cs](/Compiler/Pipeline/03%20Parser/04%20Emit/Context.cs) | C# | 61 | 4 | 11 | 76 |
| [Compiler/Pipeline/03 Parser/04 Emit/Phase.cs](/Compiler/Pipeline/03%20Parser/04%20Emit/Phase.cs) | C# | 41 | 4 | 7 | 52 |
| [Compiler/Pipeline/03 Parser/Context.cs](/Compiler/Pipeline/03%20Parser/Context.cs) | C# | 41 | 6 | 10 | 57 |
| [Compiler/Pipeline/03 Parser/Context/Mutate.cs](/Compiler/Pipeline/03%20Parser/Context/Mutate.cs) | C# | 0 | 76 | 0 | 76 |
| [Compiler/Pipeline/03 Parser/Mutate/Context.cs](/Compiler/Pipeline/03%20Parser/Mutate/Context.cs) | C# | 71 | 5 | 15 | 91 |
| [Compiler/Pipeline/03 Parser/Mutate/Step.cs](/Compiler/Pipeline/03%20Parser/Mutate/Step.cs) | C# | 99 | 9 | 19 | 127 |
| [Compiler/Pipeline/03 Parser/Process.cs](/Compiler/Pipeline/03%20Parser/Process.cs) | C# | 25 | 3 | 7 | 35 |
| [Compiler/Pipeline/03 Parser/Process/Mutate.cs](/Compiler/Pipeline/03%20Parser/Process/Mutate.cs) | C# | 0 | 48 | 0 | 48 |
| [Compiler/Pipeline/03 Parser/Semantics/Context.cs](/Compiler/Pipeline/03%20Parser/Semantics/Context.cs) | C# | 10 | 0 | 4 | 14 |
| [Compiler/Pipeline/03 Parser/Semantics/Tags.cs](/Compiler/Pipeline/03%20Parser/Semantics/Tags.cs) | C# | 147 | 13 | 26 | 186 |
| [Compiler/Pipeline/04 Lowerer/Context/Logic.cs](/Compiler/Pipeline/04%20Lowerer/Context/Logic.cs) | C# | 22 | 0 | 3 | 25 |
| [Compiler/Pipeline/04 Lowerer/Context/Virtual.cs](/Compiler/Pipeline/04%20Lowerer/Context/Virtual.cs) | C# | 18 | 1 | 3 | 22 |
| [Compiler/Pipeline/04 Lowerer/Lowerer.cs](/Compiler/Pipeline/04%20Lowerer/Lowerer.cs) | C# | 35 | 2 | 6 | 43 |
| [Compiler/Pipeline/04 Lowerer/LowererBindings.cs](/Compiler/Pipeline/04%20Lowerer/LowererBindings.cs) | C# | 10 | 0 | 2 | 12 |
| [Compiler/Pipeline/04 Lowerer/LowererContext.cs](/Compiler/Pipeline/04%20Lowerer/LowererContext.cs) | C# | 49 | 3 | 8 | 60 |
| [Compiler/Pipeline/04 Lowerer/LowererProcess.cs](/Compiler/Pipeline/04%20Lowerer/LowererProcess.cs) | C# | 81 | 8 | 13 | 102 |
| [Compiler/Project.cs](/Compiler/Project.cs) | C# | 50 | 0 | 13 | 63 |
| [Compiler/Rules/Assembly.cs](/Compiler/Rules/Assembly.cs) | C# | 21 | 5 | 9 | 35 |
| [Compiler/Rules/Bindings.cs](/Compiler/Rules/Bindings.cs) | C# | 17 | 0 | 4 | 21 |
| [Compiler/Rules/Bindings/Lexer.cs](/Compiler/Rules/Bindings/Lexer.cs) | C# | 20 | 0 | 6 | 26 |
| [Compiler/Rules/Bindings/Parser.cs](/Compiler/Rules/Bindings/Parser.cs) | C# | 80 | 3 | 25 | 108 |
| [Compiler/Rules/Context.cs](/Compiler/Rules/Context.cs) | C# | 17 | 0 | 4 | 21 |
| [Compiler/Rules/Context/Lexer.cs](/Compiler/Rules/Context/Lexer.cs) | C# | 31 | 3 | 6 | 40 |
| [Compiler/Rules/Context/Parser.cs](/Compiler/Rules/Context/Parser.cs) | C# | 77 | 4 | 12 | 93 |
| [Compiler/Rules/Definitions/Lexer.cs](/Compiler/Rules/Definitions/Lexer.cs) | C# | 13 | 2 | 2 | 17 |
| [Compiler/Rules/Definitions/Parser.cs](/Compiler/Rules/Definitions/Parser.cs) | C# | 95 | 12 | 23 | 130 |
| [Compiler/Shared.cs](/Compiler/Shared.cs) | C# | 24 | 2 | 6 | 32 |
| [Compiler/Virtual/Assembly.cs](/Compiler/Virtual/Assembly.cs) | C# | 227 | 22 | 46 | 295 |
| [Compiler/Virtual/Assembly/Fields.cs](/Compiler/Virtual/Assembly/Fields.cs) | C# | 45 | 7 | 13 | 65 |
| [Compiler/Virtual/Assembly/Methods.cs](/Compiler/Virtual/Assembly/Methods.cs) | C# | 200 | 16 | 43 | 259 |
| [Compiler/Virtual/Assembly/Properties.cs](/Compiler/Virtual/Assembly/Properties.cs) | C# | 51 | 6 | 11 | 68 |
| [Compiler/Virtual/Assembly/Types.cs](/Compiler/Virtual/Assembly/Types.cs) | C# | 254 | 20 | 49 | 323 |
| [Compiler/Virtual/Debugger.cs](/Compiler/Virtual/Debugger.cs) | C# | 65 | 3 | 13 | 81 |
| [Compiler/Virtual/World.cs](/Compiler/Virtual/World.cs) | C# | 8 | 1 | 2 | 11 |
| [Compiler/Virtual/World/Usage.cs](/Compiler/Virtual/World/Usage.cs) | C# | 111 | 32 | 26 | 169 |
| [DrizaSharp.csproj](/DrizaSharp.csproj) | XML | 11 | 0 | 4 | 15 |
| [Program.cs](/Program.cs) | C# | 3 | 1 | 1 | 5 |
| [Program.runtimeconfig.json](/Program.runtimeconfig.json) | JSON | 9 | 0 | 0 | 9 |
| [README.md](/README.md) | Markdown | 0 | 0 | 1 | 1 |
| [dzdiag.config.json](/dzdiag.config.json) | JSON | 15 | 0 | 0 | 15 |

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)