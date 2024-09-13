using CsvHelper;
using CsvHelper.Configuration;
using Fclp;
using Fclp.Internals.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auditor
{
    public class Program
    {
        public const string ClassicSearchLabel = "Classic";
        public const string ModernSearchLabel = "Modern";
        public const string SemiSpecificSearchLabel = "Semispecific";
        public const string NonspecificSearchLabel = "Nonspecific";
        public const string CrosslinkSearchLabel = "XL";
        public const string TopDownSearchLabel = "TopDown";
        public const string GlycoSearchLabel = "Glyco";

        public static string[] Labels = { ClassicSearchLabel, ModernSearchLabel,
            SemiSpecificSearchLabel, NonspecificSearchLabel, CrosslinkSearchLabel, TopDownSearchLabel, GlycoSearchLabel };

        public static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.InputFolder)
             .As('i', "InputFolder")
             .Required();

            p.Setup(arg => arg.OutputFolder)
             .As('o', "OutputFolder")
             .Required();

            p.Setup(arg => arg.NumberOfDaysToReport)
                .As('n', "NumberOfDaysToReport")
                .SetDefault(5);

            var result = p.Parse(args);

            if (!result.HasErrors)
            {
                // delete old output if it exists
                if (Directory.Exists(p.Object.OutputFolder))
                    Directory.Delete(p.Object.OutputFolder, true);

                MetaMorpheusRunResultsDirectories[] runResults = GetRunDirectories(p.Object.NumberOfDaysToReport, p.Object.InputFolder);

                // create output directory
                if (!Directory.Exists(p.Object.OutputFolder))
                    Directory.CreateDirectory(p.Object.OutputFolder);

                WriteParsingResults(p.Object.OutputFolder, runResults);

                // Only delete unused runs for the daily report
                // CAREFUL: if the number of days to report changes in the Daily runs, this will need to be adjusted
                if (p.Object.NumberOfDaysToReport == 5) 
                    CleanUpOldRunDirectories(p.Object.InputFolder);
            }
        }

        /// <summary>
        /// Gets the run directies from the last n runs
        /// </summary>
        /// <param name="numberOfDays"></param>
        /// <param name="inputFolder"></param>
        /// <returns></returns>
        public static MetaMorpheusRunResultsDirectories[] GetRunDirectories(int numberOfDays, string inputFolder)
        {
            // get results from last n runs
            MetaMorpheusRunResultsDirectories[] runResults = new MetaMorpheusRunResultsDirectories[numberOfDays];

            // get last regular run result (first thing to run)
            List<DirectoryInfo> regularRunDirectories = new DirectoryInfo(inputFolder).GetDirectories()
                .Where(v => v.Name.Contains(ClassicSearchLabel))
                .OrderByDescending(v => v.CreationTime)
                .Take(numberOfDays)
                .OrderBy(v => v.CreationTime).ToList();

            // foreach regular run, find all other runs with labels other than Classic
            for (int i = 0; i < regularRunDirectories.Count; i++)
            {
                DirectoryInfo regularRunDir = regularRunDirectories[i];

                Dictionary<string, DirectoryInfo> directoryInfos = new Dictionary<string, DirectoryInfo> { { ClassicSearchLabel, regularRunDir } };

                var ok = regularRunDir.Name.Split(new char[] { '[' });
                string timestamp = regularRunDir.Name.Split(new char[] { '[' })[1].TrimEnd(new char[] { ']' });

                var otherLabels = Labels.Where(l => l != ClassicSearchLabel).ToList();

                foreach (string label in otherLabels)
                {
                    DirectoryInfo otherRunDirectory = new DirectoryInfo(inputFolder)
                        .GetDirectories()
                        .Where(v => v.Name.Contains(label) && v.Name.Contains(timestamp))
                        .OrderByDescending(v => v.CreationTime)
                        .Take(numberOfDays)
                        .OrderBy(v => v.CreationTime)
                        .FirstOrDefault();

                    directoryInfos.Add(label, otherRunDirectory);
                }

                runResults[i] = new MetaMorpheusRunResultsDirectories(directoryInfos);
            }
            return runResults;
        }

        /// <summary>
        /// Writes the result parsing to a new csv file
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <param name="parsedResults"></param>
        static void WriteParsingResults(string outputFolder, MetaMorpheusRunResultsDirectories[] parsedResults) 
        {
            using (var csv = new CsvWriter(new StreamWriter(
                        File.Create(Path.Combine(outputFolder, "ProcessedResults.csv"))),
                        new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteHeader<MetaMorpheusRunResult>();
                foreach (var item in parsedResults.Where(run => run != null)
                    .Select(specificResult => specificResult.ParsedRunResult))
                {
                    csv.NextRecord();
                    csv.WriteRecord<MetaMorpheusRunResult>(item);
                }
            }
        }

        /// <summary>
        /// Deletes old search results 
        /// Keeps 1 result for every 7 days
        /// Keeps the last 5 days
        /// </summary>
        static void CleanUpOldRunDirectories(string inputFolder)
        {
            // aggregate all task result directories
            List<DirectoryInfo> directoriesToPotentiallyDelete = new DirectoryInfo(inputFolder)
                    .GetDirectories()
                    .OrderByDescending(v => v.CreationTime).ToList();

            DeleteDatabaseIndexFolders(inputFolder);
            DeleteOldMzMLFilesFromCalibrationOrAveraging(inputFolder);

            DateTime dateToStartDeletingAt = new DateTime(2018, 12, 9);

            directoriesToPotentiallyDelete = directoriesToPotentiallyDelete
                .Take(directoriesToPotentiallyDelete.Count)
                .Where(v => v.CreationTime.Date.CompareTo(dateToStartDeletingAt) > 0)
                .ToList();

            var datesToKeep = new List<DateTime>();

            int weeks = 0;
            while (!datesToKeep.Any() || datesToKeep.Last().CompareTo(DateTime.Now) < 0)
            {
                datesToKeep.Add(dateToStartDeletingAt.Date.AddDays(weeks * 7));
                weeks++;
            }

            for (int d = 0; d < 5; d++)
                datesToKeep.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - d).Date);
            

            var directoriesToDelete = directoriesToPotentiallyDelete.Where(v => !datesToKeep.Contains(v.CreationTime.Date)).ToList();

            foreach (DirectoryInfo directory in directoriesToDelete)
                Directory.Delete(directory.FullName, true);
            
        }

        /// <summary>
        /// Deletes all .mzML files in the MetaMorpheus output directories
        /// </summary>
        /// <param name="inputFolder"></param>
        static void DeleteOldMzMLFilesFromCalibrationOrAveraging(string inputFolder)
        {
            foreach (string mzml in Directory.GetFiles(inputFolder, "*", SearchOption.AllDirectories).Where(file => file.EndsWith(".mzML")))
                File.Delete(mzml);
        }

        /// <summary>
        /// Deletes all Index files in the MetaMorpheus input directories
        /// </summary>
        /// <param name="inputFolder"></param>
        static void DeleteDatabaseIndexFolders(string inputFolder)
        {
            var taskDirectory = Path.Combine(Path.GetDirectoryName(inputFolder), "DataAndRunSettings");
            string[] tasksWithDatabaseIndex = new[] { "Classic", "Nonspecific", "Glyco", "XL" };

            foreach (var databaseIndexDirPath in tasksWithDatabaseIndex.Select(p => Path.Combine(taskDirectory, p, "DatabaseIndex")))
                if (Directory.Exists(databaseIndexDirPath))
                    Directory.Delete(databaseIndexDirPath, true);
        }
    }

    public class ApplicationArguments
    {
        #region Public Properties

        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }
        public int NumberOfDaysToReport { get; set; }

        #endregion Public Properties
    }
}
