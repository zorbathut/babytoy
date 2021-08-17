
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

class WindowsBlocker
{
    #if true
    private delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32", EntryPoint = "SetWindowsHookExA", SetLastError = true)]
    private static extern IntPtr SetWindowsHookExA(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hHook);

    [DllImport("user32", EntryPoint = "CallNextHookEx", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

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

    private static IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        bool blnEat = false;

        Dbg.Inf("LLKPA");

        try
        {
            var o = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

            Dbg.Inf("LLKP");
            switch (wParam.ToInt64())
            {
                case 256:
                case 257:
                case 260:
                case 261:

                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key,
                    blnEat = ((o.vkCode == 9) && (o.flags == 32))  // alt+tab
                        | ((o.vkCode == 27) && (o.flags == 32)) // alt+esc
                        | ((o.vkCode == 27) && (o.flags == 0))  // ctrl+esc
                        | ((o.vkCode == 91) && (o.flags == 1))  // left winkey
                        | ((o.vkCode == 92) && (o.flags == 1))
                        | ((o.vkCode == 73) && (o.flags == 0));

                    break;
            }
        }
        catch (Exception ex)
        {
            Dbg.Ex(ex);
        }

        Dbg.Inf($"LLKP BIE {blnEat}");

        if (blnEat == true)
        {
            return (IntPtr)1;
        }
        else
        {
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }
    }

    public void Enable(bool enabled)
    {
        if (enabled && hookHandle.ToInt64() == 0)
        {
            using (ProcessModule curModule = Process.GetCurrentProcess().MainModule)
            {
                Dbg.Inf($"Hookening");

                hookHandle = SetWindowsHookExA(WH_KEYBOARD_LL, LowLevelKeyboardProc, GetModuleHandle(curModule.ModuleName), 0);

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
    #else
    public void Enable(bool enabled) { }
    #endif
}
