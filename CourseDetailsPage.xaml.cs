using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using WGUMauiApp.Classes;
using WGUMauiApp.Services;

namespace WGUMauiApp;

public partial class CourseDetailsPage : ContentPage

{
    private Course _currentCourse;

    public CourseDetailsPage(Course selectedCourse)
    {
        InitializeComponent();
        _currentCourse = selectedCourse;
        BindingContext = _currentCourse;
        LoadAssessments();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAssessments();
    }

    async void ViewNotesButtonClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedCourse = button?.BindingContext as Course;
        await Navigation.PushAsync(new ViewNotesPage(selectedCourse));
    }


    private async void AddAssessments_Clicked(object sender, EventArgs e)
    {
        var assessments = await DatabaseService.GetAssessmentsByCourseId(_currentCourse.Id);
        if (assessments.Count == 2)
        {
            DisplayMessageForAssessmentsFull();
        } else if (assessments.Count == 0)
        {
            var newPerformanceAssessment = new Assessment("New Assessment", _currentCourse.StartDate, _currentCourse.EndDate, "Performance Assessment", _currentCourse.Id);
            await DatabaseService.AddAssessment(newPerformanceAssessment);
        } else
        {
            var newObjectiveAssessment = new Assessment("New Assessment", _currentCourse.StartDate, _currentCourse.EndDate, "Objective Assessment", _currentCourse.Id);
            await DatabaseService.AddAssessment(newObjectiveAssessment);
        }
        LoadAssessments();
    }

    private async void DisplayMessageForAssessmentsFull()
    {
        await DisplayAlert("Cannot add new assessment.", "This course already has the max number of assessments.", "OK");
    }

    private async void LoadAssessments()
    {
        var assessments = await DatabaseService.GetAssessmentsByCourseId(_currentCourse.Id);

        AssessmentsCollectionView.ItemsSource = assessments;
        NoAssessmentsLabel.IsVisible = assessments.Count == 0;
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

    private async void EditAssessmentButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedAssessment = button?.BindingContext as Assessment;
        if (selectedAssessment != null)
        {
            await Navigation.PushAsync(new EditAssessmentPage(selectedAssessment));
        }
    }
}