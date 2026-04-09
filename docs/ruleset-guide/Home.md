# DRIZA# Ruleset-guide
> Status: Draft v0 — subject to change

## What is Driza#?
Driza# is a composable rule-based Compiler, think of it as a "Framework" to build programming languages. The Compiler is just an engine, a structure, the Rules that you "feed" it are the ones that define the language.

Driza#'s main philosophy is "Symmetrical Extensibility", in other words, the same API and Engine that supports language extensions can also support writing a new language from scratch, without anything else than the Compiler Engine itself.

## What is a Rule?
A Rule is a compiler unit that defines behavior across a `3-step-process`:

Match -> Validate -> Transform

Rules are composable, which means you create base Rules and Classes of Rules, and just reuse them when creating new features.

## 3-Step-Process Overview
- Match: Matches token sequences through patterns, and builds a tree structure with instances created from the matched Rules.
- Validate: Ensures semantic correctness.
- Transform: Transform the matched token sequence into a "lower-level" representation.