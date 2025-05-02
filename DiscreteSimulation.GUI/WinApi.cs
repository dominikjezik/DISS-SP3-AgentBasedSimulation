using System.Runtime.InteropServices;
using System;

namespace DiscreteSimulation.GUI;

// Kód pre používanie WPF elementov v Avalonii bol použitý z tohto článku:
// https://www.codeproject.com/Articles/5348155/Embedding-Native-Windows-and-Linux-Views-Controls
static unsafe class WinApi
{
    [DllImport("user32.dll", SetLastError = true)]
    public static unsafe extern bool DestroyWindow(IntPtr hwnd);
}
