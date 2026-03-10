using CsvHelper;
using NeuroTGAM;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using CsvHelper.Configuration;
using System.Runtime.CompilerServices;

namespace MindAnalysis.NeuroTGAM
{
    internal class MindFileWriter
    {
        private CsvWriter _csvWriter;

        public MindFileWriter(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            
            StreamWriter writer = new StreamWriter(file);
            _csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);

            _csvWriter.WriteHeader<BrainInfo>();
            _csvWriter.NextRecord();
        }

        public void AppendRecord(BrainInfo brainInfo)
        {
            _csvWriter.WriteRecord(brainInfo);
            _csvWriter.NextRecord();
        }

        public void Close()
        {
            _csvWriter.Dispose();
        }
    }

    internal class MindFileReader
    {
        private readonly CsvReader _reader;

        public MindFileReader(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            StreamReader reader = new StreamReader(file);
            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true };
            _reader = new CsvReader(reader, csvConfig);
        }

        public IEnumerator<BrainInfo> GetEnumerator()
        {
            var brainRecords = _reader.GetRecords<BrainInfo>();
            foreach (var brainRecord in brainRecords)
            {
                yield return brainRecord;
            }
        }

        public void Close()
        {
            _reader.Dispose();
        }
    }
}
