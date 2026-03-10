using CsvHelper;
using CsvHelper.Configuration;
using MindAnalysis.NeuroTGAM;
using NeuroTGAM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace MindAnalysis.GUI
{
    public enum WaveChart
    {
        Attention,
        Meditation,
        HighAlpha,
        LowAlpha,
        HighBeta,
        LowBeta,
        HighGamma,
        LowGamma,
        Theta,
        Delta
    }

    internal struct WaveChartDescription
    {
        public WaveChart waveChartType;
        public Chart waveChart;
    }

    internal class WavesCharts
    {
        private Dictionary<WaveChart, Chart> _charts;
        public decimal MaxPointsOnCharts { private get; set; }
        public DateTime StartTime { private get; set; }
        public WavesCharts(params WaveChartDescription[] charts)
        {
            _charts = new Dictionary<WaveChart, Chart>();
            foreach (var chart in charts)
            {
                _charts.Add(chart.waveChartType, chart.waveChart);
            }
        }

        public void AddPointOnSessionCharts(BrainInfo brainRecord)
        {
            TimeSpan elapsedTime = DateTime.Now - StartTime;
            elapsedTime = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
            brainRecord.Second = elapsedTime;

            if (MaxPointsOnCharts < _charts[WaveChart.HighAlpha].Series["HighAlpha"].Points.Count)
            {
                DeleteFirstPointsOnSessionCharts();
            }

            AddBrainRecordToCharts(brainRecord);
        }

        private void DeleteFirstPointsOnSessionCharts()
        {
            foreach (var chart in _charts.Keys)
            {
                string serieName = chart.ToString();
                _charts[chart].Series[serieName].Points.RemoveAt(0);
                _charts[chart].ResetAutoValues();
            }
        }

        private void AddBrainRecordToCharts(BrainInfo brainRecord, string seriesPref = "")
        {
            string time = brainRecord.Second.ToString();
            AddXY(WaveChart.Meditation, seriesPref, time, brainRecord.Meditation);
            AddXY(WaveChart.Attention, seriesPref, time, brainRecord.Attention);
            AddXY(WaveChart.HighAlpha, seriesPref, time, brainRecord.HighAlpha);
            AddXY(WaveChart.LowAlpha, seriesPref, time, brainRecord.LowAlpha);
            AddXY(WaveChart.HighBeta, seriesPref, time, brainRecord.HighBeta);
            AddXY(WaveChart.LowBeta, seriesPref, time, brainRecord.LowBeta);
            AddXY(WaveChart.HighGamma, seriesPref, time, brainRecord.HighGamma);
            AddXY(WaveChart.LowGamma, seriesPref, time, brainRecord.LowGamma);
            AddXY(WaveChart.Theta, seriesPref, time, brainRecord.Theta);
            AddXY(WaveChart.Delta, seriesPref, time, brainRecord.Delta);
        }

        private void AddXY(WaveChart waveChartType, string seriesPref, object XValue, object YValue)
        {
            string serieName = waveChartType.ToString();
            _charts[waveChartType].Series[seriesPref + serieName].Points.AddXY(XValue, YValue);
        }

        public void SetIntervalOX(WaveChart waveChart, uint interval)
        {
            string chartAreaName = waveChart.ToString();
            _charts[WaveChart.Attention].ChartAreas[chartAreaName].AxisX.Interval = interval;
        }

        public void LoadFileOnCharts(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);
            if (fileExtension != ".csv")
            {
                throw new Exception("WavesCharts.LoadFileToCharts(): Error extension file! CSV extension is need.");
            }
            
            MindFileReader mindReader = new MindFileReader(filePath);
            foreach (var brainRecord in mindReader)
            {
                AddBrainRecordToCharts(brainRecord, "Loaded");
            }
            mindReader.Close();
        }

        public void ClearAllCharts()
        {
            foreach (var chartType in _charts.Keys)
            {
                string waveName = chartType.ToString();
                _charts[chartType].Series[waveName].Points.Clear();
                _charts[chartType].Series[$"Loaded{waveName}"].Points.Clear();
            }
        }

        public void ClearLoadedRecords()
        {
            foreach (var chartType in _charts.Keys)
            {
                string waveName = chartType.ToString();
                _charts[chartType].Series[$"Loaded{waveName}"].Points.Clear();
            }
        }
    }
}
