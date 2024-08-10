using Lnk;

namespace MakeDark;

internal static class ShortcutHelper
{
    public static LnkFile? Open(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null!;
        }

        byte[] raw = File.ReadAllBytes(fileName);

        if (raw[0] == 0x4c)
        {
            LnkFile lnkObj = new(raw, fileName);
            return lnkObj;
        }
        return null!;
    }

    public static bool Create(LnkFile2 lnkFile)
    {
        if (string.IsNullOrWhiteSpace(lnkFile.SourceFile))
        {
            return false;
        }

        FileInfo lnkFileInfo = new(lnkFile.SourceFile);

        if (!Directory.Exists(lnkFileInfo.DirectoryName))
        {
            _ = Directory.CreateDirectory(lnkFileInfo.DirectoryName!);
        }

#if false
        IWshRuntimeLibrary.WshShell shell = new();
        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkFile.SourceFile);
        shortcut.TargetPath = lnkFile.LocalPath;
        shortcut.WorkingDirectory = lnkFile.WorkingDirectory;
        shortcut.WindowStyle = 1;
        shortcut.Arguments = lnkFile.Arguments;
        shortcut.Description = lnkFile.Description;
        shortcut.IconLocation = lnkFile.IconLocation;
        shortcut.Save();
#else
        dynamic shell = null!;
        dynamic shortcut = null!;

        try
        {
            shell = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"))!)!;
            shortcut = shell.CreateShortcut(lnkFile.SourceFile);
            shortcut.TargetPath = lnkFile.LocalPath;
            shortcut.WorkingDirectory = lnkFile.WorkingDirectory;
            shortcut.WindowStyle = 1;
            shortcut.Arguments = lnkFile.Arguments;
            shortcut.Description = lnkFile.Description;
            shortcut.IconLocation = lnkFile.IconLocation;
            shortcut.Save();
        }
        finally
        {
            if (shortcut != null)
            {
                _ = System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            }
            if (shell != null)
            {
                _ = System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            }
        }
#endif
        return true;
    }
}

public sealed class LnkFile2
{
    public string? SourceFile { get; set; }
    public string? LocalPath { get; set; }
    public string? WorkingDirectory { get; set; }
    public string? Arguments { get; set; }
    public string? Description { get; set; }
    public string? IconLocation { get; set; }
    public int WindowStyle { get; set; } = 1;
}

public static class ArgumentExtension
{
    public static string[] ParseArguments(string commandLine)
    {
        List<string> args = [];
        string currentArg = string.Empty;
        bool inQuotes = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (currentArg != string.Empty)
                {
                    args.Add(currentArg);
                    currentArg = string.Empty;
                }
            }
            else
            {
                currentArg += c;
            }
        }

        if (currentArg != string.Empty)
        {
            args.Add(currentArg);
        }

        return [.. args];
    }

    public static string ToArguments(IEnumerable<string> args)
    {
        return string.Join(" ", args?.Select(arg => (arg?.Contains(' ') ?? false) ? $"\"{arg}\"" : arg) ?? []);
    }
}
