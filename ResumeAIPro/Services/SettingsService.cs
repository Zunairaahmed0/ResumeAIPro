using System;
using System.IO;
using Newtonsoft.Json;
using ResumeAIPro.Models;

namespace ResumeAIPro.Services
{
    public class SettingsService
    {
        private readonly string _path;

        public SettingsService()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ResumeAIPro");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "settings.json");
        }

        private AppSettings Load()
        {
            try
            {
                if (File.Exists(_path))
                    return JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(_path)) ?? new();
            }
            catch { }
            return new();
        }

        private void Save(AppSettings s)
        {
            try { File.WriteAllText(_path, JsonConvert.SerializeObject(s, Formatting.Indented)); }
            catch { }
        }

        // Saved key takes priority; AppConfig.GeminiApiKey is the built-in fallback
        public string LoadApiKey()
        {
            var saved = Load().EffectiveGeminiKey;
            return !string.IsNullOrEmpty(saved) ? saved : AppConfig.GeminiApiKey;
        }
        public void SaveApiKey(string k) { var s = Load(); s.GeminiApiKey = k; Save(s); }

        // Saved Firebase key takes priority; AppConfig.FirebaseApiKey is the built-in fallback
        public string LoadFirebaseKey()
        {
            var saved = Load().FirebaseApiKey;
            return !string.IsNullOrEmpty(saved) ? saved : AppConfig.FirebaseApiKey;
        }
        public void SaveFirebaseKey(string k) { var s = Load(); s.FirebaseApiKey = k; Save(s); }

        public UserSession? LoadSession()       => Load().Session;
        public void   SaveSession(UserSession? session) { var s = Load(); s.Session = session; Save(s); }
        public void   ClearSession()            => SaveSession(null);

        private class AppSettings
        {
            // Old field kept for migration
            public string       ApiKey         { get; set; } = "";
            public string       GeminiApiKey   { get; set; } = "";
            public string       FirebaseApiKey { get; set; } = "";
            public UserSession? Session        { get; set; }

            // Migrate old ApiKey field on first load
            public string EffectiveGeminiKey => !string.IsNullOrEmpty(GeminiApiKey) ? GeminiApiKey : ApiKey;
        }
    }
}
