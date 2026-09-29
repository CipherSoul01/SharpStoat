using System.Text;

namespace Stoat.Core.Extensions;

public static class StringExtensions
{

    /// <summary>
    /// Truncate the string from the first index of the given character, also removing the indexed character.
    /// </summary>
    /// <param name="str">String to truncate.</param>
    /// <param name="c">Character to locate the index of.</param>
    /// <returns>Truncated string.</returns>
    public static string TruncateFromIndexOf(this string str, char c)
    {
        if(string.IsNullOrEmpty(str))
            return string.Empty;

        int last = str.IndexOf(c);
        if (last > -1)
        {
            return str.Substring(0, last);
        }

        return str;
    }

    /// <summary>
    /// Truncate the string from the last index of the given character, also removing the indexed character.
    /// </summary>
    /// <param name="str">String to truncate.</param>
    /// <param name="c">Character to locate the index of.</param>
    /// <returns>Truncated string.</returns>
    public static string TruncateFromLastIndexOf(this string str, char c)
    {
        if(string.IsNullOrEmpty(str))
            return string.Empty;

        int last = str.LastIndexOf(c);
        if (last > -1)
        {
            return str.Substring(0, last);
        }

        return str;
    }

    /// <summary>
    /// Trim all characters at the start of the string until the first index of the given character,
    /// also removing the indexed character.
    /// </summary>
    /// <param name="str">String to trim.</param>
    /// <param name="c">Character to locate the index of.</param>
    /// <returns>Trimmed string.</returns>
    public static string TrimUntilIndexOf(this string str, char c)
    {
        if(string.IsNullOrEmpty(str))
            return string.Empty;

        int first = str.IndexOf(c);
        if (first > -1)
        {
            return str.Substring(first + 1, str.Length - first - 1);
        }

        return str;
    }

    /// <summary>
    /// Trim all characters at the start of the string until the first index of the given string,
    /// also removing the indexed character.
    /// </summary>
    /// <param name="str">String to trim.</param>
    /// <param name="value">String to locate the index of.</param>
    /// <param name="comparisonType">Comparison rule for locating the string.</param>
    /// <returns>Trimmed string.</returns>
    public static string TrimUntilIndexOf(this string str, string value, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if(string.IsNullOrEmpty(str))
            return string.Empty;

        int first = str.IndexOf(value, comparisonType);
        if (first > -1)
        {
            return str.Substring(first + value.Length, str.Length - first - value.Length);
        }

        return str;
    }

    /// <summary>
    /// Trim all characters at the start of the string until the last index of the given character,
    /// also removing the indexed character.
    /// </summary>
    /// <param name="str">String to trim.</param>
    /// <param name="c">Character to locate the index of.</param>
    /// <returns>Trimmed string.</returns>
    public static string TrimUntilLastIndexOf(this string str, char c)
    {
        if(string.IsNullOrEmpty(str))
            return string.Empty;

        int first = str.LastIndexOf(c);
        if (first > -1)
        {
            return str.Substring(first + 1, str.Length - first - 1);
        }

        return str;
    }

    /// <summary>
    /// Trim all characters at the start of the string until the last index of the given string,
    /// also removing the indexed character.
    /// </summary>
    /// <param name="str">String to trim.</param>
    /// <param name="value">String to locate the index of.</param>
    /// <param name="comparisonType">Comparison rule for locating the string.</param>
    /// <returns>Trimmed string.</returns>
    public static string TrimUntilLastIndexOf(this string str, string value, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if(string.IsNullOrEmpty(str))
            return string.Empty;

        int first = str.LastIndexOf(value, comparisonType);
        if (first > -1)
        {
            return str.Substring(first + value.Length, str.Length - first - value.Length);
        }

        return str;
    }

    /// <summary>
    /// Trim the matching value from the middle of the string.
    /// </summary>
    /// <param name="str">String to trim.</param>
    /// <param name="value">String to locate and remove.</param>
    /// <param name="comparisonType">Comparison rule for locating the string.</param>
    /// <returns>Trimmed string.</returns>
    public static string TrimMiddle(this string str, string value, StringComparison comparisonType = StringComparison.Ordinal)
    {
        int index = str.IndexOf(value, comparisonType);

        if (index > -1)
        {
            return str.Substring(0, index) +
                   str.Substring(index + value.Length);
        }

        return str;
    }

    /// <summary>
    /// Check whether string contains a specified substring.
    /// </summary>
    /// <param name="str">String to check.</param>
    /// <param name="value">String to locate.</param>
    /// <param name="comparisonType">Comparison rule for comparing the strings.</param>
    /// <returns>True if the string contains the substring, false if not.</returns>
    public static bool Contains(this string str, string value, StringComparison comparisonType)
    {
        return str?.IndexOf(value, comparisonType) >= 0;
    }

    /// <summary>
    /// Convert string to snake case.
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <returns>Input string converted to snake case.</returns>
    public static string ToSnakeCase(this string str)
    {
        int len = str.Length;
        var sb = new StringBuilder(2*len);
        for (int i = 0; i < len; i++)
        {
            if (i > 0 && char.IsUpper(str[i]) &&
                (char.IsLower(str[i - 1]) || i < len - 1 && char.IsLower(str[i + 1])))
            {
                sb.Append('_');

            }
            sb.Append(char.ToLower(str[i]));
        }

        return sb.ToString();
    }
}