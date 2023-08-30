﻿using System;
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
        public int PostCalibrationTargetPsms { get; private set; }
        public int PostGptmdTargetPsms { get; private set; }
        public int SemiSpecificPsms { get; private set; }
        public int NonSpecificPsms { get; private set; }
        public int InterlinkCsms { get; private set; }
        public int IntralinkCsms { get; private set; }
        public int LoopCsms { get; private set; }
        public int DeadendCsms { get; private set; }
        public int CrosslinkSinglePsms { get; private set; }
        public int ModernSearchPsms { get; private set; }

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
                                break;
                        }
                    }
                    else if (line.Contains("All target PSM"))
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
                "Post-calibration PSMs," +
                "Post-GPTMD PSMs," +
                "Initial Search Protein Groups," +

                "Semispecific Search Time," +
                "Nonspecific Search Time," +
                "XL Search Time," +
                "Modern Search Time," +
                "Semispecific PSMs," +
                "Nonspecific PSMs," +
                "Interlink CSMs," +
                "Intralink CSMs," +
                "Loop CSMs," +
                "Deadend CSMs," +
                "Crosslink Single PSMs," +
                "Modern Search PSMs";
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
                 + PostCalibrationTargetPsms + ","
                 + PostGptmdTargetPsms + ","
                 + InitialSearchProteinGroups + ","
                 + Math.Round(SemiSpecificSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(NonSpecificSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(CrosslinkSearchTimeInSeconds / 60.0, 2) + ","
                 + Math.Round(ModernSearchTimeInSeconds / 60.0, 2) + ","
                 + SemiSpecificPsms + ","
                 + NonSpecificPsms + ","
                 + InterlinkCsms + ","
                 + IntralinkCsms + ","
                 + LoopCsms + ","
                 + DeadendCsms + ","
                 + CrosslinkSinglePsms + ","
                 + ModernSearchPsms;
        }
    }
}
