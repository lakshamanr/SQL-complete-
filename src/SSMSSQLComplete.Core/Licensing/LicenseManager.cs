using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Central manager for license validation and storage
    /// </summary>
    public class LicenseManager
    {
        private const string REGISTRY_KEY = @"Software\SSMSSQLComplete";
        private const string LICENSE_KEY_VALUE = "LicenseKey";
        private const string ACTIVATION_DATE_VALUE = "ActivationDate";
        private const string MACHINE_ID_VALUE = "MachineId";

        private static readonly Lazy<LicenseManager> _instance =
            new Lazy<LicenseManager>(() => new LicenseManager());

        public static LicenseManager Instance => _instance.Value;

        private readonly LicenseValidator _validator;
        private LicenseInfo _currentLicense;
        private readonly object _lock = new object();

        private LicenseManager()
        {
            _validator = new LicenseValidator();
            LoadLicense();
        }

        public LicenseInfo GetCurrentLicense()
        {
            lock (_lock)
            {
                if (_currentLicense != null && _currentLicense.IsValid())
                    return _currentLicense;

                // Check for trial
                var trialInfo = TrialManager.Instance.GetTrialInfo();
                if (!trialInfo.IsExpired)
                {
                    return new LicenseInfo
                    {
                        Type = LicenseType.Trial,
                        Status = LicenseStatus.Active,
                        ExpirationDate = trialInfo.ExpirationDate,
                        LicensedTo = "Trial User"
                    };
                }

                // No valid license
                return new LicenseInfo
                {
                    Type = LicenseType.Unlicensed,
                    Status = LicenseStatus.Unlicensed,
                    LicensedTo = "Unlicensed"
                };
            }
        }

        public bool IsLicensed()
        {
            var license = GetCurrentLicense();
            return license.IsValid();
        }

        public ActivationResult ActivateLicense(string licenseKey, string licensedTo = null, string companyName = null)
        {
            lock (_lock)
            {
                try
                {
                    // Validate the license key
                    var validationResult = _validator.ValidateLicenseKey(licenseKey);

                    if (!validationResult.IsValid)
                    {
                        Infrastructure.Logger.Instance.Warn($"License activation failed: {validationResult.ErrorMessage}");
                        return ActivationResult.Failure(validationResult.ErrorMessage);
                    }

                    var licenseInfo = validationResult.LicenseInfo;
                    licenseInfo.LicenseKey = licenseKey;
                    licenseInfo.LicensedTo = licensedTo ?? "Licensed User";
                    licenseInfo.CompanyName = companyName;
                    licenseInfo.ActivationDate = DateTime.UtcNow;
                    licenseInfo.MachineId = GetMachineId();

                    // Store license
                    if (!StoreLicense(licenseInfo))
                    {
                        return ActivationResult.Failure("Failed to store license information");
                    }

                    _currentLicense = licenseInfo;

                    Infrastructure.Logger.Instance.Info($"License activated successfully: {licenseInfo.Type}");
                    Infrastructure.TelemetryService.Instance.TrackEvent("LicenseActivated", new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "LicenseType", licenseInfo.Type.ToString() },
                        { "HasExpiration", licenseInfo.ExpirationDate.HasValue.ToString() }
                    });

                    return ActivationResult.Success(licenseInfo);
                }
                catch (Exception ex)
                {
                    Infrastructure.Logger.Instance.Error($"Error activating license: {ex.Message}", ex);
                    return ActivationResult.Failure($"Activation error: {ex.Message}");
                }
            }
        }

        public void DeactivateLicense()
        {
            lock (_lock)
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                    {
                        key?.DeleteValue(LICENSE_KEY_VALUE, false);
                        key?.DeleteValue(ACTIVATION_DATE_VALUE, false);
                    }

                    _currentLicense = null;

                    Infrastructure.Logger.Instance.Info("License deactivated");
                    Infrastructure.TelemetryService.Instance.TrackEvent("LicenseDeactivated");
                }
                catch (Exception ex)
                {
                    Infrastructure.Logger.Instance.Error($"Error deactivating license: {ex.Message}", ex);
                }
            }
        }

        private void LoadLicense()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key == null)
                        return;

                    var licenseKey = key.GetValue(LICENSE_KEY_VALUE) as string;
                    if (string.IsNullOrEmpty(licenseKey))
                        return;

                    var validationResult = _validator.ValidateLicenseKey(licenseKey);
                    if (validationResult.IsValid)
                    {
                        var licenseInfo = validationResult.LicenseInfo;
                        licenseInfo.LicenseKey = licenseKey;

                        var activationDateStr = key.GetValue(ACTIVATION_DATE_VALUE) as string;
                        if (DateTime.TryParse(activationDateStr, out DateTime activationDate))
                        {
                            licenseInfo.ActivationDate = activationDate;
                        }

                        licenseInfo.MachineId = GetMachineId();

                        _currentLicense = licenseInfo;

                        Infrastructure.Logger.Instance.Info($"License loaded: {licenseInfo.Type}");
                    }
                    else
                    {
                        Infrastructure.Logger.Instance.Warn($"Stored license is invalid: {validationResult.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error loading license: {ex.Message}", ex);
            }
        }

        private bool StoreLicense(LicenseInfo licenseInfo)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key == null)
                        return false;

                    key.SetValue(LICENSE_KEY_VALUE, licenseInfo.LicenseKey);
                    key.SetValue(ACTIVATION_DATE_VALUE, licenseInfo.ActivationDate?.ToString("O") ?? DateTime.UtcNow.ToString("O"));
                    key.SetValue(MACHINE_ID_VALUE, licenseInfo.MachineId);
                }

                return true;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error storing license: {ex.Message}", ex);
                return false;
            }
        }

        private string GetMachineId()
        {
            try
            {
                // Generate machine-specific ID based on hardware characteristics
                var machineInfo = $"{Environment.MachineName}|{Environment.ProcessorCount}|{Environment.OSVersion}";

                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
                    return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
                }
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        public string GetMachineIdForActivation()
        {
            return GetMachineId();
        }
    }

    public class ActivationResult
    {
        public bool IsSuccessful { get; private set; }
        public string ErrorMessage { get; private set; }
        public LicenseInfo LicenseInfo { get; private set; }

        private ActivationResult() { }

        public static ActivationResult Success(LicenseInfo licenseInfo)
        {
            return new ActivationResult
            {
                IsSuccessful = true,
                LicenseInfo = licenseInfo
            };
        }

        public static ActivationResult Failure(string errorMessage)
        {
            return new ActivationResult
            {
                IsSuccessful = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
