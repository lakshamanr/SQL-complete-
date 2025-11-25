# SSMS SQL Complete - Licensing System Guide

## Overview

SSMS SQL Complete includes a comprehensive commercial licensing system with trial functionality, license key validation, and activation management.

## Table of Contents

1. [For End Users](#for-end-users)
2. [For Developers](#for-developers)
3. [License Types](#license-types)
4. [Architecture](#architecture)
5. [Security](#security)
6. [Testing](#testing)

---

## For End Users

### Trial Period

- **Duration**: 30 days
- **Auto-start**: Trial begins automatically on first use
- **One-time only**: Trial can only be started once per machine
- **Full features**: All features available during trial
- **Notifications**: Warning when 7 days or less remain

### Activating a License

1. Open SSMS with the add-in installed
2. Go to **Tools → SSMS SQL Complete → Activate License**
3. Enter your license key in the format: `SSMS-XXXXX-XXXXX-XXXXX-XXXXX-SIGNATURE`
4. (Optional) Enter the name of the licensee
5. Click **Activate**

### Checking License Status

- Go to **Tools → SSMS SQL Complete → About**
- View current license type, status, and expiration (if applicable)

### Machine ID

- Required for activation on license server systems
- Unique identifier based on hardware characteristics
- View in activation dialog or contact support for assistance

---

## For Developers

### License Key Generation

**IMPORTANT**: The current implementation uses a placeholder RSA public key. You MUST replace this with your own key pair.

#### Step 1: Generate RSA Key Pair

```csharp
using System.Security.Cryptography;

var rsa = new RSACryptoServiceProvider(2048);

// Private key (keep secure on license server)
string privateKey = rsa.ToXmlString(true);

// Public key (embed in application)
string publicKey = rsa.ToXmlString(false);
```

#### Step 2: Update LicenseValidator.cs

Replace the `PUBLIC_KEY` constant in `LicenseValidator.cs:14` with your generated public key.

#### Step 3: Create License Generation Server

```csharp
public string GenerateLicenseKey(
    LicenseType type,
    DateTime? expirationDate,
    string majorVersion,
    string minorVersion)
{
    // Encode license data to Base32
    byte[] data = new byte[10];
    BitConverter.GetBytes((int)type).CopyTo(data, 0);

    int expirationTimestamp = expirationDate.HasValue
        ? (int)((DateTimeOffset)expirationDate.Value).ToUnixTimeSeconds()
        : 0;
    BitConverter.GetBytes(expirationTimestamp).CopyTo(data, 4);

    data[8] = byte.Parse(majorVersion);
    data[9] = byte.Parse(minorVersion);

    string dataSegment = ConvertToBase32(data);
    dataSegment = $"{dataSegment.Substring(0,5)}-{dataSegment.Substring(5,5)}-" +
                  $"{dataSegment.Substring(10,5)}-{dataSegment.Substring(15,5)}";

    // Sign with private key
    using (var rsa = new RSACryptoServiceProvider())
    {
        rsa.FromXmlString(PRIVATE_KEY);
        byte[] signature = rsa.SignData(
            Encoding.UTF8.GetBytes(dataSegment),
            SHA256.Create());

        string signatureBase32 = ConvertToBase32(signature);

        return $"SSMS-{dataSegment}-{signatureBase32}";
    }
}
```

### License Types

The system supports three license types:

#### 1. Trial (Type = 1)
- 30-day evaluation period
- Auto-starts on first use
- Tracked in Windows Registry
- Full feature access

#### 2. Perpetual (Type = 2)
- No expiration date
- One-time purchase
- Lifetime validity
- Set expirationDate = null or 0

#### 3. Subscription (Type = 3)
- Annual or monthly billing
- Expires after subscription period
- Requires renewal
- Set expirationDate to subscription end

### Storage

License data is stored in Windows Registry:
- **Location**: `HKEY_CURRENT_USER\Software\SSMSSQLComplete`
- **License Key**: Encrypted license key string
- **Activation Date**: ISO 8601 timestamp
- **Machine ID**: Hardware-based hash
- **Trial Start**: Trial start timestamp (if applicable)
- **Trial Days Used**: Number of days trial has been active

### API Usage

#### Check Current License

```csharp
using SSMSSQLComplete.Core.Licensing;

var license = LicenseManager.Instance.GetCurrentLicense();

if (license.IsValid())
{
    Console.WriteLine($"Valid license: {license.Type}");
}
else
{
    Console.WriteLine($"Invalid license: {license.Status}");
}
```

#### Activate License

```csharp
var result = LicenseManager.Instance.ActivateLicense(
    licenseKey: "SSMS-XXXXX-XXXXX-XXXXX-XXXXX-SIG",
    licensedTo: "John Doe",
    companyName: "Acme Corp");

if (result.IsSuccessful)
{
    Console.WriteLine("License activated!");
}
else
{
    Console.WriteLine($"Activation failed: {result.ErrorMessage}");
}
```

#### Deactivate License

```csharp
LicenseManager.Instance.DeactivateLicense();
```

#### Check Trial Status

```csharp
var trialInfo = TrialManager.Instance.GetTrialInfo();

Console.WriteLine($"Trial started: {trialInfo.IsTrialStarted}");
Console.WriteLine($"Days remaining: {trialInfo.DaysRemaining}");
Console.WriteLine($"Expired: {trialInfo.IsExpired}");
```

---

## Architecture

### Components

#### LicenseInfo.cs
- Data model representing license details
- Validation logic for expiration
- Display status formatting

#### LicenseValidator.cs
- RSA signature verification
- License key format validation
- Base32 decoding
- Expiration checking

#### LicenseManager.cs
- Singleton manager for license operations
- Registry storage/retrieval
- Activation/deactivation
- Machine ID generation

#### TrialManager.cs
- Trial period tracking
- 30-day countdown
- Registry-based persistence
- One-time trial enforcement

#### UI Components
- `LicenseActivationDialog.xaml`: WPF activation dialog
- `ActivateLicenseCommand.cs`: Menu command for activation
- `AboutCommand.cs`: License status display

### Integration

The licensing system integrates with the package initialization:

1. **Package Load** (`SSMSSQLCompletePackage.InitializeAsync`)
   - Check current license status
   - Auto-start trial if unlicensed
   - Show expiration warnings
   - Log license status

2. **Menu Integration**
   - Tools → SSMS SQL Complete → Activate License
   - Tools → SSMS SQL Complete → About

3. **Feature Gating** (Optional)
   - Check `LicenseManager.Instance.IsLicensed()` before premium features
   - Display trial messages for unlicensed users

---

## Security

### Best Practices

1. **Private Key Security**
   - NEVER commit private key to source control
   - Store on secure license server only
   - Use HSM (Hardware Security Module) for production

2. **License Key Complexity**
   - Current format: `SSMS-XXXXX-XXXXX-XXXXX-XXXXX-SIGNATURE`
   - 20-character data segment (Base32)
   - Variable-length RSA signature (Base32)
   - Total length: ~80 characters

3. **Anti-Tampering**
   - Registry values can be deleted by users
   - Consider additional obfuscation for production
   - Implement online activation for stricter enforcement

4. **Machine ID**
   - Based on machine name, processor count, OS version
   - SHA256 hash for privacy
   - Can be spoofed - consider additional hardware fingerprinting

### Vulnerabilities

⚠️ **Known Limitations**:
- Registry can be manually edited
- Machine ID can be cloned
- No online verification (offline mode)
- Public key visible in assembly

🔒 **Production Recommendations**:
- Implement online activation server
- Add code obfuscation (e.g., Dotfuscator)
- Use native code for critical validation
- Implement license heartbeat checks
- Add tamper detection

---

## Testing

### Manual Testing

#### Test Trial Period

```batch
# Clear existing license data
reg delete HKCU\Software\SSMSSQLComplete /f

# Launch SSMS - trial should auto-start
# Verify: Tools → About shows "Trial (30 days remaining)"
```

#### Test License Activation

```batch
# Generate test license key using license server
# Format: SSMS-XXXXX-XXXXX-XXXXX-XXXXX-SIG

# In SSMS:
# 1. Tools → Activate License
# 2. Enter test key
# 3. Verify activation success
# 4. Tools → About should show "Active (Perpetual)"
```

#### Test Expiration

```csharp
// Create expired license key (expirationDate = yesterday)
// Activate license
// Restart SSMS
// Verify: Shows "Expired" status
```

### Automated Testing

Create unit tests for licensing components:

```csharp
[TestClass]
public class LicenseValidatorTests
{
    [TestMethod]
    public void ValidateLicenseKey_ValidPerpetual_ReturnsSuccess()
    {
        var validator = new LicenseValidator();
        var key = "SSMS-XXXXX-XXXXX-XXXXX-XXXXX-VALIDSIGINATURE";

        var result = validator.ValidateLicenseKey(key);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(LicenseType.Perpetual, result.LicenseInfo.Type);
    }

    [TestMethod]
    public void ValidateLicenseKey_InvalidFormat_ReturnsFailure()
    {
        var validator = new LicenseValidator();
        var key = "INVALID-KEY";

        var result = validator.ValidateLicenseKey(key);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ErrorMessage.Contains("Invalid license key format"));
    }
}
```

---

## FAQ

### Q: Can users bypass the licensing system?

**A**: Yes, with sufficient technical knowledge. The current implementation provides basic protection suitable for honest users. For stricter enforcement, implement online activation and code obfuscation.

### Q: How do I reset the trial period?

**A**: Delete the registry key: `HKCU\Software\SSMSSQLComplete`. Note: This is intentional for testing. Users can also do this.

### Q: Can I implement floating licenses?

**A**: Yes, but requires significant changes:
1. Add license server with seat management
2. Implement heartbeat checks (every 5-15 minutes)
3. Release seat on application close
4. Handle network failures gracefully

### Q: How do I handle offline activation?

**A**: The current system is offline-capable. For stricter offline activation:
1. User provides Machine ID to vendor
2. Vendor generates activation code tied to Machine ID
3. User enters activation code in dialog
4. System validates Machine ID match

### Q: What happens when trial expires?

**A**: On package initialization, if trial is expired:
1. User sees notification dialog
2. Can choose to activate license
3. Features can be disabled (optional - requires feature gating implementation)

---

## Support

For licensing issues:
1. Check Windows Event Viewer for error logs
2. Verify registry entries in `HKCU\Software\SSMSSQLComplete`
3. Check SSMS extension logs
4. Contact support with Machine ID for activation assistance

---

## Changelog

### Version 1.0.0
- Initial licensing system implementation
- Trial period (30 days)
- RSA signature validation
- WPF activation dialog
- Registry-based storage
- Perpetual and subscription license types
- Machine ID generation
- Auto-trial start
- Expiration notifications

---

## License

This licensing system is part of SSMS SQL Complete.
© 2024 All Rights Reserved
