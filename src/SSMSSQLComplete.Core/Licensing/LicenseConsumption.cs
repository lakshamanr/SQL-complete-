using System;
using Microsoft.Win32;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Tracks license consumption and usage to prevent abuse
    /// </summary>
    public class LicenseConsumption
    {
        private const string REGISTRY_KEY = @"Software\SSMSSQLComplete";
        private const string ACTIVATION_COUNT_VALUE = "ActivationCount";
        private const string FIRST_ACTIVATION_VALUE = "FirstActivation";
        private const string LAST_USE_VALUE = "LastUse";
        private const string TOTAL_USES_VALUE = "TotalUses";
        private const string DEACTIVATION_COUNT_VALUE = "DeactivationCount";

        private static readonly Lazy<LicenseConsumption> _instance =
            new Lazy<LicenseConsumption>(() => new LicenseConsumption());

        public static LicenseConsumption Instance => _instance.Value;

        private LicenseConsumption() { }

        /// <summary>
        /// Records a license activation
        /// </summary>
        public void RecordActivation()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key == null)
                        return;

                    // Increment activation count
                    var currentCount = (int)(key.GetValue(ACTIVATION_COUNT_VALUE) ?? 0);
                    key.SetValue(ACTIVATION_COUNT_VALUE, currentCount + 1);

                    // Record first activation
                    if (key.GetValue(FIRST_ACTIVATION_VALUE) == null)
                    {
                        key.SetValue(FIRST_ACTIVATION_VALUE, DateTime.UtcNow.ToString("O"));
                    }

                    Infrastructure.Logger.Instance.Info($"License activation recorded (count: {currentCount + 1})");
                    Infrastructure.TelemetryService.Instance.TrackEvent("LicenseActivationRecorded");
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error recording activation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Records a license deactivation
        /// </summary>
        public void RecordDeactivation()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key == null)
                        return;

                    var currentCount = (int)(key.GetValue(DEACTIVATION_COUNT_VALUE) ?? 0);
                    key.SetValue(DEACTIVATION_COUNT_VALUE, currentCount + 1);

                    Infrastructure.Logger.Instance.Info($"License deactivation recorded (count: {currentCount + 1})");
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error recording deactivation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Records license usage
        /// </summary>
        public void RecordUsage()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key == null)
                        return;

                    // Update last use time
                    key.SetValue(LAST_USE_VALUE, DateTime.UtcNow.ToString("O"));

                    // Increment total uses
                    var totalUses = (int)(key.GetValue(TOTAL_USES_VALUE) ?? 0);
                    key.SetValue(TOTAL_USES_VALUE, totalUses + 1);
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error recording usage: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets consumption statistics
        /// </summary>
        public ConsumptionStats GetStats()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key == null)
                    {
                        return new ConsumptionStats();
                    }

                    var stats = new ConsumptionStats
                    {
                        ActivationCount = (int)(key.GetValue(ACTIVATION_COUNT_VALUE) ?? 0),
                        DeactivationCount = (int)(key.GetValue(DEACTIVATION_COUNT_VALUE) ?? 0),
                        TotalUses = (int)(key.GetValue(TOTAL_USES_VALUE) ?? 0)
                    };

                    var firstActivationStr = key.GetValue(FIRST_ACTIVATION_VALUE) as string;
                    if (DateTime.TryParse(firstActivationStr, out DateTime firstActivation))
                    {
                        stats.FirstActivation = firstActivation;
                    }

                    var lastUseStr = key.GetValue(LAST_USE_VALUE) as string;
                    if (DateTime.TryParse(lastUseStr, out DateTime lastUse))
                    {
                        stats.LastUse = lastUse;
                    }

                    return stats;
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error getting consumption stats: {ex.Message}", ex);
                return new ConsumptionStats();
            }
        }

        /// <summary>
        /// Detects suspicious activation patterns
        /// </summary>
        public bool DetectAbusePattern()
        {
            try
            {
                var stats = GetStats();

                // Check for excessive reactivations (more than 10)
                if (stats.ActivationCount > 10)
                {
                    Infrastructure.Logger.Instance.Warn($"Suspicious activation pattern: {stats.ActivationCount} activations");
                    return true;
                }

                // Check for rapid activation/deactivation cycles
                if (stats.ActivationCount > 3 && stats.DeactivationCount > 3)
                {
                    var ratio = (double)stats.DeactivationCount / stats.ActivationCount;
                    if (ratio > 0.8) // More than 80% deactivation rate
                    {
                        Infrastructure.Logger.Instance.Warn("Suspicious activation/deactivation pattern");
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ConsumptionStats
    {
        public int ActivationCount { get; set; }
        public int DeactivationCount { get; set; }
        public int TotalUses { get; set; }
        public DateTime? FirstActivation { get; set; }
        public DateTime? LastUse { get; set; }

        public TimeSpan GetUsageDuration()
        {
            if (!FirstActivation.HasValue || !LastUse.HasValue)
                return TimeSpan.Zero;

            return LastUse.Value - FirstActivation.Value;
        }
    }
}
