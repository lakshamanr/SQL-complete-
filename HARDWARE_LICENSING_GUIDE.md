# Hardware-Based Licensing System - Technical Documentation

## Overview

This document describes the enhanced hardware-based licensing system for SSMS SQL Complete, which provides robust protection against unauthorized license transfers, tampering, and abuse.

---

## Table of Contents

1. [Architecture](#architecture)
2. [Components](#components)
3. [Hardware Fingerprinting](#hardware-fingerprinting)
4. [License Binding](#license-binding)
5. [Tamper Detection](#tamper-detection)
6. [License Consumption Tracking](#license-consumption-tracking)
7. [License Generation](#license-generation)
8. [Security Features](#security-features)
9. [Anti-Hack Measures](#anti-hack-measures)
10. [Implementation Guide](#implementation-guide)

---

## Architecture

The hardware-based licensing system consists of five core components:

```
┌─────────────────────────────────────────────────────────────┐
│                    LicenseManager                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Hardware   │  │   License    │  │   Tamper     │     │
│  │ Fingerprint  │  │   Binding    │  │  Detection   │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│  ┌──────────────┐  ┌──────────────┐                       │
│  │   License    │  │License       │                       │
│  │ Consumption  │  │ Validator    │                       │
│  └──────────────┘  └──────────────┘                       │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
                ┌───────────────────────┐
                │  License Generator    │
                │  (Separate Tool)      │
                └───────────────────────┘
```

### Data Flow

1. **License Generation** (Server-side):
   ```
   Hardware Fingerprint → License Generator → Base License Key
                                            ↓
                           Hardware Binding → Bound License Key
   ```

2. **License Activation** (Client-side):
   ```
   Bound License Key → Extract Base Key → Validate Signature
                                        ↓
              Validate Hardware Binding → Store License
                                        ↓
                    Record Consumption → Update Tamper Checksum
   ```

3. **License Validation** (Runtime):
   ```
   Tamper Detection → Hardware Validation → Consumption Tracking
                                          ↓
                               License Valid / Invalid
   ```

---

## Components

### 1. HardwareFingerprint.cs

**Purpose**: Generates a unique, stable hardware identifier based on multiple hardware components.

**Hardware Components Used**:
- Processor ID (CPU serial number)
- Motherboard Serial Number
- BIOS Serial Number
- MAC Address (first network adapter)
- Physical Disk Serial Number
- System UUID

**Key Methods**:
```csharp
string Generate()                    // Generates full hardware fingerprint (SHA256 hash)
bool Validate(string fingerprint)    // Validates against expected fingerprint
string GetShortId()                  // Returns shortened ID for display (16 chars)
void ClearCache()                    // Clears cached fingerprint (for testing)
```

**Implementation**:
```csharp
var fingerprint = HardwareFingerprint.Instance.Generate();
// Returns: "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6"
```

**Stability**: The fingerprint remains stable across:
- Software updates
- Operating system updates
- Minor hardware changes (adding RAM, etc.)

**Uniqueness**: Each machine has a unique fingerprint based on immutable hardware IDs.

### 2. LicenseBinding.cs

**Purpose**: Binds license keys to specific hardware to prevent unauthorized transfers.

**Binding Format**:
```
BASE_LICENSE_KEY:BINDING_HASH

Example:
SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123...ABC:1234567890ABCDEF
│                                      │
│                                      └─ Hardware Binding Hash (32 chars)
└─ Base License Key (validated by RSA)
```

**Key Methods**:
```csharp
string CreateBoundLicenseKey(string baseLicenseKey, string hardwareFingerprint)
bool ValidateBinding(string boundLicenseKey, string expectedHardwareFingerprint)
string ExtractBaseLicenseKey(string boundLicenseKey)
bool IsBound(string licenseKey)
```

**Usage Example**:
```csharp
// Server-side: Bind license to hardware
var hardwareId = "A1B2C3D4E5F6G7H8..."; // From customer
var baseLicense = "SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123...";
var boundLicense = LicenseBinding.CreateBoundLicenseKey(baseLicense, hardwareId);

// Client-side: Validate binding
var currentHardware = HardwareFingerprint.Instance.Generate();
bool isValid = LicenseBinding.ValidateBinding(boundLicense, currentHardware);
```

### 3. TamperDetection.cs

**Purpose**: Detects tampering with license data, registry entries, and application binaries.

**Detection Methods**:
1. **Registry Integrity**: Checksum of license data in registry
2. **Assembly Integrity**: Strong name verification
3. **Time Manipulation**: Detects system clock rollback
4. **Debugger Detection**: Checks for attached debuggers

**Key Methods**:
```csharp
TamperDetectionResult DetectTampering()
void UpdateChecksum(string licenseKey, string activationDate, string machineId)
```

**TamperDetectionResult**:
```csharp
class TamperDetectionResult
{
    bool IsTampered { get; set; }
    string TamperType { get; set; }
    bool IsDebuggerAttached { get; set; }
    DateTime? LastCheckTime { get; set; }
}
```

**Protection Mechanisms**:
- SHA256 checksum of registry data
- Timestamp tracking to detect time manipulation
- Last verification time tracking
- Automatic checksum updates on legitimate changes

### 4. LicenseConsumption.cs

**Purpose**: Tracks license usage patterns to detect and prevent abuse.

**Tracked Metrics**:
- Activation count
- Deactivation count
- Total uses
- First activation date
- Last use date

**Abuse Detection Patterns**:
1. **Excessive Reactivations**: More than 10 activations
2. **Rapid Cycling**: High deactivation/activation ratio (>80%)
3. **Unusual Patterns**: Detects license sharing attempts

**Key Methods**:
```csharp
void RecordActivation()
void RecordDeactivation()
void RecordUsage()
ConsumptionStats GetStats()
bool DetectAbusePattern()
```

**ConsumptionStats**:
```csharp
class ConsumptionStats
{
    int ActivationCount { get; set; }
    int DeactivationCount { get; set; }
    int TotalUses { get; set; }
    DateTime? FirstActivation { get; set; }
    DateTime? LastUse { get; set; }
    TimeSpan GetUsageDuration()
}
```

### 5. LicenseGeneratorTool.cs

**Purpose**: Command-line tool for generating hardware-bound license keys.

**Features**:
- RSA key pair generation
- License key generation (Trial, Perpetual, Subscription)
- Hardware binding support
- Base32 encoding
- RSA-2048 signature generation

**Usage**:
```bash
# Generate RSA key pair
LicenseGenerator.exe generate-keys

# Generate license key (interactive)
LicenseGenerator.exe

# Output:
# License Type (1=Trial, 2=Perpetual, 3=Subscription): 2
# Expiration Date (YYYY-MM-DD): [skip for perpetual]
# Product Version (e.g., 1.0): 1.0
# Hardware Fingerprint (leave empty for non-bound): A1B2C3D4E5F6G7H8...
#
# LICENSE KEY GENERATED:
# SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123456789ABCDEF
#
# HARDWARE-BOUND KEY:
# SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123456789ABCDEF:1234567890ABCDEF
```

---

## Hardware Fingerprinting

### Technical Implementation

The hardware fingerprint is generated using WMI (Windows Management Instrumentation):

```csharp
using System.Management;

// Processor ID
var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
var processor = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
var processorId = processor?["ProcessorId"]?.ToString();

// Motherboard Serial
searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
// ... similar pattern for other components
```

### Combining Components

All hardware IDs are concatenated and hashed:

```csharp
var components = new List<string>
{
    processorId,
    motherboardSerial,
    biosSerial,
    macAddress,
    diskSerial,
    systemUuid
};

var combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));
var fingerprint = SHA256.ComputeHash(Encoding.UTF8.GetBytes(combined));
return BitConverter.ToString(fingerprint).Replace("-", "");
```

### Fallback Strategy

If WMI queries fail (insufficient permissions, VMs, etc.):

```csharp
var fallback = $"{Environment.MachineName}|{Environment.ProcessorCount}|{Environment.OSVersion}";
return SHA256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
```

### Stability Analysis

**Stable across**:
- Software installations/uninstalls
- OS updates and patches
- Driver updates
- Adding/removing peripherals
- Network changes (uses first adapter only)

**Changes when**:
- Motherboard replacement
- CPU replacement
- BIOS reflash (sometimes)
- Disk replacement (primary disk)
- Major hardware upgrade

**Recommendation**: Allow 1-2 hardware transfers per year for legitimate upgrades.

---

## License Binding

### Binding Process

1. **Customer requests license**:
   - Provides hardware fingerprint from activation dialog
   - Example: `A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6`

2. **Server generates base license key**:
   ```csharp
   var baseLicense = LicenseGeneratorTool.GenerateLicenseKey(
       licenseType: 2,  // Perpetual
       expirationDate: null,
       majorVersion: 1,
       minorVersion: 0
   );
   // Result: SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123456789ABCDEF
   ```

3. **Server binds to hardware**:
   ```csharp
   var boundLicense = LicenseBinding.CreateBoundLicenseKey(baseLicense, hardwareFingerprint);
   // Result: SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123456789ABCDEF:32CHARSBINDINGHASH
   ```

4. **Customer activates**:
   - Enters bound license key
   - System extracts base key
   - Validates RSA signature
   - Validates hardware binding
   - Stores if valid

### Binding Validation

```csharp
// Extract base license key
var baseKey = LicenseBinding.ExtractBaseLicenseKey(boundLicenseKey);

// Validate RSA signature on base key
var validationResult = validator.ValidateLicenseKey(baseKey);
if (!validationResult.IsValid)
    return ActivationResult.Failure("Invalid license key");

// Validate hardware binding
var currentHardware = HardwareFingerprint.Instance.Generate();
if (!LicenseBinding.ValidateBinding(boundLicenseKey, currentHardware))
    return ActivationResult.Failure("Hardware mismatch");
```

### License Transfer

To transfer a hardware-bound license to new hardware:

1. **Option A: Deactivate and Reactivate**:
   - Customer deactivates on old machine
   - Provides new hardware fingerprint
   - Server generates new bound key
   - Customer activates on new machine

2. **Option B: Online Transfer**:
   - Server tracks active installations
   - Allows transfer if within limit (e.g., 2 per year)
   - Automatically deactivates old hardware
   - Generates new binding

---

## Tamper Detection

### Registry Checksum

When license is stored, a checksum is calculated:

```csharp
var data = $"{licenseKey}|{activationDate}|{machineId}";
var checksum = SHA256.ComputeHash(Encoding.UTF8.GetBytes(data));
Registry.SetValue("Checksum", BitConverter.ToString(checksum));
```

On each validation:

```csharp
var currentChecksum = ComputeChecksum(currentData);
var storedChecksum = Registry.GetValue("Checksum");

if (currentChecksum != storedChecksum)
{
    // Registry tampered!
    return new TamperDetectionResult
    {
        IsTampered = true,
        TamperType = "Registry modification detected"
    };
}
```

### Time Manipulation Detection

Track last verification time:

```csharp
var lastVerified = DateTime.Parse(Registry.GetValue("LastVerified"));
var now = DateTime.UtcNow;

// Check if clock went backwards (more than 5 minutes tolerance)
if (now < lastVerified.AddMinutes(-5))
{
    return new TamperDetectionResult
    {
        IsTampered = true,
        TamperType = "System time manipulation detected"
    };
}

// Update last verified time
Registry.SetValue("LastVerified", now.ToString("O"));
```

### Assembly Integrity

Check strong name signature:

```csharp
var assembly = Assembly.GetExecutingAssembly();
var publicKeyToken = assembly.GetName().GetPublicKeyToken();

if (publicKeyToken == null || publicKeyToken.Length == 0)
{
    Logger.Warn("Assembly is not strongly named");
}

// In production, verify exact public key token matches expected value
var expectedToken = new byte[] { 0xAB, 0xCD, 0xEF, ... };
if (!publicKeyToken.SequenceEqual(expectedToken))
{
    return new TamperDetectionResult
    {
        IsTampered = true,
        TamperType = "Assembly signature invalid"
    };
}
```

---

## License Consumption Tracking

### Metrics Collection

Every interaction with the licensing system is tracked:

```csharp
// On activation
LicenseConsumption.Instance.RecordActivation();
Registry.SetValue("ActivationCount", count + 1);
Registry.SetValue("FirstActivation", DateTime.UtcNow);

// On deactivation
LicenseConsumption.Instance.RecordDeactivation();
Registry.SetValue("DeactivationCount", count + 1);

// On each use (package load)
LicenseConsumption.Instance.RecordUsage();
Registry.SetValue("LastUse", DateTime.UtcNow);
Registry.SetValue("TotalUses", uses + 1);
```

### Abuse Pattern Detection

```csharp
public bool DetectAbusePattern()
{
    var stats = GetStats();

    // Pattern 1: Excessive reactivations
    if (stats.ActivationCount > 10)
    {
        Logger.Warn($"Abuse pattern: {stats.ActivationCount} activations");
        return true;
    }

    // Pattern 2: High deactivation ratio (license sharing)
    if (stats.ActivationCount > 3 && stats.DeactivationCount > 3)
    {
        var ratio = (double)stats.DeactivationCount / stats.ActivationCount;
        if (ratio > 0.8) // More than 80% deactivation rate
        {
            Logger.Warn("Abuse pattern: Rapid activation/deactivation");
            return true;
        }
    }

    return false;
}
```

### Response to Abuse

```csharp
if (LicenseConsumption.Instance.DetectAbusePattern())
{
    return ActivationResult.Failure(
        "Too many activation attempts. Please contact support."
    );
}
```

---

## License Generation

### Server-Side Tool

The `LicenseGeneratorTool.cs` is a command-line application that runs on the license server (NOT distributed to customers).

### Key Generation Process

1. **Encode License Data** (10 bytes):
   ```
   Bytes 0-3: License Type (Int32)
   Bytes 4-7: Expiration Timestamp (Unix epoch, Int32)
   Bytes 8-9: Product Version (Major.Minor)
   ```

2. **Convert to Base32**:
   ```csharp
   string dataSegment = ConvertToBase32(data);
   // Result: "ABCDEFGHIJKLMNOPQRST" (20 characters)
   ```

3. **Format Data Segment**:
   ```csharp
   string formatted = $"{dataSegment.Substring(0,5)}-" +
                      $"{dataSegment.Substring(5,5)}-" +
                      $"{dataSegment.Substring(10,5)}-" +
                      $"{dataSegment.Substring(15,5)}";
   // Result: "ABCDE-FGHIJ-KLMNO-PQRST"
   ```

4. **Sign with Private Key**:
   ```csharp
   using (var rsa = new RSACryptoServiceProvider())
   {
       rsa.FromXmlString(PRIVATE_KEY);
       var signature = rsa.SignData(
           Encoding.UTF8.GetBytes(formatted),
           SHA256.Create()
       );
       var signatureBase32 = ConvertToBase32(signature);

       return $"SSMS-{formatted}-{signatureBase32}";
   }
   // Result: "SSMS-ABCDE-FGHIJ-KLMNO-PQRST-SIG123456789ABCDEFGHIJKLMNOPQRS..."
   ```

5. **Optionally Bind to Hardware**:
   ```csharp
   if (!string.IsNullOrEmpty(hardwareFingerprint))
   {
       return LicenseBinding.CreateBoundLicenseKey(licenseKey, hardwareFingerprint);
   }
   ```

---

## Security Features

### 1. RSA-2048 Signature

- **Purpose**: Prevents license key forgery
- **Algorithm**: RSA with SHA256
- **Key Size**: 2048 bits (secure until 2030+)
- **Private Key**: Kept on secure license server
- **Public Key**: Embedded in application

### 2. Hardware Binding

- **Purpose**: Prevents unauthorized license transfers
- **Binding**: SHA256 hash of (license key + hardware fingerprint)
- **Validation**: Re-compute hash and compare
- **Transfer**: Requires server-side action

### 3. Tamper Detection

- **Registry Checksum**: SHA256 hash of all license data
- **Time Tracking**: Detects clock manipulation
- **Assembly Verification**: Strong name signature check

### 4. Consumption Tracking

- **Activation Limits**: Configurable (default: 10)
- **Deactivation Ratio**: Detects license sharing (>80%)
- **Usage Metrics**: Tracks frequency and patterns

### 5. Multi-Component Fingerprint

- **Components**: 6 hardware identifiers
- **Hashing**: SHA256 for privacy
- **Fallback**: Basic fingerprint if WMI fails

---

## Anti-Hack Measures

### Protection Against Common Attacks

#### 1. Registry Modification
**Attack**: User edits registry to extend trial or change license
**Protection**:
- Checksum validation detects any modification
- Tamper detection invalidates license on mismatch
- Encrypted storage (optional enhancement)

#### 2. Clock Manipulation
**Attack**: User rolls back system clock to extend trial
**Protection**:
- Last verification timestamp tracking
- Rejects time going backwards (>5 min tolerance)
- Trial period uses UTC time

#### 3. Hardware Cloning
**Attack**: User clones hardware fingerprint to another machine
**Protection**:
- Hardware binding validation
- Multiple hardware components required
- WMI queries (hard to spoof)

#### 4. License Sharing
**Attack**: Multiple users share same license key
**Protection**:
- Hardware binding (one key = one machine)
- Consumption tracking (detects rapid cycling)
- Abuse pattern detection (>80% deactivation ratio)

#### 5. Binary Patching
**Attack**: User modifies DLL to bypass license checks
**Protection**:
- Strong name assembly signing
- Checksum verification (optional)
- Code obfuscation (production recommendation)

#### 6. Debugger Bypass
**Attack**: User debugs application to skip license checks
**Protection**:
- Debugger detection (`Debugger.IsAttached`)
- Anti-debug techniques (optional)
- Logging debugger attempts

### Recommendations for Production

1. **Code Obfuscation**:
   - Use Dotfuscator or ConfuserEx
   - Obfuscate licensing namespace
   - Rename methods and variables

2. **Online Activation**:
   - Implement license server verification
   - Track active installations
   - Limit concurrent activations

3. **Encrypted Storage**:
   - Encrypt license data in registry
   - Use DPAPI for Windows integration
   - Salt with machine-specific data

4. **Heartbeat Validation**:
   - Periodic license revalidation (every 7-30 days)
   - Online check for subscription licenses
   - Grace period for offline users

5. **Logging and Monitoring**:
   - Log all license events
   - Send telemetry to server
   - Alert on suspicious patterns

---

## Implementation Guide

### Step 1: Generate RSA Key Pair

```bash
cd tools/LicenseGenerator
LicenseGenerator.exe generate-keys

# Output:
# PRIVATE KEY (Keep secure! Never share!):
# <RSAKeyValue><Modulus>...</Modulus>...</RSAKeyValue>
#
# PUBLIC KEY (Embed in application):
# <RSAKeyValue><Modulus>...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>
```

### Step 2: Update Application with Public Key

Edit `LicenseValidator.cs`:

```csharp
private const string PUBLIC_KEY = @"<RSAKeyValue>
<Modulus>YOUR_PUBLIC_KEY_MODULUS_HERE</Modulus>
<Exponent>AQAB</Exponent>
</RSAKeyValue>";
```

### Step 3: Secure Private Key

Store private key securely (NOT in source control):
- Use Azure Key Vault
- Use AWS Secrets Manager
- Use Hardware Security Module (HSM)
- Or encrypted file on secure server

### Step 4: Update License Generator Tool

Edit `LicenseGeneratorTool.cs`:

```csharp
private const string PRIVATE_KEY = @"<RSAKeyValue>
<Modulus>YOUR_PRIVATE_KEY_MODULUS_HERE</Modulus>
<Exponent>AQAB</Exponent>
<D>YOUR_D_VALUE_HERE</D>
<P>YOUR_P_VALUE_HERE</P>
<Q>YOUR_Q_VALUE_HERE</Q>
<DP>YOUR_DP_VALUE_HERE</DP>
<DQ>YOUR_DQ_VALUE_HERE</DQ>
<InverseQ>YOUR_INVERSEQ_VALUE_HERE</InverseQ>
</RSAKeyValue>";
```

### Step 5: Generate Test License

```bash
LicenseGenerator.exe

# Input:
# License Type: 2 (Perpetual)
# Expiration Date: [skip]
# Product Version: 1.0
# Hardware Fingerprint: [leave empty for testing]

# Output:
# SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ
```

### Step 6: Test Activation

In your application:

```csharp
var result = LicenseManager.Instance.ActivateLicense(
    licenseKey: "SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123...",
    licensedTo: "Test User",
    companyName: "Test Company"
);

if (result.IsSuccessful)
{
    Console.WriteLine($"Activated: {result.LicenseInfo.Type}");
}
else
{
    Console.WriteLine($"Failed: {result.ErrorMessage}");
}
```

### Step 7: Enable Hardware Binding

For production licenses:

```bash
# Customer provides hardware fingerprint
# (from activation dialog or support email)
var hardwareId = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6";

# Generate bound license
LicenseGenerator.exe
# Enter hardware fingerprint when prompted

# Output:
# SSMS-A1B2C-D3E4F-G5H6I-J7K8L-SIG123...:1234567890ABCDEF
```

---

## Testing

### Unit Tests

Create tests for each component:

```csharp
[TestClass]
public class HardwareFingerprintTests
{
    [TestMethod]
    public void Generate_ReturnsConsistentFingerprint()
    {
        var fp1 = HardwareFingerprint.Instance.Generate();
        var fp2 = HardwareFingerprint.Instance.Generate();

        Assert.AreEqual(fp1, fp2);
        Assert.AreEqual(64, fp1.Length); // SHA256 hex string
    }

    [TestMethod]
    public void Validate_WithMatchingFingerprint_ReturnsTrue()
    {
        var fingerprint = HardwareFingerprint.Instance.Generate();
        var isValid = HardwareFingerprint.Instance.Validate(fingerprint);

        Assert.IsTrue(isValid);
    }
}

[TestClass]
public class LicenseBindingTests
{
    [TestMethod]
    public void CreateBoundLicenseKey_ProducesValidFormat()
    {
        var baseKey = "SSMS-ABCDE-FGHIJ-KLMNO-PQRST-SIG123...";
        var hardware = "A1B2C3D4E5F6G7H8...";

        var boundKey = LicenseBinding.CreateBoundLicenseKey(baseKey, hardware);

        Assert.IsTrue(boundKey.Contains(":"));
        Assert.AreEqual(baseKey, LicenseBinding.ExtractBaseLicenseKey(boundKey));
    }

    [TestMethod]
    public void ValidateBinding_WithCorrectHardware_ReturnsTrue()
    {
        var baseKey = "SSMS-ABCDE-FGHIJ-KLMNO-PQRST-SIG123...";
        var hardware = HardwareFingerprint.Instance.Generate();

        var boundKey = LicenseBinding.CreateBoundLicenseKey(baseKey, hardware);
        var isValid = LicenseBinding.ValidateBinding(boundKey, hardware);

        Assert.IsTrue(isValid);
    }
}
```

### Integration Tests

Test full activation flow:

```csharp
[TestClass]
public class LicensingIntegrationTests
{
    [TestMethod]
    public void ActivateHardwareBoundLicense_Success()
    {
        // Generate hardware-bound license
        var hardware = HardwareFingerprint.Instance.Generate();
        var baseKey = GenerateTestLicense();
        var boundKey = LicenseBinding.CreateBoundLicenseKey(baseKey, hardware);

        // Activate
        var result = LicenseManager.Instance.ActivateLicense(boundKey, "Test User");

        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(LicenseType.Perpetual, result.LicenseInfo.Type);
    }

    [TestMethod]
    public void ActivateHardwareBoundLicense_WrongHardware_Fails()
    {
        // Generate license bound to different hardware
        var otherHardware = "DIFFERENT_HARDWARE_ID";
        var baseKey = GenerateTestLicense();
        var boundKey = LicenseBinding.CreateBoundLicenseKey(baseKey, otherHardware);

        // Try to activate on current machine
        var result = LicenseManager.Instance.ActivateLicense(boundKey, "Test User");

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsTrue(result.ErrorMessage.Contains("hardware"));
    }
}
```

---

## FAQ

### Q: Can users bypass hardware binding by running in a VM?
**A**: VMs have their own hardware IDs (virtual motherboard, CPU, etc.), so the binding still works. However, VM snapshots can be used to clone the hardware. For stricter control, implement online activation with concurrent installation limits.

### Q: What happens if a user upgrades their hardware?
**A**: The hardware fingerprint will change. Provide a support process for license transfers:
1. Offer automated transfer (limit: 2 per year)
2. Or manual transfer through support ticket
3. Track transfer history to prevent abuse

### Q: How do I handle abuse patterns?
**A**: The system automatically detects:
- Excessive reactivations (>10)
- Rapid activation/deactivation cycles (>80% deactivation ratio)

Response options:
- Block activation with message to contact support
- Allow but flag account for review
- Implement rate limiting (1 activation per 24 hours)

### Q: Is the hardware fingerprint stable across Windows reinstalls?
**A**: Yes, because it's based on hardware IDs (CPU, motherboard, etc.), not Windows installation. The fingerprint survives:
- Windows reinstalls
- OS upgrades (Windows 10 → 11)
- Software changes

### Q: How secure is the tamper detection?
**A**: Current implementation provides:
- ✅ Basic protection against registry editing
- ✅ Time manipulation detection
- ✅ Debugger detection

For production, add:
- Code obfuscation (hides licensing logic)
- Encrypted registry storage (DPAPI)
- Anti-tampering runtime checks

### Q: Can I use this for SaaS/subscription licenses?
**A**: Yes, set `LicenseType.Subscription` with expiration date. For SaaS:
1. Implement periodic license revalidation (every 7-30 days)
2. Add online check against license server
3. Allow grace period for offline users (7-14 days)
4. Automatic renewal on payment

---

## Conclusion

The hardware-based licensing system provides robust protection against:
- ✅ Unauthorized license transfers
- ✅ License key sharing
- ✅ Trial period manipulation
- ✅ Registry tampering
- ✅ Excessive reactivations

**Security Level**: Medium-High (suitable for commercial software)

**For Maximum Security**, implement:
1. Online activation server
2. Code obfuscation
3. Encrypted storage
4. Heartbeat validation
5. Concurrent installation limits

---

**Document Version**: 1.0
**Last Updated**: 2024
**License**: Proprietary
