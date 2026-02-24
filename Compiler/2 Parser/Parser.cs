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
        //PHASES
        public static ParserPhase[] _phases = [
            new(Phases.VirtualPhase),
            new(Phases.LogicPhase)
        ];

        //RULES
        public static readonly List<Rule> _rules = [];
        public static readonly Dictionary<Type, RuleId> _rulesByType = [];
        public static R GetRule<R>() where R : Rule
        => GetRule<R>(GetRuleId<R>());
        public static R GetRule<R>(RuleId id) where R : Rule
        => (R)GetRule(id);
        public static Rule GetRule(RuleId id)
        {
            Debug.Assert(!id.IsClass);
            return _rules[id.Id];
        }
        public static RuleId GetRuleId<R>() where R : Rule
        => _rulesByType[typeof(R)];

        //RULE CLASSES
        public static readonly List<RuleClass> _ruleClasses = [];
        public static readonly Dictionary<Type, RuleId> _ruleClassesByType = [];
        public static C GetRuleClass<C>() where C : RuleClass
        => GetRuleClass<C>(GetRuleClassId<C>());
        public static C GetRuleClass<C>(RuleId id) where C : RuleClass
        => (C)GetRuleClass(id);
        public static RuleClass GetRuleClass(RuleId id)
        {
            Debug.Assert(id.IsClass);
            return _ruleClasses[id.Id];
        }
        public static RuleId GetRuleClassId<C>() where C : RuleClass
        => _ruleClassesByType[typeof(C)];

        //PROCESSES
        public static ParserProcess NewProcess() => new();
        public static void EndProcess(this ParserProcess process) { }
    }
    public static class Phases
    {
        public const byte VIRTUAL = 0;
        public const byte LOGIC = 1;

        //PHASE IMPLEMENTATIONS
        internal static void VirtualPhase(ParserProcess proc)
        {
            proc.ForeachSite(proc.Match);
            proc.ForeachSite(s =>
            {
                proc.Validate(s);
                proc.Emit(s);
            });
        }

        internal static void LogicPhase(ParserProcess proc)
        {
            proc.ForeachSite(s =>
            {
                proc.Match(s);
                proc.Validate(s);
                proc.Emit(s);
            });
        }
    }
    public class ParserPhase(Action<ParserProcess> phase)
    {
        internal readonly Action<ParserProcess> phase = phase;
        internal byte realmCount = 0;
    }
}