using MindAnalysis.GUI;
using MindAnalysis.NeuroTGAM;
using NeuroTGAM;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using AttentionAnalysis;
using Microsoft.Scripting.Utils;

namespace MindAnalysis
{
    public partial class MainForm : Form
    {
        private delegate void SessionChartDisplay(BrainInfo brainInfo);
        
        private Neurointerface _neurodevice;
        private WavesCharts _wavesCharts;
        private MindFileWriter _mindFile;

        public MainForm()
        {
            InitializeComponent();
            InitializeChartsStorage();
        }

        private void InitializeChartsStorage()
        {
            _wavesCharts = new WavesCharts
                (
                new WaveChartDescription { waveChartType = WaveChart.Attention, waveChart = AttentionAndMeditationChart },
                new WaveChartDescription { waveChartType = WaveChart.Meditation, waveChart = AttentionAndMeditationChart },
                new WaveChartDescription { waveChartType = WaveChart.HighAlpha, waveChart = AlphaWaveChart },
                new WaveChartDescription { waveChartType = WaveChart.LowAlpha, waveChart = AlphaWaveChart },
                new WaveChartDescription { waveChartType = WaveChart.HighBeta, waveChart = BetaWaveChart },
                new WaveChartDescription { waveChartType = WaveChart.LowBeta, waveChart = BetaWaveChart },
                new WaveChartDescription { waveChartType = WaveChart.HighGamma, waveChart = GammaWaveChart },
                new WaveChartDescription { waveChartType = WaveChart.LowGamma, waveChart = GammaWaveChart },
                new WaveChartDescription { waveChartType = WaveChart.Theta, waveChart = ThetaAndDeltaWavesChart },
                new WaveChartDescription { waveChartType = WaveChart.Delta, waveChart = ThetaAndDeltaWavesChart }
                );
        }

        private void StartRecording(object sender, EventArgs e)
        {
            try
            {
                _neurodevice = new Neurointerface();
                _neurodevice.OnBrainDataReceived += OnBrainDataReceived;
                _neurodevice.Connect();
            }
            catch
            {
                MessageBox.Show("Ошибка подключения к нейроинтерфейсу", "Ошибка соединения!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _wavesCharts.MaxPointsOnCharts = numMaxChartPoints.Value;
            _wavesCharts.StartTime = DateTime.Now;
            _wavesCharts.ClearAllCharts();
            _wavesCharts.SetIntervalOX(WaveChart.Attention, 0);

            if (chkSaveRecords.Checked)
            {
                _mindFile = new MindFileWriter(txtBoxFilePath.Text);
            }

            var GUISystem = UserControlSystem.GetSystem();
            GUISystem.DisableParams(btnStartRecord, numMaxChartPoints, chkSaveRecords, txtBoxFilePath, mainMenuStrip);
            GUISystem.EnableParams(btnStopRecord);
        }

        private void OnBrainDataReceived(BrainInfo brainInfo)
        {
            Invoke(new SessionChartDisplay(_wavesCharts.AddPointOnSessionCharts), brainInfo);
            _mindFile?.AppendRecord(brainInfo);
        }

        private void StopRecording(object sender, EventArgs e)
        {
            if (!_neurodevice.IsDataReading)
            {
                MessageBox.Show("Соединение с ThinkGear Connector не установлено.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _neurodevice.CloseConnection();
            _neurodevice = null;

            _mindFile?.Close();
            _mindFile = null;

            var GUISystem = UserControlSystem.GetSystem();
            GUISystem.DisableParams(btnStopRecord);
            GUISystem.EnableParams(btnStartRecord, numMaxChartPoints, chkSaveRecords, txtBoxFilePath, mainMenuStrip);
        }

        private void OnChangedValueInSaveMindRecord(object sender, EventArgs e)
        {
            UserControlSystem GUISystem = UserControlSystem.GetSystem();

            if (chkSaveRecords.Checked)
            {
                GUISystem.EnableParams(btnChangeSavePath, txtBoxFilePath);
            }
            else
            {
                GUISystem.DisableParams(btnChangeSavePath, txtBoxFilePath);
            }
        }

        private void LoadFileOnChart(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Csv files (*.csv) | *.csv" })
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                _wavesCharts.ClearAllCharts();
                _wavesCharts.SetIntervalOX(WaveChart.Attention, 60);
                _wavesCharts.SetIntervalOX(WaveChart.Meditation, 60);
                _wavesCharts.LoadFileOnCharts(openFileDialog.FileName);
            }
        }

        private void SaveFilePathForRecording(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Csv files (*.csv) | *.csv" };
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (saveFileDialog.FileName.Length > 3)
            {
                txtBoxFilePath.Text = Path.GetFullPath(saveFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("Имя файла слишком маленькое!", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearLoadedRecords(object sender, EventArgs e)
        {
            _wavesCharts.ClearLoadedRecords();
        }

        private void OpenMarkingDatasetDialog(object sender, EventArgs e)
        {
            MarkingDatasetForm markingDatasetForm = new MarkingDatasetForm();
            markingDatasetForm.ShowDialog(this);
        }
    }
}


//Программный код сглаживания точек функции
/*MindFileReader mindFile = new MindFileReader(openFileDialog.FileName); 

List<DataPoint> startedPoints = new List<DataPoint>();
int a = 1;
foreach (var brainInfo in mindFile)
{
    DataPoint attentionPoint = new DataPoint(a, brainInfo.Attention);
    startedPoints.Add(attentionPoint);
    a++;
}

DataPoint[] smoothedPoints = DataSmoothing.ExponentialSmoothing(startedPoints.ToArray());

smoothedChart.Series["SmoothedChart"].Points.AddRange(smoothedPoints);
smoothedChart.Series["StartPoints"].Points.AddRange(startedPoints);*/