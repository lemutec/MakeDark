﻿using System.Runtime.InteropServices;

namespace MakeDark;

internal static class Interop
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(nint hWnd);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmGetWindowAttribute(nint hwnd, DwmWindowAttribute attr, out int attrValue, int attrSize);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(nint hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int RtlGetVersion(ref RTL_OSVERSIONINFOEX lpVersionInformation);

    public static bool IsWindows10Version1809OrAbove()
    {
        RTL_OSVERSIONINFOEX versionInfo = new()
        {
            dwOSVersionInfoSize = (uint)Marshal.SizeOf<RTL_OSVERSIONINFOEX>(),
        };

        if (RtlGetVersion(ref versionInfo) == 0)
        {
            // Windows 10 1809
            return versionInfo.dwMajorVersion >= 10 && versionInfo.dwBuildNumber >= 17763;
        }

        return false;
    }

    public static bool IsDarkModeForWindow(nint hWnd)
    {
        if (IsWindows10Version1809OrAbove())
        {
            int hr = DwmGetWindowAttribute(hWnd, DwmWindowAttribute.UseImmersiveDarkMode, out int darkMode, sizeof(int));
            return hr >= 0 && darkMode == 1;
        }
        return true;
    }

    public static bool EnableDarkModeForWindow(nint hWnd, bool enable = true)
    {
        if (IsWindows10Version1809OrAbove())
        {
            int darkMode = enable ? 1 : 0;
            int hr = DwmSetWindowAttribute(hWnd, DwmWindowAttribute.UseImmersiveDarkMode, ref darkMode, sizeof(int));
            return hr >= 0;
        }
        return true;
    }

    public static bool SetRoundedCorners(nint hWnd, bool enable = true)
    {
        if (IsWindows10Version1809OrAbove())
        {
            int preference = enable ? (int)DwmWindowCornerPreference.DWMWCP_ROUND : (int)DwmWindowCornerPreference.DWMWCP_DONOTROUND;
            int hr = DwmSetWindowAttribute(hWnd, DwmWindowAttribute.WindowCornerPreference, ref preference, sizeof(int));
            return hr >= 0;
        }
        return true;
    }

    public static nint[] GetWindowHandleByProcessId(int pid)
    {
        List<nint> hWnds = [];

        EnumWindows((hWnd, lParam) =>
        {
            _ = GetWindowThreadProcessId(hWnd, out uint processId);

            if (processId == pid)
            {
                hWnds.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);

        return [.. hWnds];
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    public delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    public enum DwmWindowAttribute : uint
    {
        NCRenderingEnabled = 1,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation,
        PassiveUpdateMode,
        UseHostBackdropBrush,
        UseImmersiveDarkMode = 20,
        WindowCornerPreference = 33,
        BorderColor,
        CaptionColor,
        TextColor,
        VisibleFrameBorderThickness,
        SystemBackdropType,
        Last,
    }

    public enum DwmWindowCornerPreference : uint
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_OSVERSIONINFOEX
    {
        public uint dwOSVersionInfoSize;
        public uint dwMajorVersion;
        public uint dwMinorVersion;
        public uint dwBuildNumber;
        public uint dwPlatformId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
    }
}
