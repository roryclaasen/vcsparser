using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseDllLoader : IBugDatabaseDllLoader
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, IBugDatabaseProvider> databases;

        public BugDatabaseDllLoader(ILogger logger)
        {
            this.logger = logger;
            this.databases = new Dictionary<string, IBugDatabaseProvider>();
        }

        public void AddBugDatabase(IBugDatabaseProvider databaseProvider)
        {
            this.databases.Add(databaseProvider.Key, databaseProvider);
        }

        public IBugDatabaseProvider Load(string key, IEnumerable<string> args, IWebRequest webRequest)
        {
            if (!databases.ContainsKey(key))
                throw new Exception($"No bug database registered with key '{key}'");

            IBugDatabaseProvider databaseProvider = this.databases[key];
            databaseProvider.Logger = logger;
            databaseProvider.WebRequest = webRequest;
            if (databaseProvider.ProcessArgs(args) != 0)
                throw new Exception("Unable to parse Dll arguments. Check requirements");

            return databaseProvider;
        }
    }
}
