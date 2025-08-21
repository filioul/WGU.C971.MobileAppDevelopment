using System.Threading.Tasks;
using WGUMauiApp.Classes;
using WGUMauiApp.Services;


namespace WGUMauiApp;

public partial class EditTermPage : ContentPage
{
    private Term _currentTerm;
    public EditTermPage(Term selectedTerm)
	{
		InitializeComponent();
        _currentTerm = selectedTerm;
        LoadTermData();
    }
    private void LoadTermData()
    {
        TitleEntry.Text = _currentTerm.Title;
        StartDatePicker.Date = _currentTerm.StartDate;
        EndDatePicker.Date = _currentTerm.EndDate;
    }

    private async void DeleteTermButton_Clicked(object sender, EventArgs e)
    {
        await DatabaseService.DeleteTerm(_currentTerm.Id);
        await Navigation.PopAsync();
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        if(ValidateStartAndEndDates(StartDatePicker.Date, EndDatePicker.Date))
        {
            _currentTerm.Title = TitleEntry.Text;
            _currentTerm.StartDate = StartDatePicker.Date;
            _currentTerm.EndDate = EndDatePicker.Date;

            await DatabaseService.UpdateTerm(_currentTerm);

            await Navigation.PopAsync();
        }
    }

    private bool ValidateStartAndEndDates(DateTime start, DateTime end)
    {
        if (end > start)
        {
            return true;
        }
        else
        {
            DisplayMessageForInvalidDates();
            return false;
        }
    }

    private async void DisplayMessageForInvalidDates()
    {
        await DisplayAlert("Validation error", "Please enter a start date that is before the end date.", "OK");
    }
}