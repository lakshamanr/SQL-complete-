using System;
using System.Linq;

namespace SSMSSQLComplete.Core.Completion
{
    public class FuzzyMatcher
    {
        /// <summary>
        /// Calculates fuzzy match score between pattern and text
        /// Returns 0 for no match, higher values for better matches
        /// </summary>
        public static double CalculateScore(string pattern, string text, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(pattern))
                return 1.0;

            if (string.IsNullOrEmpty(text))
                return 0.0;

            if (!caseSensitive)
            {
                pattern = pattern.ToLowerInvariant();
                text = text.ToLowerInvariant();
            }

            // Exact match
            if (pattern == text)
                return 100.0;

            // Starts with pattern
            if (text.StartsWith(pattern))
                return 90.0 + (pattern.Length / (double)text.Length * 10);

            // Contains pattern
            if (text.Contains(pattern))
                return 70.0 + (pattern.Length / (double)text.Length * 10);

            // Fuzzy match - check if all pattern characters exist in order
            var score = FuzzyScore(pattern, text);
            return score;
        }

        private static double FuzzyScore(string pattern, string text)
        {
            int patternIdx = 0;
            int textIdx = 0;
            int matchCount = 0;
            int consecutiveMatches = 0;
            double score = 0;

            while (patternIdx < pattern.Length && textIdx < text.Length)
            {
                if (pattern[patternIdx] == text[textIdx])
                {
                    matchCount++;
                    consecutiveMatches++;
                    score += 1.0 + (consecutiveMatches * 0.5); // Bonus for consecutive matches
                    patternIdx++;
                }
                else
                {
                    consecutiveMatches = 0;
                }
                textIdx++;
            }

            if (patternIdx != pattern.Length)
                return 0.0; // Not all pattern characters matched

            // Normalize score
            double matchRatio = matchCount / (double)pattern.Length;
            double lengthPenalty = 1.0 - ((text.Length - pattern.Length) / (double)text.Length);

            return (score / pattern.Length) * matchRatio * lengthPenalty * 50;
        }

        /// <summary>
        /// Checks if text matches pattern
        /// </summary>
        public static bool IsMatch(string pattern, string text, bool caseSensitive = false)
        {
            return CalculateScore(pattern, text, caseSensitive) > 0;
        }
    }
}
