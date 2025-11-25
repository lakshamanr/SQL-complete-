using System;
using System.Security.Cryptography;
using System.Text;

namespace SSMSSQLComplete.Tools.LicenseGenerator
{
    /// <summary>
    /// Command-line tool for generating hardware-bound license keys
    /// </summary>
    public class LicenseGeneratorTool
    {
        // IMPORTANT: Keep this private key SECURE! Never commit to source control!
        // This is a placeholder - generate your own RSA key pair for production
        private const string PRIVATE_KEY = @"<RSAKeyValue>
<Modulus>REPLACE_WITH_YOUR_PRIVATE_KEY_MODULUS</Modulus>
<Exponent>AQAB</Exponent>
<D>REPLACE_WITH_YOUR_PRIVATE_KEY_D_VALUE</D>
<P>REPLACE_WITH_P_VALUE</P>
<Q>REPLACE_WITH_Q_VALUE</Q>
<DP>REPLACE_WITH_DP_VALUE</DP>
<DQ>REPLACE_WITH_DQ_VALUE</DQ>
<InverseQ>REPLACE_WITH_INVERSEQ_VALUE</InverseQ>
</RSAKeyValue>";

        static void Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("SSMS SQL Complete - License Generation Tool");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (args.Length > 0 && args[0] == "generate-keys")
            {
                GenerateRSAKeyPair();
                return;
            }

            try
            {
                // Get license details
                Console.Write("License Type (1=Trial, 2=Perpetual, 3=Subscription): ");
                var licenseType = int.Parse(Console.ReadLine() ?? "2");

                DateTime? expirationDate = null;
                if (licenseType == 1 || licenseType == 3)
                {
                    Console.Write("Expiration Date (YYYY-MM-DD): ");
                    var dateStr = Console.ReadLine();
                    if (DateTime.TryParse(dateStr, out DateTime parsed))
                    {
                        expirationDate = parsed;
                    }
                }

                Console.Write("Product Version (e.g., 1.0): ");
                var versionStr = Console.ReadLine() ?? "1.0";
                var versionParts = versionStr.Split('.');
                var majorVersion = byte.Parse(versionParts[0]);
                var minorVersion = versionParts.Length > 1 ? byte.Parse(versionParts[1]) : (byte)0;

                Console.Write("Hardware Fingerprint (leave empty for non-bound): ");
                var hardwareFingerprint = Console.ReadLine();

                // Generate license key
                var licenseKey = GenerateLicenseKey(licenseType, expirationDate, majorVersion, minorVersion);

                Console.WriteLine();
                Console.WriteLine("==============================================");
                Console.WriteLine("LICENSE KEY GENERATED:");
                Console.WriteLine("==============================================");
                Console.WriteLine(licenseKey);
                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(hardwareFingerprint))
                {
                    var boundKey = CreateHardwareBoundKey(licenseKey, hardwareFingerprint);
                    Console.WriteLine("HARDWARE-BOUND KEY:");
                    Console.WriteLine(boundKey);
                    Console.WriteLine();
                }

                Console.WriteLine("License Details:");
                Console.WriteLine($"  Type: {GetLicenseTypeName(licenseType)}");
                Console.WriteLine($"  Version: {majorVersion}.{minorVersion}");
                if (expirationDate.HasValue)
                {
                    Console.WriteLine($"  Expires: {expirationDate.Value:yyyy-MM-dd}");
                }
                Console.WriteLine("==============================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void GenerateRSAKeyPair()
        {
            Console.WriteLine("Generating RSA-2048 key pair...");
            Console.WriteLine();

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                var privateKey = rsa.ToXmlString(true);
                var publicKey = rsa.ToXmlString(false);

                Console.WriteLine("PRIVATE KEY (Keep secure! Never share!):");
                Console.WriteLine("==========================================");
                Console.WriteLine(privateKey);
                Console.WriteLine();

                Console.WriteLine("PUBLIC KEY (Embed in application):");
                Console.WriteLine("==========================================");
                Console.WriteLine(publicKey);
                Console.WriteLine();

                Console.WriteLine("IMPORTANT:");
                Console.WriteLine("1. Save the PRIVATE KEY securely (use HSM in production)");
                Console.WriteLine("2. Update LicenseValidator.cs with the PUBLIC KEY");
                Console.WriteLine("3. Update this tool with the PRIVATE KEY");
                Console.WriteLine("4. Never commit private key to source control!");
            }
        }

        private static string GenerateLicenseKey(int licenseType, DateTime? expirationDate, byte majorVersion, byte minorVersion)
        {
            // Build license data (10 bytes)
            byte[] data = new byte[10];

            // Bytes 0-3: License type
            BitConverter.GetBytes(licenseType).CopyTo(data, 0);

            // Bytes 4-7: Expiration timestamp
            int expirationTimestamp = expirationDate.HasValue
                ? (int)((DateTimeOffset)expirationDate.Value).ToUnixTimeSeconds()
                : 0;
            BitConverter.GetBytes(expirationTimestamp).CopyTo(data, 4);

            // Bytes 8-9: Product version
            data[8] = majorVersion;
            data[9] = minorVersion;

            // Convert to Base32
            string dataSegment = ConvertToBase32(data);

            // Format as XXXXX-XXXXX-XXXXX-XXXXX
            string formattedData = $"{dataSegment.Substring(0, 5)}-{dataSegment.Substring(5, 5)}-" +
                                  $"{dataSegment.Substring(10, 5)}-{dataSegment.Substring(15, 5)}";

            // Sign with private key
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(PRIVATE_KEY);

                byte[] signatureBytes;
                using (var sha256 = SHA256.Create())
                {
                    byte[] dataBytes = Encoding.UTF8.GetBytes(formattedData);
                    signatureBytes = rsa.SignData(dataBytes, sha256);
                }

                string signatureBase32 = ConvertToBase32(signatureBytes);

                return $"SSMS-{formattedData}-{signatureBase32}";
            }
        }

        private static string CreateHardwareBoundKey(string baseLicenseKey, string hardwareFingerprint)
        {
            var bindingData = $"{baseLicenseKey}|{hardwareFingerprint}";

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(bindingData);
                var hash = sha256.ComputeHash(bytes);

                // Take first 16 bytes
                var shortHash = new byte[16];
                Array.Copy(hash, shortHash, 16);

                var bindingHash = BitConverter.ToString(shortHash).Replace("-", "");
                return $"{baseLicenseKey}:{bindingHash}";
            }
        }

        private static string ConvertToBase32(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();

            for (int i = 0; i < data.Length; i += 5)
            {
                int byteCount = Math.Min(5, data.Length - i);
                ulong buffer = 0;

                for (int j = 0; j < byteCount; j++)
                {
                    buffer = (buffer << 8) | data[i + j];
                }

                int bitCount = byteCount * 8;
                while (bitCount > 0)
                {
                    int index = (int)((buffer >> (bitCount - 5)) & 0x1F);
                    result.Append(alphabet[index]);
                    bitCount -= 5;
                }
            }

            return result.ToString();
        }

        private static string GetLicenseTypeName(int type)
        {
            return type switch
            {
                1 => "Trial",
                2 => "Perpetual",
                3 => "Subscription",
                _ => "Unknown"
            };
        }
    }
}
