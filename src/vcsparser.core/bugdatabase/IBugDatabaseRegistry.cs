using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseRegistry
    {
        void AddBugDatabase(IBugDatabaseProvider databaseProvider);
        IBugDatabaseProvider Retrive(string path, IEnumerable<string> args, IWebRequest webRequest);
    }
}
