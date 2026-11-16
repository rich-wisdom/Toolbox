using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WindowsToolbox.Core.Models;
using WindowsToolbox.Core.Services;

namespace WindowsToolbox.App.Views;

public sealed partial class CategoryPage : Page
{
    private string? _categoryId;

    public CategoryPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string categoryId)
        {
            _categoryId = categoryId;
            LoadCategory();
        }
    }

    private void LoadCategory()
    {
        if (_categoryId == null) return;

        var toolRegistry = App.Services.GetService(typeof(IToolRegistry)) as IToolRegistry;
        var category = toolRegistry?.GetCategories().FirstOrDefault(c => c.Id == _categoryId);
        
        if (category != null)
        {
            CategoryTitle.Text = category.Name;
            CategoryDescription.Text = category.Description;
            ToolsGrid.ItemsSource = toolRegistry?.GetToolsByCategory(_categoryId);
        }
    }
}
