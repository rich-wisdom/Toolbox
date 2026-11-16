using Microsoft.UI.Xaml.Controls;
using WindowsToolbox.Core.Models;
using WindowsToolbox.Core.Services;

namespace WindowsToolbox.App.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        var toolRegistry = App.Services.GetService(typeof(IToolRegistry)) as IToolRegistry;
        CategoriesGrid.ItemsSource = toolRegistry?.GetCategories();
    }

    private void CategoriesGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ToolCategory category)
        {
            Frame.Navigate(typeof(CategoryPage), category.Id);
        }
    }
}
