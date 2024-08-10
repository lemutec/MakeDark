using Lnk;
using System.Diagnostics;

namespace MakeDark;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length <= 0)
        {
            return;
        }

        string fileName = args[0];

        if (!File.Exists(fileName))
        {
            return;
        }

        if (Path.GetExtension(fileName)?.ToLower().Equals(".lnk") ?? false)
        {
            LnkFile? src = ShortcutHelper.Open(fileName);

            if (src != null)
            {
                if (src.LocalPath == Environment.ProcessPath)
                {
                    // Done.
                    return;
                }

                LnkFile2 tar = new()
                {
                    SourceFile = src.SourceFile,
                    LocalPath = Environment.ProcessPath,
                    WorkingDirectory = src.WorkingDirectory,
                    Arguments = ArgumentExtension.ToArguments([src.LocalPath, src.Arguments]),
                    Description = "Make Dark Mode Launcher Lnk",
                    IconLocation = src.LocalPath
                };
                _ = ShortcutHelper.Create(tar);
            }
        }
        else
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = args[0],
                    Arguments = string.Join(" ", args[1..].Select(a => $"\"{a}\""))
                }
            };

            process.Start();
            process.WaitForInputIdle();

            SpinWait.SpinUntil(() => process.HasExited || process.MainWindowHandle != IntPtr.Zero);

            if (process.HasExited)
            {
                return;
            }

            NativeMethods.EnableDarkModeForWindow(process.MainWindowHandle);
            NativeMethods.SetRoundedCorners(process.MainWindowHandle);
        }
    }
}
