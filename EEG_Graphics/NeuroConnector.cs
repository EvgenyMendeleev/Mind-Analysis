﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;
using EEG_Graphics;

namespace NeuroTGAM
{
    /// <summary>
    /// Класс, способный работать с нейрогарнитурой, использующий чип TGAM
    /// </summary>
    public class NeuroConnector : NeuroModuleBase
    {
        private TcpClient _connector;                                        //TCP соединение с ThinkGear Connector.
        private Stream _connectorStream;
        private Mutex _mutex;                                                //Для синхронизации потоков.
        private BrainInfo _currentBrainData;                                 //Данные о мозговой деятельности в текущую секунду.

        /// <summary>
        /// Статус подключения к ThinkGear Connector.
        /// </summary>
        public override bool AreDataReading() => _connector.Connected;

        public NeuroConnector()
        {
            _mutex = new Mutex();
            _connector = new TcpClient();
            _currentBrainData = new BrainInfo();
        }

        /// <summary>
        /// Установить соединение с ThinkGear Connector.
        /// </summary>
        public override void Connect()
        {            
            _connector.Connect("localhost", 13854);
            _connectorStream = _connector.GetStream();

            byte[] settingsForConnector = Encoding.ASCII.GetBytes(@"{""enableRawOutput"": false,""format"": ""Json""}");
            
            _connectorStream.Write(settingsForConnector, 0, settingsForConnector.Length);
            _readingThread.Start();
        }

        /// <summary>
        /// Разорвать соединение с ThinkGear Connector.
        /// </summary>
        public override void CloseConnection() => _connector.Close();

        /// <summary>
        /// Возвращает данные о мозговой волне/состоянии в текущую секунду.
        /// </summary>
        public override BrainInfo GetCurrentBrainData()
        {
            _mutex.WaitOne();
            BrainInfo brainData = _currentBrainData.Clone() as BrainInfo;
            _mutex.ReleaseMutex();
            return brainData;
        }

        //Поток, считывающий информацию с нейрогарнитуры.
        protected override void ReadingThread()
        {
            try
            {
                byte[] bytesFromConnector = new byte[2048];
                while (_connector != null && _connector.Connected)
                {
                    int bytesRead = _connectorStream.Read(bytesFromConnector, 0, 2048);
                    if (bytesRead <= 0) continue;

                    string[] packets = Encoding.UTF8.GetString(bytesFromConnector, 0, bytesRead).Split('\r');
                    foreach (string packet in packets) if (!string.IsNullOrEmpty(packet)) ParseJSON(packet.Trim());
                }
            }
            catch
            {
                return;
            }
        }

        private void ParseJSON(string packet)
        {
            dynamic jsonObject = JObject.Parse(packet);
            if (jsonObject.eegPower == null && jsonObject.eSense == null || jsonObject.status != null) return;

            _mutex.WaitOne();
            _currentBrainData.alphaLow = (uint)jsonObject.eegPower["lowAlpha"];
            _currentBrainData.alphaHigh = (uint)jsonObject.eegPower["highAlpha"];

            _currentBrainData.betaLow = (uint)jsonObject.eegPower["lowBeta"];
            _currentBrainData.betaHigh = (uint)jsonObject.eegPower["highBeta"];

            _currentBrainData.gammaLow = (uint)jsonObject.eegPower["lowGamma"];
            _currentBrainData.gammaHigh= (uint)jsonObject.eegPower["highGamma"];

            _currentBrainData.theta = (uint)jsonObject.eegPower["theta"];
            _currentBrainData.delta = (uint)jsonObject.eegPower["delta"];

            _currentBrainData.attention = (uint)jsonObject.eSense["attention"];
            _currentBrainData.meditation = (uint)jsonObject.eSense["meditation"];
            _mutex.ReleaseMutex();

            OnBrainInfoReceived?.Invoke(_currentBrainData.Clone() as BrainInfo);
        }
    }
}