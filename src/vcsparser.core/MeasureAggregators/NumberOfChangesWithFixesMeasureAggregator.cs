﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfChangesWithFixesMeasureAggregator : IMeasureAggregator<int>
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            return dailyCodeChurn.NumberOfChangesWithFixes + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.NumberOfChangesWithFixes;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.NumberOfChangesWithFixes > 0;
        }
    }
}
