﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesChangedBugDatabaseMeasureAggregator : IMeasureAggregator<int>
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            return dailyCodeChurn.BugDatabase.TotalLinesChanged + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.BugDatabase.TotalLinesChanged;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            if (dailyCodeChurn.BugDatabase == null) return false;
            return dailyCodeChurn.BugDatabase.TotalLinesChanged > 0;
        }
    }
}
