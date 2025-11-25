using System;
using Microsoft.Win32;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Manages trial period for the add-in
    /// </summary>
    public class TrialManager
    {
        private const string REGISTRY_KEY = @"Software\SSMSSQLComplete";
        private const string TRIAL_START_VALUE = "TrialStartDate";
        private const string TRIAL_DAYS_VALUE = "TrialDaysUsed";
        private const int TRIAL_PERIOD_DAYS = 30;

        private static readonly Lazy<TrialManager> _instance =
            new Lazy<TrialManager>(() => new TrialManager());

        public static TrialManager Instance => _instance.Value;

        private TrialManager() { }

        public TrialInfo GetTrialInfo()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key == null)
                    {
                        // Trial not started yet
                        return new TrialInfo
                        {
                            IsTrialStarted = false,
                            DaysRemaining = TRIAL_PERIOD_DAYS,
                            IsExpired = false
                        };
                    }

                    var startDateStr = key.GetValue(TRIAL_START_VALUE) as string;
                    if (string.IsNullOrEmpty(startDateStr))
                    {
                        return new TrialInfo
                        {
                            IsTrialStarted = false,
                            DaysRemaining = TRIAL_PERIOD_DAYS,
                            IsExpired = false
                        };
                    }

                    if (!DateTime.TryParse(startDateStr, out DateTime startDate))
                    {
                        // Corrupted data, reset trial
                        return new TrialInfo
                        {
                            IsTrialStarted = false,
                            DaysRemaining = TRIAL_PERIOD_DAYS,
                            IsExpired = false
                        };
                    }

                    var elapsed = DateTime.UtcNow - startDate;
                    int daysUsed = (int)elapsed.TotalDays;
                    int daysRemaining = Math.Max(0, TRIAL_PERIOD_DAYS - daysUsed);
                    bool isExpired = daysRemaining == 0;

                    return new TrialInfo
                    {
                        IsTrialStarted = true,
                        StartDate = startDate,
                        DaysUsed = daysUsed,
                        DaysRemaining = daysRemaining,
                        IsExpired = isExpired,
                        ExpirationDate = startDate.AddDays(TRIAL_PERIOD_DAYS)
                    };
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error getting trial info: {ex.Message}", ex);
                return new TrialInfo
                {
                    IsTrialStarted = false,
                    DaysRemaining = TRIAL_PERIOD_DAYS,
                    IsExpired = false
                };
            }
        }

        public bool StartTrial()
        {
            try
            {
                // Check if trial already started
                var currentInfo = GetTrialInfo();
                if (currentInfo.IsTrialStarted)
                {
                    return false; // Trial already started
                }

                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key == null)
                        return false;

                    key.SetValue(TRIAL_START_VALUE, DateTime.UtcNow.ToString("O"));
                    key.SetValue(TRIAL_DAYS_VALUE, 0);
                }

                Infrastructure.Logger.Instance.Info("Trial period started");
                Infrastructure.TelemetryService.Instance.TrackEvent("TrialStarted");

                return true;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error starting trial: {ex.Message}", ex);
                return false;
            }
        }

        public void IncrementUsageDays()
        {
            try
            {
                var info = GetTrialInfo();
                if (!info.IsTrialStarted || info.IsExpired)
                    return;

                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    key?.SetValue(TRIAL_DAYS_VALUE, info.DaysUsed + 1);
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error incrementing usage days: {ex.Message}", ex);
            }
        }

        public void ResetTrial()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                {
                    key?.DeleteValue(TRIAL_START_VALUE, false);
                    key?.DeleteValue(TRIAL_DAYS_VALUE, false);
                }

                Infrastructure.Logger.Instance.Info("Trial period reset");
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error resetting trial: {ex.Message}", ex);
            }
        }
    }

    public class TrialInfo
    {
        public bool IsTrialStarted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int DaysUsed { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }

        public string GetDisplayMessage()
        {
            if (!IsTrialStarted)
            {
                return $"Trial version - {DaysRemaining} days available";
            }

            if (IsExpired)
            {
                return "Trial period has expired. Please purchase a license to continue using this add-in.";
            }

            return $"Trial version - {DaysRemaining} day{(DaysRemaining != 1 ? "s" : "")} remaining";
        }
    }
}
