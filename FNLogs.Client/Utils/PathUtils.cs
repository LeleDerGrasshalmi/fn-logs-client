using System.Text.RegularExpressions;

namespace FNLogs.Client.Utils;

internal static partial class PathUtils
{
    internal static string ReplacePlaceholdersInPath(string path)
        => path
            .Replace(LocalAppDataRegex(), Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    internal static string Replace(this string str, Regex regex, string replacement)
        => regex.Replace(str, replacement);

    [GeneratedRegex("%LocalAppData%", RegexOptions.IgnoreCase, "de-DE")]
    private static partial Regex LocalAppDataRegex();
}
