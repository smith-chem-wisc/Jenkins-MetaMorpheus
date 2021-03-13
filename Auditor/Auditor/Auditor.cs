using Fclp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditor
{
    class Auditor
    {
        static void Main(string[] args)
        {
            // get input/output directories
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.spectraFilePath)
             .As('i', "spectraFilePath")
             .Required();

            p.Setup(arg => arg.moveTo)
             .As('o', "moveTo")
             .Required();

            var result = p.Parse(args);

            string spectraFilePath = p.Object.spectraFilePath;
            string moveTo = p.Object.moveTo;

            // get last 5 results files from input directory and copy them to output directory
            if (Directory.Exists(spectraFilePath))
            {
                List<List<string>> allStrings = new List<List<string>>();
                // initial search results
                List<string> initialSearchPsmResults = new List<string>();
                initialSearchPsmResults.Add(">Initial search PSM results:");
                allStrings.Add(initialSearchPsmResults);

                List<string> initialSearchTime = new List<string>();
                initialSearchTime.Add("Initial search time:");
                allStrings.Add(initialSearchTime);
                
                // calibration results
                List<string> calibrationTimeToRun = new List<string>();
                calibrationTimeToRun.Add(">Calibration run time:");
                allStrings.Add(calibrationTimeToRun);

                // post-calibration search results
                List<string> postCalibrationSearchPsmResults = new List<string>();
                postCalibrationSearchPsmResults.Add(">Post-calibration search PSM results:");
                allStrings.Add(postCalibrationSearchPsmResults);

                List<string> postCalibrationSearchTime = new List<string>();
                postCalibrationSearchTime.Add("Post-calibration search time:");
                allStrings.Add(postCalibrationSearchTime);
                
                // gptmd results
                List<string> gptmdModsAdded = new List<string>();
                gptmdModsAdded.Add(">GPTMD modifications added:");
                allStrings.Add(gptmdModsAdded);

                List<string> gptmdTimeToRun = new List<string>();
                gptmdTimeToRun.Add("GPTMD run time:");
                allStrings.Add(gptmdTimeToRun);

                // post-gptmd search results
                List<string> postGptmdSearchPsmResults = new List<string>();
                postGptmdSearchPsmResults.Add(">Post-GPTMD search PSM results:");
                allStrings.Add(postGptmdSearchPsmResults);

                List<string> postGptmdSearchTime = new List<string>();
                postGptmdSearchTime.Add("Post-GPTMD search time:");
                allStrings.Add(postGptmdSearchTime);
                
                // parse the results files
                var resultsFolders = Directory.EnumerateDirectories(spectraFilePath).OrderBy(v => Directory.GetCreationTime(v)).ToList();
                var fiveMostRecentlyMadeDirectories = resultsFolders.Take(5).ToList();
                string[] lastFiveResultsPaths = new string[5];

                for (int i = 0; i < fiveMostRecentlyMadeDirectories.Count; i++)
                {
                    var files = Directory.GetFiles(fiveMostRecentlyMadeDirectories[i]);
                    var resultTxt = files.Where(v => Path.GetFileName(v).Equals("allResults.txt"));

                    if (resultTxt.Any())
                        lastFiveResultsPaths[i] = resultTxt.First();
                    else
                        lastFiveResultsPaths[i] = null;
                }

                Directory.CreateDirectory(moveTo);

                for (int i = 0; i < lastFiveResultsPaths.Length; i++)
                {
                    if (lastFiveResultsPaths[i] != null)
                    {
                        File.Copy(lastFiveResultsPaths[i], Path.Combine(moveTo, "allResults" + (i + 1) + ".txt"), true);
                        var time = File.GetCreationTime(lastFiveResultsPaths[i]);

                        var lines = File.ReadAllLines(lastFiveResultsPaths[i]);
                        int taskNumber = 0;
                        foreach (var line in lines)
                        {
                            if (line.Contains("Time to run task: "))
                            {
                                taskNumber++;
                            }
                            
                            // initial search
                            if (taskNumber == 1)
                            {
                                if (line.Contains("All target PSMS within 1% FDR:"))
                                    initialSearchPsmResults.Add("\t" + time + "\t---\t" + line);
                                else if (line.Contains("Time to run task:"))
                                    initialSearchTime.Add("\t" + time + "\t---\t" + line);
                            }

                            // calibration
                            if (taskNumber == 2)
                            {
                                if (line.Contains("Time to run task:"))
                                    calibrationTimeToRun.Add("\t" + time + "\t---\t" + line);
                            }

                            // post-calibration search
                            if (taskNumber == 3)
                            {
                                if (line.Contains("All target PSMS within 1% FDR:"))
                                    postCalibrationSearchPsmResults.Add("\t" + time + "\t---\t" + line);
                                else if (line.Contains("Time to run task:"))
                                    postCalibrationSearchTime.Add("\t" + time + "\t---\t" + line);
                            }

                            // gptmd
                            if (taskNumber == 4)
                            {
                                if (line.Contains("Modifications added:"))
                                    gptmdModsAdded.Add("\t" + time + "\t---\t" + line);
                                else if (line.Contains("Time to run task:"))
                                    gptmdTimeToRun.Add("\t" + time + "\t---\t" + line);
                            }

                            // post-gptmd search
                            if (taskNumber == 5)
                            {
                                if (line.Contains("All target PSMS within 1% FDR:"))
                                    postGptmdSearchPsmResults.Add("\t" + time + "\t---\t" + line);
                                else if (line.Contains("Time to run task:"))
                                    postGptmdSearchTime.Add("\t" + time + "\t---\t" + line);
                            }
                        }
                    }
                    else
                    {
                        foreach (var stringList in allStrings)
                            stringList.Add("\t" + "--");
                    }
                }

                List<string> output = new List<string>();
                foreach (var listOfStrings in allStrings)
                {
                    if(listOfStrings.First().Contains(">"))
                    {
                        output.Add("---------------------------------------------------------------------------------\n");
                    }

                    foreach (var myString in listOfStrings)
                    {
                        output.Add(myString);
                    }
                    output.Add("\n");
                }

                File.WriteAllLines(Path.Combine(moveTo, "ProcessedResults.txt"), output);
            }
        }

        internal class ApplicationArguments
        {
            #region Public Properties

            public string spectraFilePath { get; private set; }
            public string moveTo { get; private set; }

            #endregion Public Properties
        }
    }
}
