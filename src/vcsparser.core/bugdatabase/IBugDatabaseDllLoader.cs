﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseDllLoader
    {
        void AddBugDatabase(IBugDatabaseProvider databaseProvider);
        IBugDatabaseProvider Load(string path, IEnumerable<string> args, IWebRequest webRequest);
    }
}
