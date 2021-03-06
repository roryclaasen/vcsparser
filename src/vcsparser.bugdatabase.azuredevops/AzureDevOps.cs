﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using System.Text.RegularExpressions;
using vcsparser.core.p4;
using vcsparser.core.git;
using System.Threading;

namespace vcsparser.bugdatabase.azuredevops
{
    public class AzureDevOps : IAzureDevOps
    {
        private readonly ILogger logger;
        private readonly IAzureDevOpsRequest request;
        private readonly IApiConverter apiConverter;
        private readonly ITimeKeeper timeKeeper;        

        public AzureDevOps(ILogger logger, IAzureDevOpsRequest request, IApiConverter apiConverter, ITimeKeeper timeKeeper)
        {
            this.logger = logger;
            this.request = request;
            this.apiConverter = apiConverter;
            this.timeKeeper = timeKeeper;            
        }

        private void ProcessWorkItem(Dictionary<DateTime, Dictionary<string, WorkItem>> workItems, JSONQueryItem item)
        {
            var fullWorkItem = request.GetFullWorkItem(item.Url).Result;
            WorkItem workItem = apiConverter.ConvertToWorkItem(fullWorkItem);

            if (string.IsNullOrWhiteSpace(workItem.ChangesetId) || workItem.ChangesetId.ToLower().Equals("<none>"))
                return;

            lock (workItems)
            {
                var date = workItem.ClosedDate.Date;
                if (!workItems.ContainsKey(date))
                    workItems.Add(date, new Dictionary<string, WorkItem>());

                if (!workItems[date].ContainsKey(workItem.ChangesetId))
                {
                    workItems[date].Add(workItem.ChangesetId, workItem);
                }
            }
        }

        private Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessWorkItemList(JSONQueryItem[] items)
        {
            var workItems = new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            int finished = 0;
            timeKeeper.IntervalAction = () => logger.LogToConsole($"Finished processing {finished}/{items.Length} Work Items. {workItems.Count} valid Work Items");
            timeKeeper.Start();

            Parallel.ForEach(items, item =>
            {
                ProcessWorkItem(workItems, item);
                finished++;
            });

            timeKeeper.Cancel();
            return workItems;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> GetWorkItems()
        {
            logger.LogToConsole($"AzureDevOps BugDatabase");
            var json = request.GetWorkItemList().Result;
            logger.LogToConsole($"Found {json.WorkItems.Length} Work Items");
            return ProcessWorkItemList(json.WorkItems);
        }
    }
}
