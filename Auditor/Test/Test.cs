using System.IO;
using System.Linq;
using NUnit.Framework;
using Auditor;
using System.Collections.Generic;
using System;
using CsvHelper;
using System.Globalization;

namespace Test
{
    [TestFixture]
    public class Test
    {
        [Test]
        public static void TestNormal()
        {
            var myDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            string outpath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestNormal", "Output");
            
            if (Directory.Exists(outpath))
            {
                var dir = new DirectoryInfo(outpath);
                dir.Delete(true);
            }
            Assert.That(!Directory.Exists(outpath));

            var dirs = Directory.EnumerateDirectories(myDirectory).ToList();
            Assert.AreEqual(5, dirs.Count);
            
            string[] myargs = new string[]
            {
                "--i",
                myDirectory,
                "--o",
                outpath
            };

            Program.Main(myargs);

            var t = Directory.EnumerateFiles(outpath).ToList();
            Assert.AreEqual(6, t.Count);

            var res = File.ReadAllLines(t.Where(p => p.Contains("ProcessedResults.csv")).First());
            Assert.AreEqual(6, res.Length);

            var output = new DirectoryInfo(outpath);
            output.Delete(true);
        }

        [Test]
        public static void TestMissingOne()
        {
            var myDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            string outpath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestMissingOne", "Output");
            string myRealDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestMissingOne");

            if (Directory.Exists(outpath))
            {
                var dir = new DirectoryInfo(outpath);
                dir.Delete(true);
            }
            Assert.That(!Directory.Exists(outpath));

            if (Directory.Exists(myRealDirectory))
            {
                var dir = new DirectoryInfo(myRealDirectory);
                dir.Delete(true);
            }
            Assert.That(!Directory.Exists(myRealDirectory));
            
            Directory.CreateDirectory(myRealDirectory);
            foreach (var directory in new DirectoryInfo(myDirectory).GetDirectories().OrderByDescending(v => v.CreationTime).Take(5).OrderBy(v => v.CreationTime))
            {
                var allFiles = directory.GetFiles().ToList();
                var allTxtFilePath = directory.GetFiles().Where(v => v.FullName.Contains("allResults.txt")).FirstOrDefault();
                var directoryName = directory.Name;
                Directory.CreateDirectory(Path.Combine(myRealDirectory, directoryName));
                File.Copy(allTxtFilePath.FullName, Path.Combine(myRealDirectory, directoryName, allTxtFilePath.Name));
            }

            var dirs = Directory.EnumerateDirectories(myRealDirectory).ToList();
            Assert.AreEqual(5, dirs.Count);
            
            // delete one
            int indexToDelete = 2;
            var toDelete = new DirectoryInfo(dirs[indexToDelete]);
            var txtToDelete = toDelete.GetFiles().Where(v => v.FullName.Contains("allResults.txt")).FirstOrDefault();
            File.Delete(txtToDelete.FullName);
            Assert.AreEqual(5, Directory.EnumerateDirectories(myRealDirectory).Count());

            string[] myargs = new string[]
            {
                "--i",
                myRealDirectory,
                "--o",
                outpath
            };

            Program.Main(myargs);

            var t = Directory.EnumerateFiles(outpath).ToList();
            Assert.AreEqual(5, t.Count);

            var res = File.ReadAllLines(t.Where(p => p.Contains("ProcessedResults.csv")).First());
            Assert.AreEqual(6, res.Length);

            var output = new DirectoryInfo(outpath);
            output.Delete(true);
            
        }

        [Test]
        public static void TestOldDates_BottomUp_Initial()
        {
            var inputFolder = @"D:\Jenkins_Runs\TestFiles\Results";
            var numberOfDays = 2000; 
            string targetDecoyIdentifier = "Decoy/Contaminant/Target";
            string qValueIdentifier = "QValue";
            string pepQValueIdentifier = "PEP_QValue";

            // Select the top n Psm files
            List<DirectoryInfo> regularRunDirectories = new DirectoryInfo(inputFolder).GetDirectories()
                .Where(v => v.Name.Contains(Program.ClassicSearchLabel))
                .OrderByDescending(v => v.CreationTime)
                .Select(p => p.GetDirectories().First(m => m.FullName.Contains("Task1")))
                .Take(numberOfDays)
                .OrderBy(v => v.CreationTime)
                .ToList();

            var psmFiles = regularRunDirectories.Select(p => p.GetFiles("*PSMs.psmtsv", SearchOption.TopDirectoryOnly).FirstOrDefault())
                .Where(p => p != null)
                .ToList();

            List<PsmFileResults> results = new List<PsmFileResults>();
            foreach (var file in psmFiles)
            {
                var foundHeader = false;
                int targetDecoyIndex = -1;
                int qValueIndex = -1;
                int pepQValueIndex = -1;
                int qValueCount = 0;
                int pepQValueCount = 0;

                using (var sw = new StreamReader(File.OpenRead(file.FullName)))
                {    
                    while (sw.Peek() != -1)
                    {
                        var line = sw.ReadLine();
                        var splits = line.Split('\t');
                        if (!foundHeader)
                        {
                            targetDecoyIndex = Array.IndexOf(splits, targetDecoyIdentifier);
                            qValueIndex = Array.IndexOf(splits, qValueIdentifier);
                            pepQValueIndex = Array.IndexOf(splits, pepQValueIdentifier);
                            foundHeader = true;
                            continue;
                        }

                        string tdc = splits[targetDecoyIndex];
                        if (tdc != "T")
                            continue;

                        double qvalue = Double.Parse(splits[qValueIndex]);
                        if (qvalue <= 0.01)
                            qValueCount++;

                        double pep = Double.Parse(splits[pepQValueIndex]);
                        if (pep <= 0.01)
                            pepQValueCount++;
                    }
                }

                var result = new PsmFileResults()
                {
                    PsmOrPeptide = PsmFileResults.PsmString,
                    Date = file.CreationTime.Date.ToString("s"),
                    QValue = qValueCount,
                    PepQValue = pepQValueCount,
                };
                results.Add(result);
                foundHeader = false;
            }

            var peptideFiles = regularRunDirectories.Select(p => p.GetFiles("*Peptides.psmtsv", SearchOption.TopDirectoryOnly).FirstOrDefault())
                .Where(p => p != null)
                .ToList();

            foreach (var file in peptideFiles)
            {
                var foundHeader = false;
                int targetDecoyIndex = -1;
                int qValueIndex = -1;
                int pepQValueIndex = -1;
                int qValueCount = 0;
                int pepQValueCount = 0;

                using (var sw = new StreamReader(File.OpenRead(file.FullName)))
                {
                    while (sw.Peek() != -1)
                    {
                        var line = sw.ReadLine();
                        var splits = line.Split('\t');
                        if (!foundHeader)
                        {
                            targetDecoyIndex = Array.IndexOf(splits, targetDecoyIdentifier);
                            qValueIndex = Array.IndexOf(splits, qValueIdentifier);
                            pepQValueIndex = Array.IndexOf(splits, pepQValueIdentifier);
                            foundHeader = true;
                            continue;
                        }

                        string tdc = splits[targetDecoyIndex];
                        if (tdc != "T")
                            continue;

                        double qvalue = Double.Parse(splits[qValueIndex]);
                        if (qvalue <= 0.01)
                            qValueCount++;

                        double pep = Double.Parse(splits[pepQValueIndex]);
                        if (pep <= 0.01)
                            pepQValueCount++;
                    }
                }

                var result = new PsmFileResults()
                {
                    PsmOrPeptide = PsmFileResults.PeptideString,
                    Date = file.CreationTime.Date.ToString("s"),
                    QValue = qValueCount,
                    PepQValue = pepQValueCount,
                };
                results.Add(result);
                foundHeader = false;
            }

            string outPath = $@"Z:\Users\Nic\PEPTesting\BottomUp_Initial_ResultCountsOverTime_{numberOfDays}days.csv";
            using (var csv = new CsvWriter(new StreamWriter(File.Create(outPath)), CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<PsmFileResults>();
                foreach (var item in results)
                {
                    csv.NextRecord();
                    csv.WriteRecord<PsmFileResults>(item);
                }
            }
        }

        [Test]
        public static void TestOldDates_TopDown_Initial()
        {
            var inputFolder = @"D:\Jenkins_Runs\TestFiles\Results";
            var numberOfDays = 2000;
            string targetDecoyIdentifier = "Decoy/Contaminant/Target";
            string qValueIdentifier = "QValue";
            string pepQValueIdentifier = "PEP_QValue";

            // Select the top n Psm files
            List<DirectoryInfo> regularRunDirectories = new DirectoryInfo(inputFolder).GetDirectories()
                .Where(v => v.Name.Contains(Program.TopDownSearchLabel))
                .OrderByDescending(v => v.CreationTime)
                .Select(p => p.GetDirectories().First(m => m.FullName.Contains("Task1")))
                .Take(numberOfDays)
                .OrderBy(v => v.CreationTime)
                .ToList();

            var psmFiles = regularRunDirectories.Select(p => p.GetFiles("*PSMs.psmtsv", SearchOption.TopDirectoryOnly).FirstOrDefault())
                .Where(p => p != null)
                .ToList();

            List<PsmFileResults> results = new List<PsmFileResults>();
            foreach (var file in psmFiles)
            {
                var foundHeader = false;
                int targetDecoyIndex = -1;
                int qValueIndex = -1;
                int pepQValueIndex = -1;
                int qValueCount = 0;
                int pepQValueCount = 0;

                using (var sw = new StreamReader(File.OpenRead(file.FullName)))
                {
                    while (sw.Peek() != -1)
                    {
                        var line = sw.ReadLine();
                        var splits = line.Split('\t');
                        if (!foundHeader)
                        {
                            targetDecoyIndex = Array.IndexOf(splits, targetDecoyIdentifier);
                            qValueIndex = Array.IndexOf(splits, qValueIdentifier);
                            pepQValueIndex = Array.IndexOf(splits, pepQValueIdentifier);
                            foundHeader = true;
                            continue;
                        }

                        string tdc = splits[targetDecoyIndex];
                        if (tdc != "T")
                            continue;

                        double qvalue = Double.Parse(splits[qValueIndex]);
                        if (qvalue <= 0.01)
                            qValueCount++;

                        double pep = Double.Parse(splits[pepQValueIndex]);
                        if (pep <= 0.01)
                            pepQValueCount++;
                    }
                }

                var result = new PsmFileResults()
                {
                    PsmOrPeptide = PsmFileResults.PsmString,
                    Date = file.CreationTime.Date.ToString("s"),
                    QValue = qValueCount,
                    PepQValue = pepQValueCount,
                };
                results.Add(result);
                foundHeader = false;
            }

            var peptideFiles = regularRunDirectories.Select(p => p.GetFiles("*forms.psmtsv", SearchOption.TopDirectoryOnly).FirstOrDefault())
                .Where(p => p != null)
                .ToList();

            foreach (var file in peptideFiles)
            {
                var foundHeader = false;
                int targetDecoyIndex = -1;
                int qValueIndex = -1;
                int pepQValueIndex = -1;
                int qValueCount = 0;
                int pepQValueCount = 0;

                using (var sw = new StreamReader(File.OpenRead(file.FullName)))
                {
                    while (sw.Peek() != -1)
                    {
                        var line = sw.ReadLine();
                        var splits = line.Split('\t');
                        if (!foundHeader)
                        {
                            targetDecoyIndex = Array.IndexOf(splits, targetDecoyIdentifier);
                            qValueIndex = Array.IndexOf(splits, qValueIdentifier);
                            pepQValueIndex = Array.IndexOf(splits, pepQValueIdentifier);
                            foundHeader = true;
                            continue;
                        }

                        string tdc = splits[targetDecoyIndex];
                        if (tdc != "T")
                            continue;

                        double qvalue = Double.Parse(splits[qValueIndex]);
                        if (qvalue <= 0.01)
                            qValueCount++;

                        double pep = Double.Parse(splits[pepQValueIndex]);
                        if (pep <= 0.01)
                            pepQValueCount++;
                    }
                }

                var result = new PsmFileResults()
                {
                    PsmOrPeptide = PsmFileResults.ProteoformString,
                    Date = file.CreationTime.Date.ToString("s"),
                    QValue = qValueCount,
                    PepQValue = pepQValueCount,
                };
                results.Add(result);
                foundHeader = false;
            }

            string outPath = $@"Z:\Users\Nic\PEPTesting\TopDown_Initial_ResultCounts_{numberOfDays}day.csv";
            using (var csv = new CsvWriter(new StreamWriter(File.Create(outPath)), CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<PsmFileResults>();
                foreach (var item in results)
                {
                    csv.NextRecord();
                    csv.WriteRecord<PsmFileResults>(item);
                }
            }
        }

        [Test]
        public static void ToRemove()
        {
            TestOldDates_BottomUp_Initial();
            TestOldDates_TopDown_Initial();
        }

        public class PsmFileResults
        {
            internal static string PsmString => "Psm";
            internal static string PeptideString => "Peptide";
            internal static string ProteoformString => "Proteoform";

            public string PsmOrPeptide { get; set; }
            public string Date { get; set; }
            public int QValue { get; set; }
            public int PepQValue { get; set; }
        }
    }
}
