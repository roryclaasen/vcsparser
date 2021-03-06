﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.core
{
    public class MeasureConverter<T> : IMeasureConverter
    {
        private readonly DateTime startDate;
        private readonly DateTime endDate;
        private readonly Metric metric;
        private string filePrefixToRemove;
        private readonly bool projectMeasure;

        private bool processedMetric = false;

        public Metric Metric {
            get { return metric; }
        }

        public DateTime StartDate {
            get { return startDate; }
        }

        public DateTime EndDate {
            get { return endDate; }
        }

        protected IMeasureAggregator<T> measureAggregator;

        public IMeasureAggregator<T> MeasureAggregator {
            get { return this.measureAggregator; }
        }

        public MeasureConverter(DateTime startDate, DateTime endDate, Metric metric, IMeasureAggregator<T> measureAggregator, string filePrefixToRemove)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.metric = metric;
            this.measureAggregator = measureAggregator;
            this.filePrefixToRemove = filePrefixToRemove;
            this.projectMeasure = measureAggregator is IMeasureAggregatorProject<T>;
        }

        public void ProcessFileMeasure(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson)
        {
            if (!ValidDailyCodeChurn(dailyCodeChurn))
                return;

            ProcessMetric(sonarMeasuresJson);

            var fileName = ProcessFileName(dailyCodeChurn.FileName, filePrefixToRemove);

            var existingMeasureFile = sonarMeasuresJson.FindFileMeasure(metric.MetricKey, fileName) as Measure<T>;

            if (existingMeasureFile == null)
            {
                sonarMeasuresJson.AddFileMeasure(new Measure<T>()
                {
                    MetricKey = this.metric.MetricKey,
                    Value = measureAggregator.GetValueForNewMeasure(dailyCodeChurn),
                    File = fileName
                });
            }
            else
            {
                existingMeasureFile.Value = measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, existingMeasureFile);
            }
        }

        public void ProcessProjectMeasure(SonarMeasuresJson sonarMeasuresJson)
        {
            if (!projectMeasure)
                return;

            ProcessMetric(sonarMeasuresJson);

            var existingMeasureProject = sonarMeasuresJson.FindProjectMeasure(metric.MetricKey) as Measure<T>;
            var projectMeasureAggregator = measureAggregator as IMeasureAggregatorProject<T>;

            if (existingMeasureProject == null)
            {
                sonarMeasuresJson.AddProjectMeasure(new Measure<T>()
                {
                    MetricKey = this.metric.MetricKey,
                    Value = projectMeasureAggregator.GetValueForProjectMeasure()
                });
            }
            else
            {
                existingMeasureProject.Value = projectMeasureAggregator.GetValueForProjectMeasure();
            }
        }

        private bool ValidDailyCodeChurn(DailyCodeChurn dailyCodeChurn)
        {
            if (dailyCodeChurn.GetDateTimeAsDateTime() < startDate || dailyCodeChurn.GetDateTimeAsDateTime() > endDate)
                return false;

            if (!measureAggregator.HasValue(dailyCodeChurn))
                return false;

            return true;
        }

        private string ProcessFileName(string fileName, string filePrefixToRemove)
        {
            if (filePrefixToRemove == null)
                return fileName;

            if (fileName.StartsWith(filePrefixToRemove, StringComparison.OrdinalIgnoreCase))
                return fileName.Substring(filePrefixToRemove.Length);

            return fileName;
        }

        private void ProcessMetric(SonarMeasuresJson sonarMeasuresJson)
        {
            if (processedMetric)
                return;

            sonarMeasuresJson.Metrics.Add(metric);

            processedMetric = true;
        }
    }
}
