using System.Diagnostics;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Project;

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
        //PROCESSES
        public static ParserProcess NewProcess() => new();
        public static void EndProcess(this ParserProcess process) { }

        //===== RULES =====
        public static readonly List<string> Realms = ["Virtual"];
        public static readonly List<RuleInfo> _ruleInfos = [];
        public static readonly Dictionary<Type, int> _rulesByType = [];

        public static int AddRuleInfo<R>(int ruleId, bool isClass)
        {
            var id = _ruleInfos.Count;

            _ruleInfos.Add(new(ruleId, typeof(R).Name, isClass));
            _rulesByType[typeof(R)] = id;
            return id;
        }
        public static RuleInfo GetRuleInfo(int id)
        => _ruleInfos[id];
        public static string? GetRuleName(int id)
        => _ruleInfos[id].RuleName;

        //RULE MONOS
        public static readonly List<Rule> _rules = [];

        public static R GetRule<R>() where R : Rule
        => GetRule<R>(GetRuleId<R>());
        public static R GetRule<R>(int id) where R : Rule
        => (R)GetRule(id);
        public static Rule GetRule(int id)
        => GetRule(_ruleInfos[id]);
        internal static Rule GetRule(RuleInfo info)
        {
            if (info.IsClass)
                throw new Exception($"ID IS NOT MONO RULE");

            return _rules[info.RuleId];
        }

        public static int GetRuleId<R>() where R : Rule => _rulesByType[typeof(R)];

        //RULE CLASSES
        public static readonly List<RuleClass> _ruleClasses = [];

        public static C GetRuleClass<C>() where C : RuleClass
        => GetRuleClass<C>(GetRuleClassId<C>());
        public static C GetRuleClass<C>(int id) where C : RuleClass
        => (C)GetRuleClass(id);
        public static RuleClass GetRuleClass(int id)
        => GetRuleClass(_ruleInfos[id]);
        internal static RuleClass GetRuleClass(RuleInfo info)
        {
            if (!info.IsClass)
                throw new Exception($"ID IS NOT RULE CLASS");

            return _ruleClasses[info.RuleId];
        }

        public static int GetRuleClassId<C>() where C : RuleClass => _rulesByType[typeof(C)];
    }
    public static class ParserRealms
    {
        public const int VIRTUAL = 0;
    }

    public class RuleInfo(int ruleId, string ruleName, bool isClass)
    {
        public readonly int RuleId = ruleId;
        public readonly string RuleName = ruleName;

        public readonly bool IsClass = isClass;
    }
}