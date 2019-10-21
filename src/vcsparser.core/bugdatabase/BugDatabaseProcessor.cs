using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using vcsparser.core.git;
using vcsparser.core.p4;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseProcessor : IBugDatabaseProcessor
    {
        private readonly IBugDatabaseRegistry registry;
        private readonly IWebRequest webRequest;

        private readonly IFileSystem fileSystem;
        private readonly IJsonListParser<WorkItem> workItemParser;
        private readonly ILogger logger;

        public BugDatabaseProcessor(IBugDatabaseRegistry registry, IWebRequest webRequest, IFileSystem fileSystem, IJsonListParser<WorkItem> workItemParser, ILogger logger)
        {
            this.registry = registry;
            this.webRequest = webRequest;
            this.fileSystem = fileSystem;
            this.workItemParser = workItemParser;
            this.logger = logger;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string databaseKey, IEnumerable<string> databaseArgs)
        {
            IBugDatabaseProvider databaseProvider = registry.Retrive(databaseKey, databaseArgs, webRequest);
            return databaseProvider.Process();
        }

        public void ProcessCache(string cacheOutput, IChangesetProcessor changesetProcessor)
        {
            if (string.IsNullOrEmpty(cacheOutput))
                return;

            var files = fileSystem.GetFiles(fileSystem.GetParentFullName(cacheOutput), "*.json");

            foreach (var file in files)
            {
                logger.LogToConsole($"Processing {file.FileName}");
                var workItemList = workItemParser.ParseFile(file.FileName);
                foreach(var workitem in workItemList)
                {
                    if (!changesetProcessor.WorkItemCache.ContainsKey(workitem.ChangesetId))
                        changesetProcessor.WorkItemCache.Add(workitem.ChangesetId, new List<WorkItem>());

                    changesetProcessor.WorkItemCache[workitem.ChangesetId].Add(workitem);
                }
            }
        }
    }
}
