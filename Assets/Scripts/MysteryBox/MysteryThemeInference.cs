using System;
using System.Collections.Generic;

namespace DressUpGame.MysteryBox
{
    public static class MysteryThemeInference
    {
        private static readonly string[] ColorTokens =
        {
            "pink", "red", "orange", "yellow", "green", "blue", "purple", "brown", "black", "white",
            "grey", "gray", "gold", "silver", "mint", "lavender", "coral", "peach", "navy", "teal"
        };

        private static readonly string[] MotifTokens =
        {
            "star", "heart", "flower", "rainbow", "princess", "crown", "bow", "sparkle"
        };

        public static string[] InferTags(string itemId, string displayName)
        {
            HashSet<string> tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string combined = $"{itemId} {displayName}".ToLowerInvariant();

            foreach (string token in ColorTokens)
            {
                if (combined.Contains(token))
                {
                    tags.Add(Capitalize(token));
                }
            }

            foreach (string token in MotifTokens)
            {
                if (combined.Contains(token))
                {
                    tags.Add(Capitalize(token));
                }
            }

            if (tags.Count == 0)
            {
                return Array.Empty<string>();
            }

            string[] result = new string[tags.Count];
            tags.CopyTo(result);
            return result;
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return char.ToUpperInvariant(value[0]).ToString();
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static bool SharesAnyTag(string[] left, string[] right)
        {
            if (left == null || right == null || left.Length == 0 || right.Length == 0)
            {
                return false;
            }

            foreach (string tag in left)
            {
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }

                foreach (string other in right)
                {
                    if (string.Equals(tag, other, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
