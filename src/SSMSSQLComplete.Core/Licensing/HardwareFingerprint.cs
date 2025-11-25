using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Generates a unique, tamper-resistant hardware fingerprint
    /// </summary>
    public class HardwareFingerprint
    {
        private static readonly Lazy<HardwareFingerprint> _instance =
            new Lazy<HardwareFingerprint>(() => new HardwareFingerprint());

        public static HardwareFingerprint Instance => _instance.Value;

        private readonly object _lock = new object();
        private string _cachedFingerprint;

        private HardwareFingerprint() { }

        /// <summary>
        /// Generates a unique hardware fingerprint based on multiple hardware characteristics
        /// </summary>
        public string Generate()
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_cachedFingerprint))
                    return _cachedFingerprint;

                try
                {
                    var components = new List<string>();

                    // Collect hardware identifiers
                    components.Add(GetProcessorId());
                    components.Add(GetMotherboardSerial());
                    components.Add(GetBiosSerial());
                    components.Add(GetMacAddress());
                    components.Add(GetDiskSerial());
                    components.Add(GetSystemUuid());

                    // Combine and hash
                    var combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));
                    _cachedFingerprint = ComputeHash(combined);

                    Infrastructure.Logger.Instance.Info($"Hardware fingerprint generated: {_cachedFingerprint.Substring(0, 8)}...");
                    return _cachedFingerprint;
                }
                catch (Exception ex)
                {
                    Infrastructure.Logger.Instance.Error($"Error generating hardware fingerprint: {ex.Message}", ex);

                    // Fallback to basic fingerprint
                    var fallback = $"{Environment.MachineName}|{Environment.ProcessorCount}|{Environment.OSVersion}";
                    _cachedFingerprint = ComputeHash(fallback);
                    return _cachedFingerprint;
                }
            }
        }

        /// <summary>
        /// Validates if the current hardware matches the expected fingerprint
        /// </summary>
        public bool Validate(string expectedFingerprint)
        {
            if (string.IsNullOrWhiteSpace(expectedFingerprint))
                return false;

            var currentFingerprint = Generate();
            return string.Equals(currentFingerprint, expectedFingerprint, StringComparison.OrdinalIgnoreCase);
        }

        private string GetProcessorId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    var processor = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    return processor?["ProcessorId"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetMotherboardSerial()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    var board = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    return board?["SerialNumber"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetBiosSerial()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    var bios = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    return bios?["SerialNumber"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetMacAddress()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL"))
                {
                    var adapter = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    return adapter?["MACAddress"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetDiskSerial()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia"))
                {
                    var disk = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    var serial = disk?["SerialNumber"]?.ToString()?.Trim();
                    return string.IsNullOrWhiteSpace(serial) ? string.Empty : serial;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetSystemUuid()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct"))
                {
                    var system = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    return system?["UUID"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        /// <summary>
        /// Gets a short hardware ID for display purposes
        /// </summary>
        public string GetShortId()
        {
            var fullId = Generate();
            return fullId.Substring(0, Math.Min(16, fullId.Length));
        }

        /// <summary>
        /// Clears the cached fingerprint (for testing purposes)
        /// </summary>
        public void ClearCache()
        {
            lock (_lock)
            {
                _cachedFingerprint = null;
            }
        }
    }
}
