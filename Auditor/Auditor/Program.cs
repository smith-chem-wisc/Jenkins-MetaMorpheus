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

        private static string[] labels = { ClassicSearchLabel, ModernSearchLabel,
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

            var result = p.Parse(args);

            if (!result.HasErrors)
            {
                // delete old output if it exists
                if (Directory.Exists(p.Object.OutputFolder))
                {
                    var dir = new DirectoryInfo(p.Object.OutputFolder);
                    dir.Delete(true);
                }

                // get results from last 5 runs
                int historyCount = 5;
                MetaMorpheusRunResultsDirectories[] runResults = new MetaMorpheusRunResultsDirectories[historyCount];

                // get last regular run result (first thing to run)
                List<DirectoryInfo> regularRunDirectories = new DirectoryInfo(p.Object.InputFolder).GetDirectories()
                    .Where(v => v.Name.Contains(ClassicSearchLabel))
                    .OrderByDescending(v => v.CreationTime)
                    .Take(historyCount)
                    .OrderBy(v => v.CreationTime).ToList();

                for (int i = 0; i < regularRunDirectories.Count; i++)
                {
                    DirectoryInfo regularRunDir = regularRunDirectories[i];

                    Dictionary<string, DirectoryInfo> directoryInfos = new Dictionary<string, DirectoryInfo> { { ClassicSearchLabel, regularRunDir } };

                    var ok = regularRunDir.Name.Split(new char[] { '[' });
                    string timestamp = regularRunDir.Name.Split(new char[] { '[' })[1].TrimEnd(new char[] { ']' });

                    var otherLabels = labels.Where(l => l != ClassicSearchLabel).ToList();

                    foreach (string label in otherLabels)
                    {
                        DirectoryInfo otherRunDirectory = new DirectoryInfo(p.Object.InputFolder)
                            .GetDirectories()
                            .Where(v => v.Name.Contains(label) && v.Name.Contains(timestamp))
                            .OrderByDescending(v => v.CreationTime)
                            .Take(historyCount)
                            .OrderBy(v => v.CreationTime)
                            .FirstOrDefault();

                        directoryInfos.Add(label, otherRunDirectory);
                    }

                    runResults[i] = new MetaMorpheusRunResultsDirectories(directoryInfos);
                }

                // set up output file
                List<string> output = new List<string> { MetaMorpheusRunResult.CommaSeparatedHeader() };

                // add results from each run
                runResults.Where(v => v != null).ForEach(v => output.Add(v.ParsedRunResult.ToString()));

                // write results
                // create output directory
                if (!Directory.Exists(p.Object.OutputFolder))
                {
                    Directory.CreateDirectory(p.Object.OutputFolder);
                }

                File.WriteAllLines(Path.Combine(p.Object.OutputFolder, "ProcessedResults.csv"), output);

                // delete old search results (keeps 1 result for every 7 days, except it keeps all of the last 5 days)
                List<DirectoryInfo> directoriesToPotentiallyDelete = new DirectoryInfo(p.Object.InputFolder)
                    .GetDirectories()
                    .OrderByDescending(v => v.CreationTime).ToList();

                // delete old calibrated and averaged files
                foreach (string mzml in Directory.GetFiles(p.Object.InputFolder).Where(file => file.EndsWith(".mzml")))
                    File.Delete(mzml);

                // delete old database index files
                var indexedDatabaseDirectoryPathClassic = Path.Combine(Path.GetDirectoryName(p.Object.InputFolder.ToString()), "DataAndRunSettings", "Classic", "DatabaseIndex");
                var indexedDatabaseDirectoryPathNonSpecific = Path.Combine(Path.GetDirectoryName(p.Object.InputFolder.ToString()), "DataAndRunSettings", "Nonspecific", "DatabaseIndex");
                directoriesToPotentiallyDelete.AddRange( new DirectoryInfo(indexedDatabaseDirectoryPathClassic)
                    .GetDirectories()
                    .OrderByDescending(v => v.CreationTime).ToList());
                directoriesToPotentiallyDelete.AddRange(new DirectoryInfo(indexedDatabaseDirectoryPathNonSpecific)
                    .GetDirectories()
                    .OrderByDescending(v => v.CreationTime).ToList());

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

                for (int d = 0; d < historyCount; d++)
                {
                    datesToKeep.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - d).Date);
                }

                var directoriesToDelete = directoriesToPotentiallyDelete.Where(v => !datesToKeep.Contains(v.CreationTime.Date)).ToList();

                foreach (DirectoryInfo directory in directoriesToDelete)
                {
                    Directory.Delete(directory.FullName, true);
                }
            }
        }
    }

    public class ApplicationArguments
    {
        #region Public Properties

        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }

        #endregion Public Properties
    }
}
