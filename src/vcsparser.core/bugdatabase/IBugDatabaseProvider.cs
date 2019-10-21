using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseProvider
    {
        string Key { get; }

        int ProcessArgs(IEnumerable<string> args);
        Dictionary<DateTime, Dictionary<string, WorkItem>> Process();
    }
}
