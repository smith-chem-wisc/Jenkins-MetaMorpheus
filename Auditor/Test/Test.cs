using System.IO;
using System.Linq;
using NUnit.Framework;
using Auditor;

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
    }
}
