using System;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using System.Globalization;

namespace vcsparser.bugdatabase.azuredevops
{
    public class BugDatabaseProvider : IBugDatabaseProvider
    {
        public string Key => "AzureDevOps";

        private readonly IBugDatabaseFactory bugDbFactory;
        private readonly IAzureDevOpsFactory factory;
        private readonly IWebRequest webRequest;
        private readonly ILogger logger;

        private IAzureDevOps azureDevOps;

        public BugDatabaseProvider(IBugDatabaseFactory bugDbFactory, IAzureDevOpsFactory factory, IWebRequest webRequest, ILogger logger)
        {
            this.bugDbFactory = bugDbFactory;
            this.factory = factory;
            this.webRequest = webRequest;
            this.logger = logger;
        }

        public int ProcessArgs(IEnumerable<string> args)
        {
            var code = Parser.Default.ParseArguments<BugDatabaseArgs>(args).MapResult(
                (BugDatabaseArgs a) => SetUp(a),
                err => 1);
            return code;
        }

        private int SetUp(BugDatabaseArgs args)
        {
            const string dateFormat = @"yyyy-MM-dd";
            if (!DateTime.TryParseExact(args.From, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _) ||
                !DateTime.TryParseExact(args.To, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
            {
                logger.LogToConsole($"Date inputs must match '{dateFormat}'");
                return 1;
            }

            IAzureDevOpsRequest request = factory.AzureDevOpsRequest(webRequest, args);
            IApiConverter converter = factory.ApiConverter();
            ITimeKeeper timeKeeper = bugDbFactory.TimeKeeper(TimeSpan.FromSeconds(30));
            azureDevOps = factory.AzureDevOps(logger, request, converter, timeKeeper);
            return 0;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> Process()
        {
            if (azureDevOps == null) return new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            return azureDevOps.GetWorkItems();
        }
    }
}
