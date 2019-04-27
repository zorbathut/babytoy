﻿
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

class WindowsBlocker
{
    private delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hHook);

    [DllImport("user32", EntryPoint = "CallNextHookEx", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

    private const int WH_KEYBOARD_LL = 13;

    /*code needed to disable start menu*/
    [DllImport("user32.dll")]
    private static extern int FindWindow(string className, string windowText);
    [DllImport("user32.dll")]
    private static extern int ShowWindow(int hwnd, int command);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 1;

    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    private static IntPtr hookHandle;

    private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
    {
        bool blnEat = false;

        try
        {
            switch (wParam.ToInt64())
            {
                case 256:
                case 257:
                case 260:
                case 261:

                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key,
                    blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32))  // alt+tab
                        | ((lParam.vkCode == 27) && (lParam.flags == 32)) // alt+esc
                        | ((lParam.vkCode == 27) && (lParam.flags == 0))  // ctrl+esc
                        | ((lParam.vkCode == 91) && (lParam.flags == 1))  // left winkey
                        | ((lParam.vkCode == 92) && (lParam.flags == 1))
                        | ((lParam.vkCode == 73) && (lParam.flags == 0));

                    break;
            }


        }
        catch (Exception ex)
        {
            Dbg.Ex(ex);
        }

        if (blnEat == true)
        {
            return (IntPtr)1;
        }
        else
        {
            return CallNextHookEx(hookHandle, nCode, wParam, ref lParam);
        }
    }

    public void Enable(bool enabled)
    {
        if (enabled && hookHandle.ToInt64() == 0)
        {
            using (ProcessModule curModule = Process.GetCurrentProcess().MainModule)
            {
                hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, GetModuleHandle(curModule.ModuleName), 0);

                if (hookHandle.ToInt64() == 0)
                {
                    Dbg.Err("Failed to hook!");
                }
            }
        }
        else if (!enabled && hookHandle.ToInt64() != 0)
        {
            UnhookWindowsHookEx(hookHandle);
            hookHandle = new IntPtr(0);
        }
    }
}
