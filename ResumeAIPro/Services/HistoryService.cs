using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ResumeAIPro.Models;

namespace ResumeAIPro.Services
{
    public class HistoryService
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ResumeAIPro");
        private static readonly string HistoryFile = Path.Combine(AppFolder, "history.json");

        public List<AnalysisHistoryEntry> Load()
        {
            try
            {
                if (!File.Exists(HistoryFile)) return new();
                var json = File.ReadAllText(HistoryFile);
                return JsonConvert.DeserializeObject<List<AnalysisHistoryEntry>>(json) ?? new();
            }
            catch { return new(); }
        }

        public void Save(IEnumerable<AnalysisHistoryEntry> entries)
        {
            try
            {
                Directory.CreateDirectory(AppFolder);
                File.WriteAllText(HistoryFile,
                    JsonConvert.SerializeObject(entries, Formatting.Indented));
            }
            catch { }
        }
    }
}
