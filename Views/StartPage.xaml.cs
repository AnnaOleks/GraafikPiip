using GraafikPiip.ViewModels;

namespace GraafikPiip.Views;

public partial class StartPage : ContentPage
{
	public StartPage(StartPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        Shell.SetBackgroundColor(this, Color.FromArgb("#111214"));
        Shell.SetTitleColor(this, Color.FromArgb("#F3F4F6"));
    }
}