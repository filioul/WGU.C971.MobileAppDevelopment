using WGUMauiApp.Services;
using WGUMauiApp.Classes;
using System.Threading.Tasks;

namespace WGUMauiApp;

public partial class ViewNotesPage : ContentPage
{
    private Course _currentCourse;
    public ViewNotesPage(Course selectedCourse)
    {
        InitializeComponent();
        _currentCourse = selectedCourse;
        BindingContext = _currentCourse;

        if (string.IsNullOrWhiteSpace(_currentCourse.Notes))
        {
            NoNotesLabel.IsVisible = true;
        }
        else
        {
            NotesLabel.IsVisible = true;
        }
    }

    private async void EditNotesButton_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Edit Notes", "Enter your notes:", initialValue: _currentCourse.Notes ?? "", maxLength: 500);

        if (result != null) // User didn't cancel
        {
            _currentCourse.Notes = result;
            await DatabaseService.UpdateCourse(_currentCourse);

            bool hasNotes = !string.IsNullOrWhiteSpace(_currentCourse.Notes);
            NotesLabel.IsVisible = hasNotes;
            NoNotesLabel.IsVisible = !hasNotes;

            BindingContext = null;
            BindingContext = _currentCourse;
        }
    }

    private async void ShareNotesButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentCourse.Notes))
        {
            await DisplayAlert("Error", "You don't have any notes for this course.", "OK");
        }
        else
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = _currentCourse.Notes,
                Title = "Share notes."
            });
        }
    }
}