﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;

namespace vcsparser.bugdatabase.azuredevops
{
    public interface IAzureDevOpsFactory
    {
        IAzureDevOps AzureDevOps(ILogger logger, IAzureDevOpsRequest request, IApiConverter apiConverter, ITimeKeeper timeKeeper);
        IAzureDevOpsRequest AzureDevOpsRequest(IWebRequest webRequest, BugDatabaseArgs args);
        IApiConverter ApiConverter();
    }
}
