# MATCH PHASE

## Phase Purpose
Define how token sequences are recognized and converted into Rule instances.

## Steps
Match is segmented in two steps:
- Pattern Matching
- Building

### PATTERN MATCHING
We're creating our `Var Declaration` Rule. And we want it to match

```dzsharp
var x = 0
```

So, we're going to define a pattern specifically for it, like:

`keyword ("var") + keyword ("x") + operator ("=") + numberLiteral ("0")`

```csharp
//This is done on the class constructor
SetPattern(t => t
    .kw("var").kw("x").oper("=").numberLit("0")
);
```

Yet, this pattern is extremely rigid. In case we want to match this too

```dzsharp
var y = 1
```

We'll be needing to whether:
- Create a new pattern, but we can't afford create a new pattern for every possible user input.
- Or make the pattern more generic, allowing a single pattern match multiple inputs. Which is what we're going to do.

`keyword ("var") + keyword (any value) + operator ("=") + numberLiteral (any value)`

```csharp
SetPattern(t => t
    .kw("var").kw().oper("=").numberLit()
);
```

It works, but we've no way to know which is the name of the variable or which is the number written. So we must save that information.

`keyword ("var") + keyword (save value as "varName") + operator ("=") + numberLiteral (save value as "numberVal")`

```csharp
//We save values with under a "tag", a int value which must not repeat on the local pattern unless intentional
const int varName = 0;
const int numberVal = 1;

SetPattern(t => t
    .kw("var").kw(captureTag: varName).oper("=").numberLit(captureTag: numberVal)
);
```

And load it (in the instance created after a succesful matching) so we can access them later.

`load "varName" and save in Instance`
`load "numberVal" and save in Instance`

```csharp
//This is done inside a method called "OnInstantiate()"
inst.VarName = view.LoadTokenVar(varName);
inst.NumberVal = view.LoadTokenVar(numberVal);
```

We got something solid. But we might not always want to set our variable "x" to a number, instead, we might want too

```dzsharp
var y = 0 + 2
```

or even

```dzsharp
var x = 1
var y = x + 2
```

Meaning that our pattern must be of the shape

`keyword ("var") + keyword (save value as "varName") + operator ("=") + expression`

With `expression` being whatever can be written on the right side.

So, let's define a "Expression" Rule Class, which will aggroupate every single "expression" pattern that can be written

```csharp
class ExprRule : RuleClass<Expr> {}
class Expr : RuleInstance {}
```

And rewrite our pattern code into

```csharp
const int varName = 0;
const int expr = 1;

SetPattern(t => t
    .kw("var").kw(captureTag: varName).oper("=").Rule<ExprRule>(captureTag: expr)
);

//Inside "OnInstantiate()"
inst.VarName = view.LoadTokenVar(varName);
inst.Expr = view.LoadRuleVar<Expr>(expr);
```

Now to add a new `expression` for the right side, we only need to create a rule and make it be a part of our new `ExprRule` Class.

### BUILDING
We're creating our `If Statement` Rule. And we want it to match

```dzsharp
if (true) {
    ...
}
```

So we do

`keyword ("if") + opener parentheses + expression + closer parentheses + opener brackets + body + closer brackets`

```csharp
const int expr = 0;
const int body = 1;

SetPattern(t => t
    .kw("if").oparen().Rule<ExprRule>(captureTag: expr).cparen().obrack().Body(captureTag: body).cbrack()
);

//Inside "OnInstantiate()"
inst.Expr = view.LoadRuleVar<Expr>(expr);
inst._body = view.LoadVar(body);
```

And while this works, we must tell the Compiler what do we mean by "Body", and that it should try to keep matching new rules, being able to match things like

```dzsharp
if (true) {
    var x = 0
    print(x)
}
```

without problems.

While we could use the Repeat() and Realm() patterns to achieve this

```csharp
SetPattern(t => t
    .kw("if").oparen().Rule<ExprRule>(captureTag: expr).cparen().obrack().Repeat(t => t.Realm(Realms.Logic)).cbrack()
);
```

It is better to delegate the work to the Compiler instead of doing it ourselves, as we might overflow the Compiler's match-time vars memory suffer from poor performance, and increase pattern complexity if we want to manage wrong-written inputs like

```dzsharp
if (true) {
    let x = 0
}
```

instead of

```dzsharp
if (true) {
    var x = 0
}
```

To delegate the work to the Compiler we must use the "OnNest()" method

```csharp
//Inside "OnNest()"
Body = ctx.NestSpan(_body);

//The method NestSpan returns the Nested Node Id, which we'll use instead of the raw TokenSpan we get from "OnInstantiate()"
```