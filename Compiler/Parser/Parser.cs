using System.Diagnostics;
using DrzSharp.Compiler.Core;
using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler
{
    public partial class Compiler
    {
        public static ParserProcess NewParser() => ParserManager.NewProcess();
        public static void ParseProject(DzProject project)
        {
            var par = NewParser();
            par.ParseProject(project);
            par.EndProcess();
        }
    }
}

namespace DrzSharp.Compiler.Parser
{
    internal static class ParserManager
    {
        //RULES
        public static readonly List<ParserRule> _rules = [];
        public static readonly Dictionary<Type, RuleId> _rulesByType = [];
        public static R GetRule<R>() where R : ParserRule
        => GetRule<R>(GetRuleId<R>());
        public static R GetRule<R>(RuleId id) where R : ParserRule
        => (R)GetRule(id);
        public static ParserRule GetRule(RuleId id)
        {
            Debug.Assert(!id.IsClass);
            return _rules[id.Id];
        }
        public static RuleId GetRuleId<R>() where R : ParserRule
        => _rulesByType[typeof(R)];

        //RULE CLASSES
        public static readonly List<ParserRuleClass> _ruleClasses = [];
        public static readonly Dictionary<Type, RuleId> _ruleClassesByType = [];
        public static C GetRuleClass<C>() where C : ParserRuleClass
        => GetRuleClass<C>(GetRuleClassId<C>());
        public static C GetRuleClass<C>(RuleId id) where C : ParserRuleClass
        => (C)GetRuleClass(id);
        public static ParserRuleClass GetRuleClass(RuleId id)
        {
            Debug.Assert(id.IsClass);
            return _ruleClasses[id.Id];
        }
        public static RuleId GetRuleClassId<C>() where C : ParserRuleClass
        => _ruleClassesByType[typeof(C)];

        //PROCESSES
        public static ParserProcess NewProcess() => new();
        public static void EndProcess(this ParserProcess process) { }
    }
}