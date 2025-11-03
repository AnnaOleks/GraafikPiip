using GraafikPiip.ViewModels;

namespace GraafikPiip.Views;

public partial class MonthCalendarPage : ContentPage
{
	public MonthCalendarPage(MonthCalendarPageViewModel mcvm)
	{
		InitializeComponent();
        BindingContext = mcvm;
        Shell.SetBackgroundColor(this, Color.FromArgb("#111214"));
        Shell.SetTitleColor(this, Color.FromArgb("#F3F4F6"));
    }
}