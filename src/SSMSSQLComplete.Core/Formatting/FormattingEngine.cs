using System.Collections.Generic;

namespace SSMSSQLComplete.Core.Formatting
{
    public class FormattingEngine
    {
        private readonly Dictionary<string, FormattingProfile> _profiles;
        private FormattingProfile _currentProfile;

        public FormattingEngine()
        {
            _profiles = new Dictionary<string, FormattingProfile>
            {
                { "Standard", FormattingProfile.StandardProfile },
                { "Compact", FormattingProfile.CompactProfile }
            };

            _currentProfile = FormattingProfile.StandardProfile;
        }

        public string Format(string sql)
        {
            var formatter = new SqlFormatter(_currentProfile);
            return formatter.Format(sql);
        }

        public string Format(string sql, string profileName)
        {
            if (_profiles.TryGetValue(profileName, out var profile))
            {
                var formatter = new SqlFormatter(profile);
                return formatter.Format(sql);
            }

            return Format(sql); // Use default
        }

        public void SetProfile(FormattingProfile profile)
        {
            _currentProfile = profile;
        }

        public void AddProfile(string name, FormattingProfile profile)
        {
            _profiles[name] = profile;
        }

        public IEnumerable<string> GetProfileNames()
        {
            return _profiles.Keys;
        }
    }
}
