﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseProcessor
    {
        /// <summary>
        /// Process the provided bug database and return the Work Items found
        /// </summary>
        /// <param name="dllPath">The key of the bug database to use</param>
        /// <param name="dllArgs">List of arguments to pass to the bugdatabase</param>
        /// <returns>Dictionary indexed by DateTime, indexed by changeset id</returns>
        Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string databaseKey, IEnumerable<string> databaseArgs);
        void ProcessCache(string cacheDirectory, IChangesetProcessor changesetProcessor);
    }
}
