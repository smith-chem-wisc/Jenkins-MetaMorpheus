using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auditor
{
    public class MetaMorpheusRunResultsDirectories
    {
        private Dictionary<string, DirectoryInfo> directoryInfos;
        private Dictionary<string, FileInfo> labelToFileInfo;
        public MetaMorpheusRunResult ParsedRunResult;

        public MetaMorpheusRunResultsDirectories(Dictionary<string, DirectoryInfo> directoryInfos)
        {
            this.directoryInfos = directoryInfos;

            GetAllResultsTextFileFromDirectories();
            ParseAllResultsTextFiles();
        }

        private void GetAllResultsTextFileFromDirectories()
        {
            labelToFileInfo = new Dictionary<string, FileInfo>();
            foreach (var directory in directoryInfos)
            {
                string label = directory.Key;

                if (directory.Value != null)
                {
                    var resultsFile = directory.Value.GetFiles()
                            .FirstOrDefault(v => v.FullName.Contains("allResults.txt"));

                    labelToFileInfo.Add(label, resultsFile);
                }
                else
                {
                    labelToFileInfo.Add(label, null);
                }
            }
        }

        private void ParseAllResultsTextFiles()
        {
            DateTime timestamp = new DateTime();
            var nonNullFiles = labelToFileInfo.Where(p => p.Value != null).ToList();

            if (!nonNullFiles.Any())
            {
                timestamp = directoryInfos.Select(p => p.Value.CreationTime).First();
            }
            else
            {
                timestamp = labelToFileInfo.Select(p => p.Value.CreationTime).First();
            }

            ParsedRunResult = new MetaMorpheusRunResult(labelToFileInfo, timestamp);
        }
    }
}
