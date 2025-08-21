using Microsoft.Maui.Controls;
using System;
using WGUMauiApp.Classes;
using WGUMauiApp.Services;
namespace WGUMauiApp;

public partial class TermPage : ContentPage
{
	public TermPage()
	{
		InitializeComponent();
        LoadTerms();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadTerms();
    }

    async void AddTermButtonClicked(object sender, EventArgs args)
    {
        DateTime defaultStart = DateTime.Now;
        DateTime defaultEnd = defaultStart.AddMonths(6);
        Term newTerm = new Term("New Term", defaultStart, defaultEnd);
        await DatabaseService.AddTerm(newTerm);
        LoadTerms();
    }

    private async void LoadTerms()
    {
        try
        {
            var termInventory = await DatabaseService.GetTerms();
            termsCollectionView.ItemsSource = termInventory.Terms;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void EditTermButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedTerm = button?.BindingContext as Term;

        if (selectedTerm != null)
        {
            await Navigation.PushAsync(new EditTermPage(selectedTerm));
        }
    }

    private async void AddCourseButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedTerm = button?.BindingContext as Term;

        if (selectedTerm != null)
        {
            Course newCourse = new Course("New Course", selectedTerm.Id, selectedTerm.StartDate, selectedTerm.EndDate, "Upcoming", "John Smith", "555-555-5555", "johnsmith@wgu.edu", "No notes.");
            await DatabaseService.AddCourse(newCourse);
        }
        LoadTerms();
    }

    private async void EditCourseButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedCourse = button?.BindingContext as Course;
        if (selectedCourse != null)
        {
            await Navigation.PushAsync(new EditCoursePage(selectedCourse));
        }
    }
}