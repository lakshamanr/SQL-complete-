using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Validates license keys using RSA signature verification
    /// </summary>
    public class LicenseValidator
    {
        // Public key for license verification (private key kept secure on license server)
        private const string PUBLIC_KEY = @"<RSAKeyValue>
<Modulus>xKZQz8jJ9vYDN8K3L4F9zR2wX5mP1qT7nB8dV6cH4kL9sW2eF3gY1jN5pM8tR7vK4wZ2xC3yA5bD6fE8hG9iJ0kL1mN2oP3qR4sT5uV6wX7yZ8aB9cD0eF1gH2iJ3kL4mN5oP6qR7sT8uV9wX0yZ1aC2bD3cE4fG5hI6jK7lM8nO9pQ0rS1tU2vW3xY4zA5BC6DE7FG8HI9JK0LM1NO2PQ3RS4TU5VW6XY7Z</Modulus>
<Exponent>AQAB</Exponent>
</RSAKeyValue>";

        // License key format: PREFIX-XXXXX-XXXXX-XXXXX-XXXXX-SIGNATURE
        // Example: SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123456789ABCDEF
        private static readonly Regex LicenseKeyPattern = new Regex(
            @"^SSMS-([A-Z0-9]{5})-([A-Z0-9]{5})-([A-Z0-9]{5})-([A-Z0-9]{5})-([A-Z0-9]+)$",
            RegexOptions.Compiled);

        public ValidationResult ValidateLicenseKey(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                return ValidationResult.Failure("License key cannot be empty");
            }

            // Check format
            var match = LicenseKeyPattern.Match(licenseKey.Trim().ToUpper());
            if (!match.Success)
            {
                return ValidationResult.Failure("Invalid license key format");
            }

            try
            {
                // Extract components
                string dataSegment = $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}-{match.Groups[4].Value}";
                string signature = match.Groups[5].Value;

                // Decode the data segment
                var licenseData = DecodeLicenseData(dataSegment);

                // Verify signature
                if (!VerifySignature(dataSegment, signature))
                {
                    return ValidationResult.Failure("Invalid license signature");
                }

                // Check expiration
                if (licenseData.ExpirationDate.HasValue && DateTime.UtcNow > licenseData.ExpirationDate.Value)
                {
                    return ValidationResult.Failure("License has expired", licenseData);
                }

                // Check product version compatibility
                if (!IsVersionCompatible(licenseData.ProductVersion))
                {
                    return ValidationResult.Failure("License not valid for this version", licenseData);
                }

                return ValidationResult.Success(licenseData);
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Error validating license: {ex.Message}");
            }
        }

        private LicenseInfo DecodeLicenseData(string dataSegment)
        {
            // Remove hyphens and decode Base32-like encoding
            string encoded = dataSegment.Replace("-", "");

            // Decode the license data (simplified - in production use proper Base32)
            byte[] data = ConvertFromBase32(encoded);

            // Parse the binary data structure:
            // Bytes 0-3: License type (1=Trial, 2=Perpetual, 3=Subscription)
            // Bytes 4-7: Expiration timestamp (Unix epoch, 0 for perpetual)
            // Bytes 8-9: Major.Minor version

            int licenseType = BitConverter.ToInt32(data, 0);
            int expirationTimestamp = BitConverter.ToInt32(data, 4);
            byte majorVersion = data[8];
            byte minorVersion = data[9];

            var licenseInfo = new LicenseInfo
            {
                Type = (LicenseType)licenseType,
                ProductVersion = $"{majorVersion}.{minorVersion}",
                Status = LicenseStatus.Active
            };

            if (expirationTimestamp > 0)
            {
                licenseInfo.ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(expirationTimestamp).UtcDateTime;
            }

            return licenseInfo;
        }

        private bool VerifySignature(string data, string signature)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(PUBLIC_KEY);

                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    byte[] signatureBytes = ConvertFromBase32(signature);

                    using (var sha256 = SHA256.Create())
                    {
                        return rsa.VerifyData(dataBytes, sha256, signatureBytes);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool IsVersionCompatible(string licenseVersion)
        {
            // For now, accept all versions
            // In production, check against current assembly version
            return true;
        }

        private byte[] ConvertFromBase32(string base32)
        {
            // Simplified Base32 decoder
            // In production, use proper Base32 implementation like base32-encoding NuGet package
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

            try
            {
                // Remove padding
                base32 = base32.TrimEnd('=');

                var bits = new StringBuilder();
                foreach (char c in base32)
                {
                    int value = alphabet.IndexOf(c);
                    if (value < 0)
                        throw new ArgumentException("Invalid Base32 character");

                    bits.Append(Convert.ToString(value, 2).PadLeft(5, '0'));
                }

                // Convert bits to bytes
                var bytes = new byte[bits.Length / 8];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(bits.ToString(i * 8, 8), 2);
                }

                return bytes;
            }
            catch
            {
                // Fallback for invalid Base32
                return Encoding.UTF8.GetBytes(base32);
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }
        public LicenseInfo LicenseInfo { get; private set; }

        private ValidationResult() { }

        public static ValidationResult Success(LicenseInfo licenseInfo)
        {
            return new ValidationResult
            {
                IsValid = true,
                LicenseInfo = licenseInfo
            };
        }

        public static ValidationResult Failure(string errorMessage, LicenseInfo licenseInfo = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                LicenseInfo = licenseInfo
            };
        }
    }
}
