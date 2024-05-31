using System.Collections.Generic;

namespace Auditor
{
    public class MetaMorpheusRunType 
    {
        public string Label { get; private set; }
        public string PsmResultTextFinder { get; private set; }
        public string PeptideResultTextFinder { get; private set; }
        public string TimeResultTextFinder { get; private set; } = "Time to run task:";

        public static Dictionary<string, MetaMorpheusRunType> RunTypes { get; }

        static MetaMorpheusRunType()
        {
            RunTypes = new Dictionary<string, MetaMorpheusRunType>()
            {
                {Program.ClassicSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.ClassicSearchLabel,
                    PsmResultTextFinder = "All target PSM",
                    PeptideResultTextFinder = "All target pept",
                }},
                {Program.ModernSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.ModernSearchLabel,
                    PsmResultTextFinder = "All target PSM",
                    PeptideResultTextFinder = "All target pept",
                }},
                {Program.SemiSpecificSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.SemiSpecificSearchLabel,
                    PsmResultTextFinder = "All target PSM",
                    PeptideResultTextFinder = "All target pept",
                }},
                {Program.NonspecificSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.NonspecificSearchLabel,
                    PsmResultTextFinder = "All target PSM",
                    PeptideResultTextFinder = "All target pept",
                }},
                {Program.CrosslinkSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.CrosslinkSearchLabel,
                    PsmResultTextFinder = "All target PSM",
                    PeptideResultTextFinder = "All target pept",
                }},
                {Program.TopDownSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.TopDownSearchLabel,
                    PsmResultTextFinder = "All target PSM",
                    PeptideResultTextFinder = "All target proteo",
                }},
                {Program.GlycoSearchLabel, new MetaMorpheusRunType()
                {
                    Label = Program.GlycoSearchLabel,
                    PsmResultTextFinder = "PSMs within 1%",
                    PeptideResultTextFinder = "All target pept",
                }},
            };
        }
    }
}
