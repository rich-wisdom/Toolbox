using System.Runtime.InteropServices;

namespace WindowsToolbox.App.Services;

public interface IGlobalHotKeyService
{
    void RegisterHotKey(ModifierKeys modifiers, VirtualKey key, Action callback);
    void UnregisterAllHotKeys();
}

public class GlobalHotKeyService : IGlobalHotKeyService
{
    private const int WM_HOTKEY = 0x0312;
    private readonly Dictionary<int, Action> _hotKeyCallbacks = new();
    private int _currentId = 1;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public void RegisterHotKey(ModifierKeys modifiers, VirtualKey key, Action callback)
    {
        var id = _currentId++;
        _hotKeyCallbacks[id] = callback;
        
        var hwnd = GetMainWindowHandle();
        if (hwnd != IntPtr.Zero)
        {
            RegisterHotKey(hwnd, id, (uint)modifiers, (uint)key);
        }
    }

    public void UnregisterAllHotKeys()
    {
        var hwnd = GetMainWindowHandle();
        if (hwnd != IntPtr.Zero)
        {
            foreach (var id in _hotKeyCallbacks.Keys)
            {
                UnregisterHotKey(hwnd, id);
            }
        }
        _hotKeyCallbacks.Clear();
    }

    private IntPtr GetMainWindowHandle()
    {
        var mainWindow = Microsoft.UI.Xaml.Application.Current?.Windows?.FirstOrDefault();
        if (mainWindow != null)
        {
            return WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
        }
        return IntPtr.Zero;
    }
}
