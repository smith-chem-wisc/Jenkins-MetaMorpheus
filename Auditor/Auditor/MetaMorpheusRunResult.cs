using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auditor
{
    public class MetaMorpheusRunResult
    {
        // path to allResults.txt
        public Dictionary<string, FileInfo> AllResultsTexts { get; private set; }

        // time of run folder creation
        public DateTime DateTime { get; private set; }

        // PSM results
        public int InitialSearchTargetPsms { get; private set; }
        public int InitialSearchTargetPeptides { get; private set; }
        public int PostCalibrationTargetPsms { get; private set; }
        public int PostCalibrationTargetPeptides { get; private set; }
        public int PostGptmdTargetPsms { get; private set; }
        public int PostGptmdTargetPeptides { get; private set; }
        public int SemiSpecificPsms { get; private set; }
        public int SemiSpecificPeptides { get; private set; }
        public int NonSpecificPsms { get; private set; }
        public int NonSpecificPeptides { get; private set; }
        public int InterlinkCsms { get; private set; }
        public int IntralinkCsms { get; private set; }
        public int LoopCsms { get; private set; }
        public int DeadendCsms { get; private set; }
        public int CrosslinkSinglePsms { get; private set; }
        public int CrosslinkSinglePeptides { get; private set; }
        public int ModernSearchPsms { get; private set; }
        public int ModernSearchPeptides { get; private set; }
        public int GlycoSearchPsms { get; private set; }
        public int GlycoSearchPeptides { get; private set; }
        public double TopDownInitialSearchPsms { get; private set; }
        public double TopDownInitialSearchProteoforms { get; private set; }
        public double TopDownPostCalibrationSearchPsms { get; private set; }
        public double TopDownPostCalibrationSearchProteoforms { get; private set; }
        public double TopDownPostAveragingSearchPsms { get; private set; }
        public double TopDownPostAveragingSearchProteoforms { get; private set; }
        public double TopDownPostGPTMDSearchPsms { get; private set; }
        public double TopDownPostGPTMDSearchProteoforms { get; private set; }

        // time to run each of the tasks
        public double InitialSearchTimeInSeconds { get; private set; }
        public double CalibrationTimeInSeconds { get; private set; }
        public double PostCalibrationSearchTimeInSeconds { get; private set; }
        public double GptmdTimeInSeconds { get; private set; }
        public double PostGptmdSearchTimeInSeconds { get; private set; }
        public double SemiSpecificSearchTimeInSeconds { get; private set; }
        public double NonSpecificSearchTimeInSeconds { get; private set; }
        public double CrosslinkSearchTimeInSeconds { get; private set; }
        public double ModernSearchTimeInSeconds { get; private set; }
        public double GlycoSearchTimeInSeconds { get; private set; }
        public double TopDownInitialSearchTimeInSeconds { get; private set; }
        public double TopDownCalibrationTimeInSeconds { get; private set; }
        public double TopDownPostCalibrationSearchTimeInSeconds { get; private set; }
        public double TopDownAveragingTimeInSeconds { get; private set; }
        public double TopDownPostAveragingSearchTimeInSeconds { get; private set; }
        public double TopDownGptmdTimeInSeconds { get; private set; }
        public double TopDownPostGPTMDSearchTimeInSeconds { get; private set; }

        // protein groups for initial search (task 1)
        public int InitialSearchProteinGroups { get; private set; }

        /// <summary>
        /// There will be 4 allResults.txt files
        /// Search/Calib/Search/GPTMD/Search/ModernSearch/SemiSearch, Nonspecific, Crosslink, Topdown
        /// </summary>
        /// <param name="allResultsTxtPath"></param>
        /// <param name="dir"></param>
        public MetaMorpheusRunResult(Dictionary<string, FileInfo> resultLabelToFileInfo, DateTime timestamp)
        {
            DateTime = timestamp;
            AllResultsTexts = resultLabelToFileInfo;

            ParseResults();
        }

        /// <summary>
        /// CASES ARE:
        /// 1. Initial search
        /// 2. Calibration
        /// 3. Post-calibration search
        /// 4. GPTMD
        /// 5. Post-GPTMD search
        /// 6. Modern Search
        /// 7. Semi-specific search
        /// 8. Non-specific search
        /// 9. Crosslink search
        /// 10. TopDown Intitial Search 
        /// 11. Top Down Calibration
        /// 12. Top Down Post-calibration search 
        /// 13. Top Down Spectral Averaging
        /// 14. Top Down Post-Averaging Search
        /// 15. Top Down GPTMD
        /// 16. Top Down Post-GPTMD search
        /// 17. Glyco
        /// </summary>
        private void ParseResults()
        {
            // run folder exists but allResults.txt is not present; this indicates a failed run (everything is set to 0)
            if (AllResultsTexts.All(p => p.Value == null))
            {
                return;
            }

            foreach (var allResultsTxtFile in AllResultsTexts)
            {
                int taskNumberReading = 0;

                if (allResultsTxtFile.Value == null)
                {
                    continue;
                }

                var lines = File.ReadAllLines(allResultsTxtFile.Value.FullName);
                foreach (var line in lines)
                {
                    if (line.Contains("Time to run task:"))
                    {
                        taskNumberReading++;

                        var stringTime = line.Replace("Time to run task:", "").Trim();
                        var time = TimeSpan.Parse(stringTime);
                        double timeInSeconds = time.TotalSeconds;

                        switch (allResultsTxtFile.Key)
                        {
                            case Program.ClassicSearchLabel:
                                switch (taskNumberReading)
                                {
                                    case 1:
                                        InitialSearchTimeInSeconds = timeInSeconds;
                                        break;
                                    case 2:
                                        CalibrationTimeInSeconds = timeInSeconds;
                                        break;
                                    case 3:
                                        PostCalibrationSearchTimeInSeconds = timeInSeconds;
                                        break;
                                    case 4:
                                        GptmdTimeInSeconds = timeInSeconds;
                                        break;
                                    case 5:
                                        PostGptmdSearchTimeInSeconds = timeInSeconds;
                                        break;
                                }
                                break;
                            case Program.CrosslinkSearchLabel:
                                CrosslinkSearchTimeInSeconds = timeInSeconds;
                                break;
                            case Program.ModernSearchLabel:
                                ModernSearchTimeInSeconds = timeInSeconds;
                                break;
                            case Program.NonspecificSearchLabel:
                                NonSpecificSearchTimeInSeconds = timeInSeconds;
                                break;
                            case Program.SemiSpecificSearchLabel:
                                SemiSpecificSearchTimeInSeconds = timeInSeconds;
                                break;
                            case Program.TopDownSearchLabel:
                                switch (taskNumberReading)
                                {
                                    case 1:
                                        TopDownInitialSearchTimeInSeconds = timeInSeconds;
                                        break;
                                    case 2:
                                        TopDownCalibrationTimeInSeconds = timeInSeconds;
                                        break;
                                    case 3: 
                                        TopDownPostCalibrationSearchTimeInSeconds = timeInSeconds;
                                        break;
                                    case 40:  // TODO: After avraging is in command line, change these numbers
                                        TopDownAveragingTimeInSeconds = timeInSeconds;
                                        break;
                                    case 4:
                                        TopDownPostAveragingSearchTimeInSeconds = timeInSeconds;
                                        break;
                                    case 5: 
                                        TopDownGptmdTimeInSeconds = timeInSeconds;
                                        break;
                                    case 6:
                                        TopDownPostGPTMDSearchTimeInSeconds = timeInSeconds;
                                        break;
                                }
                                break;
                            case Program.GlycoSearchLabel:
                                GlycoSearchTimeInSeconds = timeInSeconds;
                                break;
                        }
                    }
                    else if (line.Contains("All target PSM") || (allResultsTxtFile.Key is Program.GlycoSearchLabel && line.Contains("PSMs within 1%")))
                    {
                        int numPsms = int.Parse(line.Split(':').Last().Trim());

                        switch (allResultsTxtFile.Key)
                        {
                            case Program.ClassicSearchLabel:
                                switch (taskNumberReading)
                                {
                                    case 1:
                                        InitialSearchTargetPsms = numPsms;
                                        break;
                                    case 3:
                                        PostCalibrationTargetPsms = numPsms;
                                        break;
                                    case 5:
                                        PostGptmdTargetPsms = numPsms;
                                        break;
                                }
                                break;
                            case Program.ModernSearchLabel:
                                ModernSearchPsms = numPsms;
                                break;
                            case Program.NonspecificSearchLabel:
                                NonSpecificPsms = numPsms;
                                break;
                            case Program.SemiSpecificSearchLabel:
                                SemiSpecificPsms = numPsms;
                                break;
                            case Program.TopDownSearchLabel:
                                switch (taskNumberReading)
                                {
                                    case 1:
                                        TopDownInitialSearchPsms = numPsms;
                                        break;
                                    case 3:
                                        TopDownPostCalibrationSearchPsms = numPsms;
                                        break;
                                    //case 5:
                                    case 4: // TODO: After avraging is in command line, change these numbers
                                        TopDownPostAveragingSearchPsms = numPsms;
                                        break;
                                    case 6:
                                        TopDownPostGPTMDSearchPsms = numPsms;
                                        break;
                                }
                                break;
                            case Program.GlycoSearchLabel:
                                GlycoSearchPsms = numPsms;
                                break;
                        }
                    }
                    else if (line.Contains("All target pept") || line.Contains("All target proteo"))
                    {
                        int numPsms = int.Parse(line.Split(':').Last().Trim());

                        switch (allResultsTxtFile.Key)
                        {
                            case Program.ClassicSearchLabel:
                                switch (taskNumberReading)
                                {
                                    case 1:
                                        InitialSearchTargetPeptides = numPsms;
                                        break;
                                    case 3:
                                        PostCalibrationTargetPeptides = numPsms;
                                        break;
                                    case 5:
                                        PostGptmdTargetPeptides = numPsms;
                                        break;
                                }
                                break;
                            case Program.ModernSearchLabel:
                                ModernSearchPeptides = numPsms;
                                break;
                            case Program.NonspecificSearchLabel:
                                NonSpecificPeptides = numPsms;
                                break;
                            case Program.SemiSpecificSearchLabel:
                                SemiSpecificPeptides = numPsms;
                                break;
                            case Program.TopDownSearchLabel:
                                switch (taskNumberReading)
                                {
                                    case 1:
                                        TopDownInitialSearchProteoforms = numPsms;
                                        break;
                                    case 3:
                                        TopDownPostCalibrationSearchProteoforms = numPsms;
                                        break;
                                    //case 5:
                                    case 4: // TODO: After avraging is in command line, change these numbers
                                        TopDownPostAveragingSearchProteoforms = numPsms;
                                        break;
                                    case 6:
                                        TopDownPostGPTMDSearchProteoforms = numPsms;
                                        break;
                                }
                                break;
                            case Program.GlycoSearchLabel:
                                GlycoSearchPeptides = numPsms;
                                break;
                        }
                    }
                    else if (line.Contains("All target protein groups")
                             && allResultsTxtFile.Key.Equals(Program.ClassicSearchLabel)
                             && taskNumberReading == 1)
                    {
                        int numProteinGroups = int.Parse(line.Split(':').Last().Trim());
                        InitialSearchProteinGroups = numProteinGroups;
                    }

                    // crosslink search results
                    else if (line.Contains("Target inter-crosslinks within 1% FDR:"))
                    {
                        InterlinkCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target intra-crosslinks within 1% FDR:"))
                    {
                        IntralinkCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target single peptides within 1% FDR:"))
                    {
                        CrosslinkSinglePsms =
                            int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target loop-linked peptides within 1% FDR:"))
                    {
                        LoopCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target deadend peptides within 1% FDR:"))
                    {
                        DeadendCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                }
            }
        }

        public static string CommaSeparatedHeader()
        {
            return
                "Date," +
                "Initial Search Time," +
                "Calibration Time," +
                "Post-calibration Search Time," +
                "GPTMD Time," +
                "Post-GPTMD Search Time," +
                "Initial Search PSMs," +
                "Initial Search Peptides," +
                "Post-calibration PSMs," +
                "Post-calibration Peptides," +
                "Post-GPTMD PSMs," +
                "Post-GPTMD Peptides," +
                "Initial Search Protein Groups," +

                "TopDown Initial Search Time," +
                "TopDown Calibration Time," +
                "TopDown Post-calibration Search Time," +
                "TopDown Averaging Time," +
                "TopDown Post-averaging Search Time," +
                "TopDown GPTMD Time," +
                "TopDown Post-GPTMD Search Time," +
                "TopDown Initial PrSMs," +
                "TopDown Initial Proteoforms," +
                "TopDown Post-calibration PrSMs," +
                "TopDown Post-calibration Proteoforms," +
                "TopDown Post-averaging PrSMs," +
                "TopDown Post-averaging Proteoforms," +
                "TopDown Post-GPTMD PrSMs," +
                "TopDown Post-GPTMD Proteoforms," +

                "Semispecific Search Time," +
                "Nonspecific Search Time," +
                "XL Search Time," +
                "Modern Search Time," +
                "Glyco Search Time," +
                "Semispecific PSMs," +
                "Semispecific Peptides," +
                "Nonspecific PSMs," +
                "Nonspecific Peptides," +
                "Interlink CSMs," +
                "Intralink CSMs," +
                "Loop CSMs," +
                "Deadend CSMs," +
                "Crosslink Single PSMs," +
                "Crosslink Single Peptides," +
                "Modern Search PSMs," +
                "Modern Search Peptides," +
                "Glyco Search PSMs," +
                "Glyco Search Peptides,";
        }

        public override string ToString()
        {
            return DateTime + ","
                 + Math.Round(InitialSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(CalibrationTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(PostCalibrationSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(GptmdTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(PostGptmdSearchTimeInSeconds / 60.0, 2) + ","
                 + InitialSearchTargetPsms + ","
                 + InitialSearchTargetPeptides + ","
                 + PostCalibrationTargetPsms + ","
                 + PostCalibrationTargetPeptides + ","
                 + PostGptmdTargetPsms + ","
                 + PostGptmdTargetPeptides + ","
                 + InitialSearchProteinGroups + ","
                 + Math.Round(TopDownInitialSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(TopDownCalibrationTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(TopDownPostCalibrationSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(TopDownAveragingTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(TopDownPostAveragingSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(TopDownGptmdTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(TopDownPostGPTMDSearchTimeInSeconds / 60.0, 2) + ","
                 + TopDownInitialSearchPsms + ","
                 + TopDownInitialSearchProteoforms + ","
                 + TopDownPostCalibrationSearchPsms + ","
                 + TopDownPostCalibrationSearchProteoforms + ","
                 + TopDownPostAveragingSearchPsms + ","
                 + TopDownPostAveragingSearchProteoforms + ","
                 + TopDownPostGPTMDSearchPsms + ","
                 + TopDownPostGPTMDSearchProteoforms + ","
                 + Math.Round(SemiSpecificSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(NonSpecificSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(CrosslinkSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(ModernSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(GlycoSearchTimeInSeconds / 60.0, 2) + ","
                 + SemiSpecificPsms + ","
                 + SemiSpecificPeptides + ","
                 + NonSpecificPsms + ","
                 + NonSpecificPeptides + ","
                 + InterlinkCsms + ","
                 + IntralinkCsms + ","
                 + LoopCsms + ","
                 + DeadendCsms + ","
                 + CrosslinkSinglePsms + ","
                 + CrosslinkSinglePeptides + ","
                 + ModernSearchPsms + ","
                 + ModernSearchPeptides + ","
                 + GlycoSearchPsms + ","
                 + GlycoSearchPeptides;
        }
    }
}
