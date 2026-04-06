# Details

Date : 2026-04-04 15:18:51

Directory c:\\Driza\\DrizaSharp

Total : 87 files,  5807 codes, 1032 comments, 1081 blanks, all 7920 lines

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [Compiler/Compiler.cs](/Compiler/Compiler.cs) | C# | 49 | 17 | 18 | 84 |
| [Compiler/Context/Context.cs](/Compiler/Context/Context.cs) | C# | 21 | 1 | 5 | 27 |
| [Compiler/Context/Identifiers.cs](/Compiler/Context/Identifiers.cs) | C# | 5 | 0 | 1 | 6 |
| [Compiler/Context/Loader.cs](/Compiler/Context/Loader.cs) | C# | 112 | 6 | 22 | 140 |
| [Compiler/Context/Loader/01 Bind.cs](/Compiler/Context/Loader/01%20Bind.cs) | C# | 130 | 24 | 28 | 182 |
| [Compiler/Context/Loader/02 BindData.cs](/Compiler/Context/Loader/02%20BindData.cs) | C# | 290 | 38 | 51 | 379 |
| [Compiler/Context/Loader/03 Ruleset.cs](/Compiler/Context/Loader/03%20Ruleset.cs) | C# | 21 | 6 | 5 | 32 |
| [Compiler/Context/Loader/Loader.cs](/Compiler/Context/Loader/Loader.cs) | C# | 25 | 3 | 4 | 32 |
| [Compiler/Default/Bindings.cs](/Compiler/Default/Bindings.cs) | C# | 9 | 0 | 1 | 10 |
| [Compiler/Default/Lexer/Bindings.cs](/Compiler/Default/Lexer/Bindings.cs) | C# | 58 | 4 | 15 | 77 |
| [Compiler/Default/Lexer/Rules.cs](/Compiler/Default/Lexer/Rules.cs) | C# | 125 | 3 | 22 | 150 |
| [Compiler/Default/Parser/Bindings.cs](/Compiler/Default/Parser/Bindings.cs) | C# | 30 | 2 | 7 | 39 |
| [Compiler/Default/Parser/Patterns/Groups.cs](/Compiler/Default/Parser/Patterns/Groups.cs) | C# | 57 | 4 | 6 | 67 |
| [Compiler/Default/Parser/Patterns/Shortcuts.cs](/Compiler/Default/Parser/Patterns/Shortcuts.cs) | C# | 9 | 0 | 2 | 11 |
| [Compiler/Default/Parser/Patterns/TokenTables.cs](/Compiler/Default/Parser/Patterns/TokenTables.cs) | C# | 52 | 1 | 9 | 62 |
| [Compiler/Default/Parser/Patterns/Tokens.cs](/Compiler/Default/Parser/Patterns/Tokens.cs) | C# | 36 | 4 | 6 | 46 |
| [Compiler/Default/Parser/Realms.cs](/Compiler/Default/Parser/Realms.cs) | C# | 7 | 0 | 2 | 9 |
| [Compiler/Default/Parser/Rules.cs](/Compiler/Default/Parser/Rules.cs) | C# | 409 | 11 | 44 | 464 |
| [Compiler/Default/Parser/Tags.cs](/Compiler/Default/Parser/Tags.cs) | C# | 7 | 0 | 2 | 9 |
| [Compiler/Diagnostics/Config.cs](/Compiler/Diagnostics/Config.cs) | C# | 0 | 29 | 0 | 29 |
| [Compiler/Diagnostics/Diagnostics.cs](/Compiler/Diagnostics/Diagnostics.cs) | C# | 53 | 0 | 5 | 58 |
| [Compiler/Diagnostics/Exceptions.cs](/Compiler/Diagnostics/Exceptions.cs) | C# | 2 | 0 | 1 | 3 |
| [Compiler/Diagnostics/Render.cs](/Compiler/Diagnostics/Render.cs) | C# | 0 | 521 | 0 | 521 |
| [Compiler/Models/Source.cs](/Compiler/Models/Source.cs) | C# | 42 | 0 | 6 | 48 |
| [Compiler/Models/TASI.cs](/Compiler/Models/TASI.cs) | C# | 207 | 17 | 37 | 261 |
| [Compiler/Models/TAST.cs](/Compiler/Models/TAST.cs) | C# | 460 | 32 | 83 | 575 |
| [Compiler/Pipeline/01 Loader/01 Restore.cs](/Compiler/Pipeline/01%20Loader/01%20Restore.cs) | C# | 31 | 3 | 6 | 40 |
| [Compiler/Pipeline/01 Loader/02 Load.cs](/Compiler/Pipeline/01%20Loader/02%20Load.cs) | C# | 19 | 1 | 4 | 24 |
| [Compiler/Pipeline/01 Loader/Process.cs](/Compiler/Pipeline/01%20Loader/Process.cs) | C# | 25 | 2 | 4 | 31 |
| [Compiler/Pipeline/02 Lexer/01 Lex/Context.cs](/Compiler/Pipeline/02%20Lexer/01%20Lex/Context.cs) | C# | 32 | 3 | 7 | 42 |
| [Compiler/Pipeline/02 Lexer/01 Lex/Phase.cs](/Compiler/Pipeline/02%20Lexer/01%20Lex/Phase.cs) | C# | 55 | 9 | 13 | 77 |
| [Compiler/Pipeline/02 Lexer/Context.cs](/Compiler/Pipeline/02%20Lexer/Context.cs) | C# | 15 | 1 | 2 | 18 |
| [Compiler/Pipeline/02 Lexer/Process.cs](/Compiler/Pipeline/02%20Lexer/Process.cs) | C# | 20 | 3 | 6 | 29 |
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
| [Compiler/Pipeline/03 Parser/04 Emit/Context.cs](/Compiler/Pipeline/03%20Parser/04%20Emit/Context.cs) | C# | 68 | 4 | 13 | 85 |
| [Compiler/Pipeline/03 Parser/04 Emit/Phase.cs](/Compiler/Pipeline/03%20Parser/04%20Emit/Phase.cs) | C# | 41 | 4 | 7 | 52 |
| [Compiler/Pipeline/03 Parser/Context.cs](/Compiler/Pipeline/03%20Parser/Context.cs) | C# | 41 | 6 | 10 | 57 |
| [Compiler/Pipeline/03 Parser/Mutate/Context.cs](/Compiler/Pipeline/03%20Parser/Mutate/Context.cs) | C# | 71 | 5 | 15 | 91 |
| [Compiler/Pipeline/03 Parser/Mutate/Step.cs](/Compiler/Pipeline/03%20Parser/Mutate/Step.cs) | C# | 99 | 9 | 19 | 127 |
| [Compiler/Pipeline/03 Parser/Process.cs](/Compiler/Pipeline/03%20Parser/Process.cs) | C# | 26 | 3 | 7 | 36 |
| [Compiler/Pipeline/03 Parser/Semantics/Context.cs](/Compiler/Pipeline/03%20Parser/Semantics/Context.cs) | C# | 10 | 0 | 4 | 14 |
| [Compiler/Pipeline/03 Parser/Semantics/Tags.cs](/Compiler/Pipeline/03%20Parser/Semantics/Tags.cs) | C# | 147 | 13 | 26 | 186 |
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
| [Compiler/Pipeline/04 Lowerer/Process.cs](/Compiler/Pipeline/04%20Lowerer/Process.cs) | C# | 24 | 3 | 6 | 33 |
| [Compiler/Project.cs](/Compiler/Project.cs) | C# | 57 | 0 | 13 | 70 |
| [Compiler/Rules/Assembly.cs](/Compiler/Rules/Assembly.cs) | C# | 21 | 5 | 9 | 35 |
| [Compiler/Rules/Bindings.cs](/Compiler/Rules/Bindings.cs) | C# | 21 | 0 | 4 | 25 |
| [Compiler/Rules/Bindings/Lexer.cs](/Compiler/Rules/Bindings/Lexer.cs) | C# | 20 | 0 | 6 | 26 |
| [Compiler/Rules/Bindings/Parser.cs](/Compiler/Rules/Bindings/Parser.cs) | C# | 80 | 3 | 25 | 108 |
| [Compiler/Rules/Context.cs](/Compiler/Rules/Context.cs) | C# | 15 | 0 | 4 | 19 |
| [Compiler/Rules/Context/Lexer.cs](/Compiler/Rules/Context/Lexer.cs) | C# | 30 | 3 | 5 | 38 |
| [Compiler/Rules/Context/Parser.cs](/Compiler/Rules/Context/Parser.cs) | C# | 76 | 4 | 11 | 91 |
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