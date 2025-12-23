using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Exodus.Helpers;

public static class UrlUtils
{
    public static void OpenUrl(string url)
    {
        if (url == null!)
            return;
        var startInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            startInfo.FileName = "powershell";
            startInfo.Arguments = "start \"{" + url + "}\"";
            Process.Start(startInfo);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            startInfo.FileName = "xdg-open";
            startInfo.UseShellExecute = false;
            startInfo.Arguments = url;
            Process.Start(startInfo);
        }
        else
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return;
            startInfo.FileName = "open";
            startInfo.Arguments = "\"" + url + "\"";
            Process.Start(startInfo);
        }
    }
}