using DrzSharp.Compiler.Default.Lexer;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

//EXAMPLE 1
public class T1 : ParserRule<TestRuleInst>
{
    public T1()
    {
        SetPatterns(
            new TokenPattern()
            .Token(TokenType.Keyword, val: "A")
            .Token(TokenType.Keyword, val: "B")
            .Token(TokenType.Keyword, val: "C")
            .Token(TokenType.Keyword, val: "D")
        );
    }
}

//EXAMPLE 2
public class T2 : ParserRule<TestRuleInst>
{
    public T2()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .Optional(t => t.TKeyword(val: "B"))
            .TKeyword(val: "C")
        );
    }
}

//EXAMPLE 3
public class T3 : ParserRule<TestRuleInst>
{
    public T3()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .Repeat(t => t.TKeyword(val: "B"))
            .TKeyword(val: "C")
        );
    }
}

//EXAMPLE 4
public class T4 : ParserRule<TestRuleInst>
{
    public T4()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .Rule<T4Inner>()
            .TKeyword(val: "D")
        );
    }
}
public class T4Inner : ParserRule<TestRuleInst>
{
    public T4Inner()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "B")
            .TKeyword(val: "C")
        );
    }
}

//EXAMPLE 5
public class T5 : ParserRule<TestRuleInst>
{
    public T5()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .Repeat(t => t.Rule<T4Inner>())
            .TKeyword(val: "D")
        );
    }
}

//EXAMPLE 6
public class T6 : ParserRule<TestRuleInst>
{
    public T6()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .Or(
                t => t.TKeyword(val: "B").TKeyword(val: "C"), 
                t => t.TKeyword(val: "B")
            )
            .TKeyword(val: "D")
        );
    }
}

//EXAMPLE 7
public class T7 : ParserRule<TestRuleInst>
{
    public T7()
    {
        SetPatterns(
            new TokenPattern()
            .Or(
                t => t.Rule<X>(),
                t => t.Rule<Y>()
            )
        );
    }
}
public class X : ParserRule<TestRuleInst>
{
    public X()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .TKeyword(val: "B")
            .TKeyword(val: "C")
        );
    }
}
public class Y : ParserRule<TestRuleInst>
{
    public Y()
    {
        SetPatterns(
            new TokenPattern()
            .TKeyword(val: "A")
            .TKeyword(val: "B")
        );
    }
}

//EXAMPLE 8
public class T8 : ParserRule<TestRuleInst>
{
    public T8()
    {
        SetPatterns(
            new TokenPattern()
            .Rule<A>(captureTag: "A")
            .Rule<B>(captureTag: "B")
            .Rule<C>(captureTag: "C")
        );
    }
}
public class A : ParserRule<TestRuleInst>
{
    public A() => SetPatterns(new TokenPattern().TKeyword(val: "A", captureTag: "x"));
}
public class B : ParserRule<TestRuleInst>
{
    public B() => SetPatterns(new TokenPattern().TKeyword(val: "B", captureTag: "x"));
}
public class C : ParserRule<TestRuleInst>
{
    public C() => SetPatterns(new TokenPattern().TKeyword(val: "C", captureTag: "x"));
}

public class TestRuleInst : ParserRuleInstance { }

/*
public class ForRule : ParserRule<ForRuleInst>
{
    public ForRule()
    {
        SetPatterns(
            new TokenPattern()
            .Token(TokenType.Keyword, val: "for")
            .Token(TokenType.OpParen)
            .ClosedGroup(captureTag: "init")
            .Token(TokenType.Operator, val: ",")
            .ClosedGroup(captureTag: "cond")
            .Token(TokenType.Operator, val: ",")
            .ClosedGroup(captureTag: "inc")
            .Token(TokenType.ClParen)
            .Token(TokenType.OpBrace)
            .ClosedGroup(captureTag: "body")
            .Token(TokenType.ClBrace),
            new TokenPattern().ClosedGroup(captureTag: "myVar"),
            new TokenPattern("myVar").Token(TokenType.Keyword, val: "for")
        );

        SetPatterns(
            new TokenPattern().Rule<ForRule>(captureTag: "myForRule"),
            new TokenPattern("myForRule", "aVarInsideMyForRule")
        );
    }

    public override void OnInstantiate(ParserMatchToolkit tk, ForRuleInst inst)
    {
        inst.Init = tk.LoadVar("init");
        inst.Cond = tk.LoadVar("cond");
        inst.Inc = tk.LoadVar("inc");
        inst.Body = tk.LoadVar("body");
    }
}

public class ForRuleInst : ParserRuleInstance
{
    public TokenSpan Init;
    public TokenSpan Cond;
    public TokenSpan Inc;
    public TokenSpan Body;
}
*/