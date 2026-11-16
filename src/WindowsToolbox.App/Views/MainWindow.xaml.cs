using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WindowsToolbox.App.ViewModels;

namespace WindowsToolbox.App.Views;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(NavView);
        
        ContentFrame.Navigate(typeof(HomePage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            switch (tag)
            {
                case "Home":
                    ContentFrame.Navigate(typeof(HomePage));
                    break;
                default:
                    ContentFrame.Navigate(typeof(CategoryPage), tag);
                    break;
            }
        }
    }
}
