using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseRegistry : IBugDatabaseRegistry
    {
        private readonly Dictionary<string, IBugDatabaseProvider> databases;

        public BugDatabaseRegistry()
        {
            this.databases = new Dictionary<string, IBugDatabaseProvider>();
        }

        public void AddBugDatabase(IBugDatabaseProvider databaseProvider)
        {
            this.databases.Add(databaseProvider.Key.ToLower(), databaseProvider);
        }

        public IBugDatabaseProvider Retrive(string key, IEnumerable<string> args, IWebRequest webRequest)
        {
            if (!databases.ContainsKey(key.ToLower()))
                throw new Exception($"No Bug Database registered with key '{key}'");

            IBugDatabaseProvider databaseProvider = this.databases[key];
            if (databaseProvider.ProcessArgs(args) != 0)
                throw new Exception("Unable to parse Bug Databse arguments. Check requirements");

            return databaseProvider;
        }
    }
}
