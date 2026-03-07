using System.Globalization;
using System.Text.RegularExpressions;

namespace NYC311Dashboard.Extensions
{
    public static class StringExtensions
    {
        public static string? FromCamelCaseToProperSpaced(this string? target) =>
            target == null ? target :
            string.Concat(
                target.Select((x, i) =>
                    i > 0 && char.IsUpper(x) && (char.IsLower(target[i - 1]) || i < target.Length - 1 && char.IsLower(target[i + 1]))
                        ? " " + x
                        : x.ToString()));

        public static string? FromCamelCaseToSnakeCase(this string? target) =>
            target == null ? target :
            string.Concat(
                target.Select((x, i) =>
                    i > 0 && char.IsUpper(x) && (char.IsLower(target[i - 1]) || i < target.Length - 1 && char.IsLower(target[i + 1]))
                        ? "_" + x
                        : x.ToString().ToLower()));

        public static string ToProperCase(this string target, string culture = "en-GB")
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return target;
            }

            var textInfo = new CultureInfo(culture, false).TextInfo;
            return textInfo.ToTitleCase(target.ToLower());
        }

        public static string ToKebabCase(this string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return target;
            }

            return Regex.Replace(target.ToLower(), @"\s+", "-");
        }

        public static string InsertNewlines(this string? target, int minWrapAtLength)
        {
            if (string.IsNullOrEmpty(target) || target.Length <= minWrapAtLength)
            {
                return target;
            }

            return string.Join(Environment.NewLine, target
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line =>
                    Regex.Replace(
                        line,
                        $@"(.{{{minWrapAtLength - 1},}}?)(?:\s+|$)",
                        $"$1{Environment.NewLine}")
                    .TrimEnd('\r', '\n')));
        }
    }
}
