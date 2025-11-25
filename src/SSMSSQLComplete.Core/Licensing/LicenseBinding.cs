using System;
using System.Security.Cryptography;
using System.Text;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Binds licenses to specific hardware to prevent unauthorized transfers
    /// </summary>
    public class LicenseBinding
    {
        /// <summary>
        /// Creates a hardware-bound license key
        /// </summary>
        public static string CreateBoundLicenseKey(string baseLicenseKey, string hardwareFingerprint)
        {
            if (string.IsNullOrWhiteSpace(baseLicenseKey))
                throw new ArgumentNullException(nameof(baseLicenseKey));

            if (string.IsNullOrWhiteSpace(hardwareFingerprint))
                throw new ArgumentNullException(nameof(hardwareFingerprint));

            // Create a binding hash
            var bindingData = $"{baseLicenseKey}|{hardwareFingerprint}";
            var bindingHash = ComputeBindingHash(bindingData);

            // Return bound key format: BASE_KEY:BINDING_HASH
            return $"{baseLicenseKey}:{bindingHash}";
        }

        /// <summary>
        /// Validates that a bound license key matches the current hardware
        /// </summary>
        public static bool ValidateBinding(string boundLicenseKey, string expectedHardwareFingerprint)
        {
            if (string.IsNullOrWhiteSpace(boundLicenseKey))
                return false;

            if (string.IsNullOrWhiteSpace(expectedHardwareFingerprint))
                return false;

            try
            {
                // Parse bound key
                var parts = boundLicenseKey.Split(':');
                if (parts.Length != 2)
                    return false;

                var baseLicenseKey = parts[0];
                var storedBindingHash = parts[1];

                // Recompute binding hash with current hardware
                var bindingData = $"{baseLicenseKey}|{expectedHardwareFingerprint}";
                var computedBindingHash = ComputeBindingHash(bindingData);

                // Compare hashes
                return string.Equals(storedBindingHash, computedBindingHash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts the base license key from a bound license key
        /// </summary>
        public static string ExtractBaseLicenseKey(string boundLicenseKey)
        {
            if (string.IsNullOrWhiteSpace(boundLicenseKey))
                return null;

            var colonIndex = boundLicenseKey.IndexOf(':');
            if (colonIndex < 0)
                return boundLicenseKey; // Not a bound key, return as-is

            return boundLicenseKey.Substring(0, colonIndex);
        }

        /// <summary>
        /// Checks if a license key is hardware-bound
        /// </summary>
        public static bool IsBound(string licenseKey)
        {
            return !string.IsNullOrWhiteSpace(licenseKey) && licenseKey.Contains(":");
        }

        private static string ComputeBindingHash(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                var hash = sha256.ComputeHash(bytes);

                // Take first 16 bytes for shorter hash
                var shortHash = new byte[16];
                Array.Copy(hash, shortHash, 16);

                return BitConverter.ToString(shortHash).Replace("-", "");
            }
        }
    }
}
