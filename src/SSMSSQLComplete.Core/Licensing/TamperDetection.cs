using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Detects tampering with license data and application binaries
    /// </summary>
    public class TamperDetection
    {
        private const string REGISTRY_KEY = @"Software\SSMSSQLComplete";
        private const string CHECKSUM_VALUE = "Checksum";
        private const string LAST_VERIFIED_VALUE = "LastVerified";

        private static readonly Lazy<TamperDetection> _instance =
            new Lazy<TamperDetection>(() => new TamperDetection());

        public static TamperDetection Instance => _instance.Value;

        private TamperDetection() { }

        /// <summary>
        /// Performs comprehensive tamper detection
        /// </summary>
        public TamperDetectionResult DetectTampering()
        {
            var result = new TamperDetectionResult { IsTampered = false };

            try
            {
                // Check 1: Registry integrity
                if (!ValidateRegistryIntegrity())
                {
                    result.IsTampered = true;
                    result.TamperType = "Registry modification detected";
                    Infrastructure.Logger.Instance.Warn("Tamper detection: Registry integrity check failed");
                    return result;
                }

                // Check 2: Assembly integrity
                if (!ValidateAssemblyIntegrity())
                {
                    result.IsTampered = true;
                    result.TamperType = "Assembly modification detected";
                    Infrastructure.Logger.Instance.Warn("Tamper detection: Assembly integrity check failed");
                    return result;
                }

                // Check 3: Time manipulation
                if (DetectTimeManipulation())
                {
                    result.IsTampered = true;
                    result.TamperType = "System time manipulation detected";
                    Infrastructure.Logger.Instance.Warn("Tamper detection: Time manipulation detected");
                    return result;
                }

                // Check 4: Debugger attachment
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    result.IsDebuggerAttached = true;
                    Infrastructure.Logger.Instance.Info("Debugger detected (allowed in development)");
                }

                result.LastCheckTime = DateTime.UtcNow;
                UpdateLastVerified();

                return result;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error in tamper detection: {ex.Message}", ex);
                result.IsTampered = false; // Don't block on error
                return result;
            }
        }

        private bool ValidateRegistryIntegrity()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key == null)
                        return true; // No registry data yet

                    var licenseKey = key.GetValue("LicenseKey") as string;
                    if (string.IsNullOrEmpty(licenseKey))
                        return true;

                    // Compute checksum of registry data
                    var data = $"{licenseKey}|{key.GetValue("ActivationDate")}|{key.GetValue("MachineId")}";
                    var currentChecksum = ComputeChecksum(data);

                    var storedChecksum = key.GetValue(CHECKSUM_VALUE) as string;
                    if (string.IsNullOrEmpty(storedChecksum))
                    {
                        // First time, store checksum
                        StoreChecksum(currentChecksum);
                        return true;
                    }

                    // Verify checksum matches
                    return string.Equals(currentChecksum, storedChecksum, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return true; // Don't block on error
            }
        }

        private bool ValidateAssemblyIntegrity()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var location = assembly.Location;

                if (!File.Exists(location))
                    return true; // Can't verify, allow

                // Check if assembly is signed (in production, verify strong name)
                var name = assembly.GetName();
                var publicKeyToken = name.GetPublicKeyToken();

                // In production, verify the public key token matches expected value
                // For now, just check if it's signed
                if (publicKeyToken == null || publicKeyToken.Length == 0)
                {
                    Infrastructure.Logger.Instance.Warn("Assembly is not strongly named");
                }

                return true; // Allow for now
            }
            catch
            {
                return true; // Don't block on error
            }
        }

        private bool DetectTimeManipulation()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key == null)
                        return false;

                    var lastVerifiedStr = key.GetValue(LAST_VERIFIED_VALUE) as string;
                    if (string.IsNullOrEmpty(lastVerifiedStr))
                        return false;

                    if (!DateTime.TryParse(lastVerifiedStr, out DateTime lastVerified))
                        return false;

                    var now = DateTime.UtcNow;

                    // Check if current time is before last verification time
                    if (now < lastVerified.AddMinutes(-5)) // 5 minute tolerance for clock drift
                    {
                        Infrastructure.Logger.Instance.Warn($"Time manipulation detected: Current={now}, Last={lastVerified}");
                        return true;
                    }

                    // Check for unreasonable time jumps (more than 1 day)
                    var timeDiff = now - lastVerified;
                    if (timeDiff.TotalDays > 1)
                    {
                        // This is normal for system sleep/hibernate, allow it
                    }

                    return false;
                }
            }
            catch
            {
                return false; // Don't block on error
            }
        }

        private void StoreChecksum(string checksum)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    key?.SetValue(CHECKSUM_VALUE, checksum);
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error storing checksum: {ex.Message}", ex);
            }
        }

        private void UpdateLastVerified()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    key?.SetValue(LAST_VERIFIED_VALUE, DateTime.UtcNow.ToString("O"));
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error updating last verified: {ex.Message}", ex);
            }
        }

        private string ComputeChecksum(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        /// <summary>
        /// Updates the registry checksum after legitimate license changes
        /// </summary>
        public void UpdateChecksum(string licenseKey, string activationDate, string machineId)
        {
            try
            {
                var data = $"{licenseKey}|{activationDate}|{machineId}";
                var checksum = ComputeChecksum(data);
                StoreChecksum(checksum);
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error updating checksum: {ex.Message}", ex);
            }
        }
    }

    public class TamperDetectionResult
    {
        public bool IsTampered { get; set; }
        public string TamperType { get; set; }
        public bool IsDebuggerAttached { get; set; }
        public DateTime? LastCheckTime { get; set; }

        public string GetMessage()
        {
            if (IsTampered)
            {
                return $"License tampering detected: {TamperType}. Please reinstall the application or contact support.";
            }

            return "License integrity verified";
        }
    }
}
