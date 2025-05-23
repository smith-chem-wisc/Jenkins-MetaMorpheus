﻿using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auditor
{
    public class MetaMorpheusRunResult
    {
        // path to allResults.txt
        [Ignore] public Dictionary<string, FileInfo> AllResultsTexts { get; private set; }

        // time of run folder creation
        [Name("Date")] public DateTime DateTime { get; private set; }

        // Bottom Up results
        [Name("Initial Search PSMs")] public int? InitialSearchTargetPsms { get; private set; }
        [Name("Initial Search Peptides")] public int? InitialSearchTargetPeptides { get; private set; }
        [Name("Post-calibration PSMs")] public int? PostCalibrationTargetPsms { get; private set; }
        [Name("Post-calibration Peptides")] public int? PostCalibrationTargetPeptides { get; private set; }
        [Name("Post-GPTMD PSMs")] public int? PostGptmdTargetPsms { get; private set; }
        [Name("Post-GPTMD Peptides")] public int? PostGptmdTargetPeptides { get; private set; }
        [Name("Initial Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? InitialSearchTimeInSeconds { get; private set; }
        [Name("Calibration Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? CalibrationTimeInSeconds { get; private set; }
        [Name("Post-calibration Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? PostCalibrationSearchTimeInSeconds { get; private set; }
        [Name("GPTMD Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? GptmdTimeInSeconds { get; private set; }
        [Name("Post-GPTMD Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? PostGptmdSearchTimeInSeconds { get; private set; }

        // Cross Link
        [Name("Interlink CSMs")] public int? InterlinkCsms { get; private set; }
        [Name("Intralink CSMs")] public int? IntralinkCsms { get; private set; }
        [Name("Loop CSMs")] public int? LoopCsms { get; private set; }
        [Name("Deadend CSMs")] public int? DeadendCsms { get; private set; }
        [Name("Crosslink Single PSMs")] public int? CrosslinkSinglePsms { get; private set; }
        [Name("Crosslink Single Peptides")] public int? CrosslinkSinglePeptides { get; private set; }
        [Name("XL Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? CrosslinkSearchTimeInSeconds { get; private set; }

        // O-Glyco Search
        [Name("Glyco Search PSMs")] public int? GlycoSearchPsms { get; private set; }
        [Name("Glyco Search Peptides")] public int? GlycoSearchPeptides { get; private set; }
        [Name("Glyco Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? GlycoSearchTimeInSeconds { get; private set; }

        // Semi-Specific Search
        [Name("Semispecific PSMs")] public int? SemiSpecificPsms { get; private set; }
        [Name("Semispecific Peptides")] public int? SemiSpecificPeptides { get; private set; }
        [Name("Semispecific Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? SemiSpecificSearchTimeInSeconds { get; private set; }

        // Non-Specific Search
        [Name("Nonspecific PSMs")] public int? NonSpecificPsms { get; private set; }
        [Name("Nonspecific Peptides")] public int? NonSpecificPeptides { get; private set; }
        [Name("Nonspecific Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? NonSpecificSearchTimeInSeconds { get; private set; }

        // Modern Search
        [Name("Modern Search PSMs")] public int? ModernSearchPsms { get; private set; }
        [Name("Modern Search Peptides")] public int? ModernSearchPeptides { get; private set; }
        [Name("Modern Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? ModernSearchTimeInSeconds { get; private set; }

        // Top Down
        [Name("TopDown Initial PrSMs")] public double? TopDownInitialSearchPsms { get; private set; }
        [Name("TopDown Initial Proteoforms")] public double? TopDownInitialSearchProteoforms { get; private set; }
        [Name("TopDown Post-calibration PrSMs")] public double? TopDownPostCalibrationSearchPsms { get; private set; }
        [Name("TopDown Post-calibration Proteoforms")] public double? TopDownPostCalibrationSearchProteoforms { get; private set; }
        [Name("TopDown Post-averaging PrSMs")] public double? TopDownPostAveragingSearchPsms { get; private set; }
        [Name("TopDown Post-averaging Proteoforms")] public double? TopDownPostAveragingSearchProteoforms { get; private set; }
        [Name("TopDown Post-GPTMD PrSMs")] public double? TopDownPostGPTMDSearchPsms { get; private set; }
        [Name("TopDown Post-GPTMD Proteoforms")] public double? TopDownPostGPTMDSearchProteoforms { get; private set; }
        [Name("TopDown Initial Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownInitialSearchTimeInSeconds { get; private set; }
        [Name("TopDown Calibration Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownCalibrationTimeInSeconds { get; private set; }
        [Name("TopDown Post-calibration Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownPostCalibrationSearchTimeInSeconds { get; private set; }
        [Name("TopDown Averaging Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownAveragingTimeInSeconds { get; private set; }
        [Name("TopDown Post-averaging Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownPostAveragingSearchTimeInSeconds { get; private set; }
        [Name("TopDown GPTMD Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownGptmdTimeInSeconds { get; private set; }
        [Name("TopDown Post-GPTMD Search Time")][TypeConverter(typeof(SecondsToMinutesConverter))] public double? TopDownPostGPTMDSearchTimeInSeconds { get; private set; }

        // protein groups for initial search (task 1)
        public int? InitialSearchProteinGroups { get; private set; }

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
        /// Parses results from the AllResults.text files
        /// Uses the TimeToRunTask to incrememt in multi-task runs such as the bottom up or top down
        /// 
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
                var taskCount = lines.Count(p => p.Contains("Time to run task:"));
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
                                // differential task count is due to averaging command line not function for several jenkins runs
                                if (taskCount == 6)
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
                                else if (taskCount == 7)
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
                                        case 4:  // TODO: After avraging is in command line, change these numbers
                                            TopDownAveragingTimeInSeconds = timeInSeconds;
                                            break;
                                        case 5:
                                            TopDownPostAveragingSearchTimeInSeconds = timeInSeconds;
                                            break;
                                        case 6: 
                                            TopDownGptmdTimeInSeconds = timeInSeconds;
                                            break;
                                        case 7:
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
                                        if (InitialSearchTargetPsms == null)
                                            InitialSearchTargetPsms = numPsms; 
                                        break;
                                    case 3:
                                        if (PostCalibrationTargetPsms == null)
                                            PostCalibrationTargetPsms = numPsms;
                                        break;
                                    case 5:
                                        if (PostGptmdTargetPsms == null)
                                            PostGptmdTargetPsms = numPsms;
                                        break;
                                }
                                break;
                            case Program.ModernSearchLabel:
                                if (ModernSearchPsms == null)
                                    ModernSearchPsms = numPsms;
                                break;
                            case Program.NonspecificSearchLabel:
                                if (NonSpecificPsms == null)
                                    NonSpecificPsms = numPsms;
                                break;
                            case Program.SemiSpecificSearchLabel:
                                if (SemiSpecificPsms == null)
                                    SemiSpecificPsms = numPsms;
                                break;
                            case Program.TopDownSearchLabel:
                                // differential task count is due to averaging command line not function for several jenkins runs
                                if (taskCount == 6)
                                    switch (taskNumberReading)
                                    {
                                        case 1:
                                            if (TopDownInitialSearchPsms == null)
                                                TopDownInitialSearchPsms = numPsms;
                                            break;
                                        case 3:
                                            if (TopDownPostCalibrationSearchPsms == null)
                                                TopDownPostCalibrationSearchPsms = numPsms;
                                            break;
                                        case 4:
                                            if (TopDownPostAveragingSearchPsms == null)
                                                TopDownPostAveragingSearchPsms = numPsms;
                                            break;
                                        case 6:
                                            if (TopDownPostGPTMDSearchPsms == null)
                                                TopDownPostGPTMDSearchPsms = numPsms;
                                            break;
                                    }
                                else if (taskCount == 7)
                                    switch (taskNumberReading)
                                    {
                                        case 1:
                                            if (TopDownInitialSearchPsms == null)
                                                TopDownInitialSearchPsms = numPsms;
                                            break;
                                        case 3:
                                            if (TopDownPostCalibrationSearchPsms == null)
                                                TopDownPostCalibrationSearchPsms = numPsms;
                                            break;
                                        case 5:
                                            if (TopDownPostAveragingSearchPsms == null)
                                                TopDownPostAveragingSearchPsms = numPsms;
                                            break;
                                        case 7:
                                            if (TopDownPostGPTMDSearchPsms == null)
                                                TopDownPostGPTMDSearchPsms = numPsms;
                                            break;
                                    }
                                break;
                            case Program.GlycoSearchLabel:
                                if (GlycoSearchPsms == null)
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
                                        if (InitialSearchTargetPeptides == null)
                                            InitialSearchTargetPeptides = numPsms;
                                        break;
                                    case 3:
                                        if (PostCalibrationTargetPeptides == null)
                                            PostCalibrationTargetPeptides = numPsms;
                                        break;
                                    case 5:
                                        if (PostGptmdTargetPeptides == null)
                                            PostGptmdTargetPeptides = numPsms;
                                        break;
                                }
                                break;
                            case Program.ModernSearchLabel:
                                if (ModernSearchPeptides == null)
                                    ModernSearchPeptides = numPsms;
                                break;
                            case Program.NonspecificSearchLabel:
                                if (NonSpecificPeptides == null)
                                    NonSpecificPeptides = numPsms;
                                break;
                            case Program.SemiSpecificSearchLabel:
                                if (SemiSpecificPeptides == null)
                                    SemiSpecificPeptides = numPsms;
                                break;
                            case Program.TopDownSearchLabel:
                                // differential task count is due to averaging command line not function for several jenkins runs
                                if (taskCount == 6) 
                                    switch (taskNumberReading)
                                    {
                                        case 1:
                                            if (TopDownInitialSearchProteoforms == null)
                                                TopDownInitialSearchProteoforms = numPsms;
                                            break;
                                        case 3:
                                            if (TopDownPostCalibrationSearchProteoforms == null)
                                                TopDownPostCalibrationSearchProteoforms = numPsms;
                                            break;
                                        case 4:
                                            if (TopDownPostAveragingSearchProteoforms == null)
                                                TopDownPostAveragingSearchProteoforms = numPsms;
                                            break;
                                        case 6:
                                            if (TopDownPostGPTMDSearchProteoforms == null)
                                                TopDownPostGPTMDSearchProteoforms = numPsms;
                                            break;
                                    }
                                else if (taskCount == 7)
                                    switch (taskNumberReading)
                                    {
                                        case 1:
                                            if (TopDownInitialSearchProteoforms == null)
                                                TopDownInitialSearchProteoforms = numPsms;
                                            break;
                                        case 3:
                                            if (TopDownPostCalibrationSearchProteoforms == null)
                                                TopDownPostCalibrationSearchProteoforms = numPsms;
                                            break;
                                        case 5:
                                            if (TopDownPostAveragingSearchProteoforms == null)
                                                TopDownPostAveragingSearchProteoforms = numPsms;
                                            break;
                                        case 7:
                                            if (TopDownPostGPTMDSearchProteoforms == null)
                                                TopDownPostGPTMDSearchProteoforms = numPsms;
                                            break;
                                    }
                                break;
                            case Program.GlycoSearchLabel:
                                if (GlycoSearchPeptides == null)
                                    GlycoSearchPeptides = numPsms;
                                break;
                        }
                    }
                    else if (line.Contains("All target protein groups")
                             && allResultsTxtFile.Key.Equals(Program.ClassicSearchLabel)
                             && taskNumberReading == 1)
                    {
                        if (InitialSearchProteinGroups == null)
                        {
                            int numProteinGroups = int.Parse(line.Split(':').Last().Trim());
                            InitialSearchProteinGroups = numProteinGroups;
                        }      
                    }

                    // crosslink search results
                    else if (line.Contains("Target inter-crosslinks within 1% FDR:"))
                    {
                        if (InterlinkCsms == null)
                            InterlinkCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target intra-crosslinks within 1% FDR:"))
                    {
                        if (IntralinkCsms == null)
                            IntralinkCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target single peptides within 1% FDR:"))
                    {
                        if (CrosslinkSinglePsms == null)
                            CrosslinkSinglePsms =
                            int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target loop-linked peptides within 1% FDR:"))
                    {
                        if (LoopCsms == null)
                            LoopCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                    else if (line.Contains("Target deadend peptides within 1% FDR:"))
                    {
                        if (DeadendCsms == null)
                            DeadendCsms = int.Parse(line.Split(':').Last().Trim());
                    }
                }
            }
        }
    }

    internal class SecondsToMinutesConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is null) // When the task did not exist in jenkins. 
                return "0";
            if (value is double || value is double?)
            {
                var minutes = (double)value / 60.0;
                return Math.Round(minutes, 2, MidpointRounding.AwayFromZero).ToString();
            }
            throw new ArgumentException("time must be double or double?");
        }
    }
}
