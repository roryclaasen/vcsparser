﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.p4
{
    public interface IDescribeParser
    {
        PerforceChangeset Parse(List<string> lines);
    }
}
