using luval.rpa.common.extractors.bp;
using luval.rpa.common.model.bp;
using luval.rpa.common.rules;
using luval.rpa.common.rules.configuration;
using luval.rpa.rules.bp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace luval.rpa.console
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;

            Console.WriteLine("Hello from rpa console app. \nFile: " + args[0]);
            var file = args[0];
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            MainExec process = new MainExec(file);
            process.Run();
        }

        class MainExec
        {
            private readonly string _fileName;
            private readonly Release _release;
            private RuleProfile _profile;

            public MainExec(string filename)
            {
                _fileName = filename;
                _profile = RuleProfile.LoadFromFile();
                _profile.Rules = _profile.Rules.Distinct().ToList();
                _profile.Save();

                if (!File.Exists(_fileName))
                {
                    throw new FileNotFoundException(string.Format("File {0} could not be found", _fileName));
                }
                var extractor = new ReleaseExtractor(File.ReadAllText(_fileName));
                extractor.Load();
                _release = extractor.Release;
            }
            public void Run()

            {
                ExecuteRules();
            }
            private object ExecuteRules()
            {
                var profile = RuleProfile.LoadFromFile();
                var ruleEngine = new BPRunner();
                ruleEngine.RuleRun += RuleEngine_RuleRun;
                var rules = ruleEngine.GetRulesFromProfile(profile);
                var results = ruleEngine.RunRules(profile, _release, rules.ToList());
                var filename = SaveReport(profile, rules, results, _release);
                Reports.RunReport_no_delegate(filename);
                return null;
            }
            private string SaveReport(RuleProfile profile, IEnumerable<IRule> rules, IEnumerable<Result> results, Release release)
            {
                var fileName = (!string.IsNullOrWhiteSpace(release.Name) ? release.Name : @"result") + ".xlsx";
                if (string.IsNullOrWhiteSpace(fileName)) return "result.xlsx";
                var fileInfo = new FileInfo(fileName);
                release.GetAnalysisUnits();
                ExcelOutputGenerator temp = new ExcelOutputGenerator();
                temp.CreateReport(fileInfo.FullName, profile, rules, results, release);
                return fileName;
            }

            private void RuleEngine_RuleRun(object sender, RunnerMessageEventArgs e)
            {
            }

        }
    }
}