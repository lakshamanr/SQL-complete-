using System;

namespace SSMSSQLComplete.Core.Licensing
{
    /// <summary>
    /// Represents license information for the add-in
    /// </summary>
    public class LicenseInfo
    {
        public string LicenseKey { get; set; }
        public string LicensedTo { get; set; }
        public string CompanyName { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public LicenseType Type { get; set; }
        public LicenseStatus Status { get; set; }
        public int MaxActivations { get; set; }
        public int CurrentActivations { get; set; }
        public string ProductVersion { get; set; }
        public DateTime? ActivationDate { get; set; }
        public string MachineId { get; set; }

        public bool IsValid()
        {
            if (Status != LicenseStatus.Active)
                return false;

            if (Type == LicenseType.Trial)
            {
                if (!ExpirationDate.HasValue)
                    return false;

                return DateTime.UtcNow <= ExpirationDate.Value;
            }

            if (Type == LicenseType.Subscription)
            {
                if (!ExpirationDate.HasValue)
                    return false;

                return DateTime.UtcNow <= ExpirationDate.Value;
            }

            // Perpetual license
            return true;
        }

        public int DaysRemaining()
        {
            if (!ExpirationDate.HasValue)
                return int.MaxValue;

            var remaining = (ExpirationDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, remaining);
        }

        public string GetDisplayStatus()
        {
            switch (Status)
            {
                case LicenseStatus.Active:
                    if (Type == LicenseType.Trial)
                        return $"Trial ({DaysRemaining()} days remaining)";
                    else if (Type == LicenseType.Subscription)
                        return $"Active (Expires {ExpirationDate?.ToString("yyyy-MM-dd")})";
                    else
                        return "Active (Perpetual)";

                case LicenseStatus.Expired:
                    return "Expired";

                case LicenseStatus.Invalid:
                    return "Invalid";

                case LicenseStatus.Unlicensed:
                    return "Unlicensed";

                default:
                    return "Unknown";
            }
        }
    }

    public enum LicenseType
    {
        Unlicensed = 0,
        Trial = 1,
        Perpetual = 2,
        Subscription = 3
    }

    public enum LicenseStatus
    {
        Unlicensed = 0,
        Active = 1,
        Expired = 2,
        Invalid = 3,
        Revoked = 4
    }
}
