using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using luval.rpa.common.model;
using luval.rpa.common.rules;
using luval.rpa.common.rules.attributes;
using luval.rpa.common.model.bp;

namespace luval.rpa.rules.bp
{
    [Name("Stage Count On Page"),
     Description("Checks that a reasonable number of stages are in used on a page")]
    public class StageCountOnPage : BPRuleBase, IRule
    {
        private int _maxCount;
        private int _maxCountWarning;

        public StageCountOnPage()
        {
            _maxCount = GetMaxCount();
            _maxCountWarning = GetMaxCountWarning();
        }

        public override IEnumerable<Result> Execute(Release release)
        {
            var res = new List<Result>();
            ValidateRule(res, release.Processes, "Process");
            ValidateRule(res, release.Objects, "Object");
            return res;
        }

        private void ValidateRule(List<Result> res, IEnumerable<PageBasedStage> items, string scope)
        {
            foreach (var i in items)
            {
                ValidateStages(res, i.MainPage, i.Name, scope, "Main");
                foreach (var p in i.Pages)
                {
                    ValidateStages(res, p.Stages, i.Name, scope, p.Name);
                }
            }
        }

        private void ValidateStages(List<Result> res, IEnumerable<Stage> stages, string parent, string scope, string page)
        {
            if (IsValid(stages))
                return;
            res.Add(new Result()
            {
                RuleName = Name,
                RuleDescription = GetRuleDescription(),
                Scope = scope,
                Page = page,
                Parent = parent,
                Type = FilterStages(stages).Count() >= _maxCount ? ResultType.Error : ResultType.Warning, //to be optimized I guess
                Message = string.Format("Stages on the page exceed the established max {0}", FilterStages(stages).Count() >= _maxCount ? _maxCount : _maxCountWarning)
            }); ; ; ;
        }

        private IEnumerable<Stage> FilterStages(IEnumerable<Stage> stages)
        {
            var types = new[] { "Start", "End", "Data", "Anchor", "Note", "Block" };
            return stages.Where(i => !types.Contains(i.Type)).ToList();
        }

        private bool IsValid(IEnumerable<Stage> stages)
        {
            return FilterStages(stages).Count() <= _maxCountWarning;
        }

        private int GetMaxCount()
        {
            return GetSetting<int>("MaxCount", 50);
        }

        private int GetMaxCountWarning()
        {
            return GetSetting<int>("MaxCountWarning", 30);
        }
    }
}
