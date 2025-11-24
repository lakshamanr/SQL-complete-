using System;
using System.IO;
using Newtonsoft.Json;

namespace SSMSSQLComplete.Core.Config
{
    public sealed class SettingsManager
    {
        private static readonly Lazy<SettingsManager> _instance =
            new Lazy<SettingsManager>(() => new SettingsManager());

        public static SettingsManager Instance => _instance.Value;

        private readonly string _settingsPath;
        private CompletionSettings _completionSettings;
        private FormattingSettings _formattingSettings;
        private SchemaSettings _schemaSettings;
        private TelemetrySettings _telemetrySettings;

        private SettingsManager()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SSMSSQLComplete",
                "Settings");

            if (!Directory.Exists(_settingsPath))
            {
                Directory.CreateDirectory(_settingsPath);
            }

            LoadSettings();
        }

        public CompletionSettings GetCompletionSettings()
        {
            return _completionSettings ?? (_completionSettings = new CompletionSettings());
        }

        public FormattingSettings GetFormattingSettings()
        {
            return _formattingSettings ?? (_formattingSettings = new FormattingSettings());
        }

        public SchemaSettings GetSchemaSettings()
        {
            return _schemaSettings ?? (_schemaSettings = new SchemaSettings());
        }

        public TelemetrySettings GetTelemetrySettings()
        {
            return _telemetrySettings ?? (_telemetrySettings = new TelemetrySettings());
        }

        public void SaveCompletionSettings(CompletionSettings settings)
        {
            _completionSettings = settings;
            SaveToFile("completion.json", settings);
        }

        public void SaveFormattingSettings(FormattingSettings settings)
        {
            _formattingSettings = settings;
            SaveToFile("formatting.json", settings);
        }

        public void SaveSchemaSettings(SchemaSettings settings)
        {
            _schemaSettings = settings;
            SaveToFile("schema.json", settings);
        }

        public void SaveTelemetrySettings(TelemetrySettings settings)
        {
            _telemetrySettings = settings;
            SaveToFile("telemetry.json", settings);
        }

        private void LoadSettings()
        {
            _completionSettings = LoadFromFile<CompletionSettings>("completion.json") ?? new CompletionSettings();
            _formattingSettings = LoadFromFile<FormattingSettings>("formatting.json") ?? new FormattingSettings();
            _schemaSettings = LoadFromFile<SchemaSettings>("schema.json") ?? new SchemaSettings();
            _telemetrySettings = LoadFromFile<TelemetrySettings>("telemetry.json") ?? new TelemetrySettings();
        }

        private T LoadFromFile<T>(string filename) where T : class
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, filename);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error loading settings from {filename}: {ex.Message}", ex);
            }

            return null;
        }

        private void SaveToFile<T>(string filename, T settings) where T : class
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, filename);
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error saving settings to {filename}: {ex.Message}", ex);
            }
        }
    }
}
