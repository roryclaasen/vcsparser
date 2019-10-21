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
    }
}
