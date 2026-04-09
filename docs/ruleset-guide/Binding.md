# RULE DEFINITIONS & BINDING

Rules are defined as classes (or methods), and must follow specific conventions to maintain consistency.

Though, defining Rules isn't enough, we need also to bind them to the Compiler, so they can actually be used.
For that we create a `BIND ENTRYPOINT` function.


```csharp
public static void Bind(BindingContext ctx)
{
    ...
}
```

`BindingContext` carries the Binding API.

## Index

- Parser
  - Realms
  - Rule
  - Rule Class

### (Parser) Realms
Realms are language IDs we group Rules by, and use to tell the compiler which Rules should be tried to match.

The default one is `Model.Realms.VIRTUAL`, this is the Realm the whole file token structure belongs by default. You can change the Realm of certain tokens in the Matching phase.

In order to define a new Realm we give it a name. For example `Logic`, for all the "Logic" in the Program (such as math operations, method calls, etc).

```csharp
//Inside the BIND ENTRYPOINT
ctx.Parser.AddRealm("Logic");
```

By convention, is better to store that name in a constant, as it'll be used frequently.

```csharp
static class Realms {
    public const string Logic = "Logic";
}

ctx.Parser.AddRealm(Realms.Logic);
```

### (Parser) Rule
A Rule has two parts, the matcher (the one the Compiler uses to match token sequences)

```csharp
class AdditionRule : Rule<Addition> {
    ...
}
```

And the instance (the one the Compiler creates when your token pattern is matched)

```csharp
class Addition : RuleInstance {
    ...
}
```

By convention, the matcher part's name is your {Rule Name} plus the word "Rule" at the end, and the instance part's name is your {Rule Name}.

In order to bind that Rule we use the `BindRule<>()` method. It has two arguments:
- Realm: The "language ID" this Rule belongs to.
- IsAbstract: If false, the Rule will be tried automatically by the compiler. If true, this Rule can only be matched if another pattern tries it (or the Rule class it belongs to) specifically.

```csharp
//Inside the BIND ENTRYPOINT
ctx.Parser.BindRule<AdditionRule>(Realms.Logic, false);
```

And in case we want it to be inside a aggroupation Rule class such as `SingleExprRule`.

```csharp
//Here we don't define the Realm, as it is inherited from the Rule class
ctx.Parser.BindRule<AdditionRule, SingleExprRule>(false);
```

### (Parser) Rule Class
Rule Classes are another way we can aggroupate Rules, it isn't mandatory as Realms, but it is really useful for composability. They have two parts, the header (the one you bind to the Compiler and call inside patterns)

```csharp
class ExprRule : RuleClass {
    ...
}
```

And the instance (the one all the Rules and Rule Classes which want to be part of this Rule Class MUST extend to)

```csharp
class Expr : RuleInstance {
    ...
}
```

By convention, the header part's name is your {Rule Name} plus the word "Rule" at the end, and the instance part's name is your {Rule Name}.

To bind that Rule Class we use the `BindRuleClass<>()` method. It has a single argument:
- Realm: The "language ID" this Rule belongs to.

```csharp
//Inside the BIND ENTRYPOINT
ctx.Parser.BindRuleClass<ExprRule>(Realms.Logic);
```

And in case we want it to be inside another Rule Class, such as `SingleExprRule` inside `ExprRule`.

```csharp
//Here we don't define the Realm, as it is inherited from the outer Rule class
ctx.Parser.BindRuleClass<SingleExprRule, ExprRule>();
```