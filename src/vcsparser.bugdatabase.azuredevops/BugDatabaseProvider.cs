﻿using System;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.bugdatabase.azuredevops;
using System.Text.RegularExpressions;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.p4;
using vcsparser.core.git;
using System.Globalization;

namespace vcsparser.bugdatabase.azuredevops
{
    public class BugDatabaseProvider : IBugDatabaseProvider
    {
        public string Key => "AzureDevOps";
        public ILogger Logger { get; set; }
        public IWebRequest WebRequest { get; set; }
        public IAzureDevOpsFactory AzureDevOpsFactory { get; set; } = new AzureDevOpsFactory();

        private IAzureDevOps azureDevOps;

        public int ProcessArgs(IEnumerable<string> args)
        {
            var code = Parser.Default.ParseArguments<DllArgs>(args).MapResult(
                (DllArgs a) => SetUp(a),
                err => 1);
            return code;
        }

        private int SetUp(DllArgs args)
        {
            const string dateFormat = @"yyyy-MM-dd";
            if (!DateTime.TryParseExact(args.From, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _) ||
                !DateTime.TryParseExact(args.To, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
            {
                Logger.LogToConsole($"Date inputs must match '{dateFormat}'");
                return 1;
            }

            IAzureDevOpsRequest request = new AzureDevOpsRequest(WebRequest, args);
            IApiConverter converter = new ApiConverter();
            ITimeKeeper timeKeeper = new TimeKeeper(TimeSpan.FromSeconds(30));
            azureDevOps = AzureDevOpsFactory.GetAzureDevOps(Logger, request, converter, timeKeeper);
            return 0;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> Process()
        {
            if (azureDevOps == null) return new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            return azureDevOps.GetWorkItems();
        }
    }
}
