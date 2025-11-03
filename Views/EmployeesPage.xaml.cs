using GraafikPiip.ViewModels;

namespace GraafikPiip.Views;

public partial class EmployeesPage : ContentPage
{
	public EmployeesPage(EmployeesPageViewModel epvm)
	{
		InitializeComponent();
        BindingContext = epvm;	
        Shell.SetBackgroundColor(this, Color.FromArgb("#111214"));
        Shell.SetTitleColor(this, Color.FromArgb("#F3F4F6"));
    }
}