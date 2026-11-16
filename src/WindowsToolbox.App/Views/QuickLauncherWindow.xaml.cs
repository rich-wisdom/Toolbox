using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WindowsToolbox.App.ViewModels;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.App.Views;

public sealed partial class QuickLauncherWindow : Window
{
    public QuickLauncherViewModel ViewModel { get; }

    public QuickLauncherWindow(QuickLauncherViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        
        ExtendsContentIntoTitleBar = true;
        
        KeyDown += OnKeyDown;
        Activated += OnActivated;
        
        ToolsList.ItemsSource = ViewModel.FilteredTools;
    }

    public void ShowAndActivate()
    {
        Activate();
        CenterWindow();
        SearchBox.Focus(FocusState.Programmatic);
        ViewModel.RefreshRecentTools();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            Hide();
        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            Hide();
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Enter && ToolsList.SelectedItem is ITool tool)
        {
            ViewModel.LaunchTool(tool);
            Hide();
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Down)
        {
            if (ToolsList.Items.Count > 0)
            {
                ToolsList.SelectedIndex = Math.Min(ToolsList.SelectedIndex + 1, ToolsList.Items.Count - 1);
            }
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Up)
        {
            if (ToolsList.Items.Count > 0)
            {
                ToolsList.SelectedIndex = Math.Max(ToolsList.SelectedIndex - 1, 0);
            }
            e.Handled = true;
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.SearchQuery = SearchBox.Text;
    }

    private void ToolsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ITool tool)
        {
            ViewModel.LaunchTool(tool);
            Hide();
        }
    }

    private void CenterWindow()
    {
        var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
        var workArea = displayArea.WorkArea;
        
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(
            Microsoft.UI.Win32Interop.GetWindowIdFromWindow(
                WinRT.Interop.WindowNative.GetWindowHandle(this)));
        
        var x = (workArea.Width - appWindow.Size.Width) / 2;
        var y = (workArea.Height - appWindow.Size.Height) / 3;
        
        appWindow.Move(new Windows.Graphics.PointInt32(x, y));
    }

    private void Hide()
    {
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(
            Microsoft.UI.Win32Interop.GetWindowIdFromWindow(
                WinRT.Interop.WindowNative.GetWindowHandle(this)));
        appWindow.Hide();
    }
}
