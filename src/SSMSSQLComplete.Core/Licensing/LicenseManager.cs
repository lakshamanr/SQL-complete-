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
                // Perform tamper detection
                var tamperResult = TamperDetection.Instance.DetectTampering();
                if (tamperResult.IsTampered)
                {
                    Infrastructure.Logger.Instance.Error($"License tampered: {tamperResult.TamperType}");
                    return new LicenseInfo
                    {
                        Type = LicenseType.Unlicensed,
                        Status = LicenseStatus.Invalid,
                        LicensedTo = "Tampered"
                    };
                }

                // Record usage
                LicenseConsumption.Instance.RecordUsage();

                if (_currentLicense != null && _currentLicense.IsValid())
                {
                    // Validate hardware binding
                    if (!string.IsNullOrEmpty(_currentLicense.MachineId))
                    {
                        var currentHardware = HardwareFingerprint.Instance.Generate();
                        if (!string.Equals(_currentLicense.MachineId, currentHardware, StringComparison.OrdinalIgnoreCase))
                        {
                            Infrastructure.Logger.Instance.Warn("Hardware mismatch detected");
                            return new LicenseInfo
                            {
                                Type = LicenseType.Unlicensed,
                                Status = LicenseStatus.Invalid,
                                LicensedTo = "Hardware Mismatch"
                            };
                        }
                    }

                    return _currentLicense;
                }

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
                    // Check for abuse patterns
                    if (LicenseConsumption.Instance.DetectAbusePattern())
                    {
                        Infrastructure.Logger.Instance.Warn("Suspicious activation pattern detected");
                        return ActivationResult.Failure("Too many activation attempts. Please contact support.");
                    }

                    // Extract base license key if hardware-bound
                    var baseLicenseKey = LicenseBinding.ExtractBaseLicenseKey(licenseKey);

                    // Validate the license key
                    var validationResult = _validator.ValidateLicenseKey(baseLicenseKey);

                    if (!validationResult.IsValid)
                    {
                        Infrastructure.Logger.Instance.Warn($"License activation failed: {validationResult.ErrorMessage}");
                        return ActivationResult.Failure(validationResult.ErrorMessage);
                    }

                    // Get hardware fingerprint
                    var hardwareFingerprint = HardwareFingerprint.Instance.Generate();

                    // If license is hardware-bound, validate binding
                    if (LicenseBinding.IsBound(licenseKey))
                    {
                        if (!LicenseBinding.ValidateBinding(licenseKey, hardwareFingerprint))
                        {
                            Infrastructure.Logger.Instance.Warn("Hardware binding validation failed");
                            return ActivationResult.Failure("This license key is bound to different hardware. Please contact support for a transfer.");
                        }
                    }

                    var licenseInfo = validationResult.LicenseInfo;
                    licenseInfo.LicenseKey = licenseKey;
                    licenseInfo.LicensedTo = licensedTo ?? "Licensed User";
                    licenseInfo.CompanyName = companyName;
                    licenseInfo.ActivationDate = DateTime.UtcNow;
                    licenseInfo.MachineId = hardwareFingerprint;

                    // Store license
                    if (!StoreLicense(licenseInfo))
                    {
                        return ActivationResult.Failure("Failed to store license information");
                    }

                    _currentLicense = licenseInfo;

                    // Record activation
                    LicenseConsumption.Instance.RecordActivation();

                    // Update tamper detection checksum
                    TamperDetection.Instance.UpdateChecksum(
                        licenseKey,
                        licenseInfo.ActivationDate?.ToString("O"),
                        hardwareFingerprint);

                    Infrastructure.Logger.Instance.Info($"License activated successfully: {licenseInfo.Type}");
                    Infrastructure.TelemetryService.Instance.TrackEvent("LicenseActivated", new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "LicenseType", licenseInfo.Type.ToString() },
                        { "HasExpiration", licenseInfo.ExpirationDate.HasValue.ToString() },
                        { "HardwareBound", LicenseBinding.IsBound(licenseKey).ToString() }
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
                    // Record deactivation
                    LicenseConsumption.Instance.RecordDeactivation();

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

                    // Extract base license key if hardware-bound
                    var baseLicenseKey = LicenseBinding.ExtractBaseLicenseKey(licenseKey);

                    // Validate hardware binding if present
                    var currentHardware = HardwareFingerprint.Instance.Generate();
                    if (LicenseBinding.IsBound(licenseKey))
                    {
                        if (!LicenseBinding.ValidateBinding(licenseKey, currentHardware))
                        {
                            Infrastructure.Logger.Instance.Warn("Stored license has invalid hardware binding");
                            return;
                        }
                    }

                    var validationResult = _validator.ValidateLicenseKey(baseLicenseKey);
                    if (validationResult.IsValid)
                    {
                        var licenseInfo = validationResult.LicenseInfo;
                        licenseInfo.LicenseKey = licenseKey;

                        var activationDateStr = key.GetValue(ACTIVATION_DATE_VALUE) as string;
                        if (DateTime.TryParse(activationDateStr, out DateTime activationDate))
                        {
                            licenseInfo.ActivationDate = activationDate;
                        }

                        licenseInfo.MachineId = currentHardware;

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
            return HardwareFingerprint.Instance.Generate();
        }

        public string GetMachineIdForActivation()
        {
            return HardwareFingerprint.Instance.Generate();
        }

        public string GetShortMachineId()
        {
            return HardwareFingerprint.Instance.GetShortId();
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
