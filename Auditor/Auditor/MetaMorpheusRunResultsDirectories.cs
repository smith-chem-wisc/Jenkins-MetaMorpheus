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
            DateTime timestamp;
            FileInfo firstSourceFile = labelToFileInfo.Values.FirstOrDefault(v => v != null);
            if (firstSourceFile != null)
            {
                timestamp = firstSourceFile.CreationTime;
            }
            else
            {
                DateTime firstDirectoryTime = directoryInfos.Values
                    .Where(v => v != null)
                    .Select(v => v.CreationTime)
                    .FirstOrDefault();
                timestamp = firstDirectoryTime != default(DateTime)
                    ? firstDirectoryTime
                    : DateTime.UtcNow;
            }

            ParsedRunResult = new MetaMorpheusRunResult(labelToFileInfo, timestamp);
        }
    }
}
