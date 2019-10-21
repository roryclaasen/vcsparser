using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseFactory : IBugDatabaseFactory
    {
        public IHttpClientWrapper GetHttpClientWrapper()
        {
            return new HttpClientWrapper();
        }

        public ITimeKeeper TimeKeeper(TimeSpan timeSpan)
        {
            return new TimeKeeper(timeSpan);
        }

        public ITimeKeeper TimeKeeper(TimeSpan timeSpan, Action action)
        {
            return new TimeKeeper(timeSpan, action);
        }
    }
}
