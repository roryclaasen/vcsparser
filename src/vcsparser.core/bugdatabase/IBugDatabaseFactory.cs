using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseFactory
    {
        IHttpClientWrapper GetHttpClientWrapper();
        ITimeKeeper TimeKeeper(TimeSpan timeSpan);
        ITimeKeeper TimeKeeper(TimeSpan timeSpan, Action action);
    }
}
