using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms.Integration;
using Avalonia.Controls;
using Avalonia.Platform;

namespace DiscreteSimulation.GUI;

// Kód pre používanie WPF elementov v Avalonii bol použitý z tohto článku:
// https://www.codeproject.com/Articles/5348155/Embedding-Native-Windows-and-Linux-Views-Controls
public class EmbedFrameworkElement : NativeControlHost
{
    private readonly FrameworkElement _frameworkElement;

    public EmbedFrameworkElement(FrameworkElement frameworkElement)
    {
        _frameworkElement = frameworkElement;
    }
    
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // use ElementHost to produce a win32 Handle for embedding
            ElementHost elementHost = new ElementHost();

            elementHost.Child = _frameworkElement;

            return new PlatformHandle(elementHost.Handle, "Hndl");
        }
        return base.CreateNativeControlCore(parent);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // destroy the win32 window
            WinApi.DestroyWindow(control.Handle);

            return;
        }

        base.DestroyNativeControlCore(control);
    }
}
