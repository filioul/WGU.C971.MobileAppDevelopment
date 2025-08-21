using WGUMauiApp.Classes;
using WGUMauiApp.Services;
using Plugin.LocalNotification;

namespace WGUMauiApp;

public partial class EditAssessmentPage : ContentPage
{
	private Assessment _currentAssessment;
	public EditAssessmentPage(Assessment selectedAssessment)
	{
		InitializeComponent();
        _currentAssessment = selectedAssessment;
        BindingContext = _currentAssessment;
		LoadAssessmentData();
    }

    private void LoadAssessmentData()
    {
        TitleEntry.Text = _currentAssessment.AssessmentTitle;
        StartDatePicker.Date = _currentAssessment.StartDate;
        EndDatePicker.Date = _currentAssessment.DueDate;
    }

    private async void DeleteAssessmentButton_Clicked(object sender, EventArgs e)
    {
        await DatabaseService.DeleteAssessment(_currentAssessment.Id);
        await NotificationHelper.ClearAllAssessmentNotifications(_currentAssessment.Id);
        await Navigation.PopAsync();
    }

    public enum NotificationType
    {
        AssessmentStart = 3,
        AssessmentEnd = 4
    }

    public static class NotificationHelper
    {
        public static int GenerateNotificationId(int assessmentId, NotificationType type)
        {
            return assessmentId * 100 + (int)type;
        }

        public static async Task ClearAllAssessmentNotifications(int courseId)
        {
            var startId = GenerateNotificationId(courseId, NotificationType.AssessmentStart);
            var endId = GenerateNotificationId(courseId, NotificationType.AssessmentEnd);

            LocalNotificationCenter.Current.Cancel(startId);
            LocalNotificationCenter.Current.Cancel(endId);
        }

    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        if (ValidateStartAndEndDates(StartDatePicker.Date, EndDatePicker.Date)) {

            _currentAssessment.AssessmentTitle = TitleEntry.Text;
            _currentAssessment.StartDate = StartDatePicker.Date;
            _currentAssessment.DueDate = EndDatePicker.Date;

            await NotificationHelper.ClearAllAssessmentNotifications(_currentAssessment.Id);
            if (StartDateNotificationsSwitch.IsToggled)
            {
                var startRequest = new NotificationRequest
                {
                    NotificationId = NotificationHelper.GenerateNotificationId(_currentAssessment.Id, NotificationType.AssessmentStart),
                    Title = $"{_currentAssessment.AssessmentTitle} - Starting Today",
                    Description = $"Your course '{_currentAssessment.AssessmentTitle}' starts today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = _currentAssessment.StartDate
                    }
                };
                await LocalNotificationCenter.Current.Show(startRequest);
            }

            if (EndDateNotificationsSwitch.IsToggled && _currentAssessment.DueDate > DateTime.Now)
            {
                var endRequest = new NotificationRequest
                {
                    NotificationId = NotificationHelper.GenerateNotificationId(_currentAssessment.Id, NotificationType.AssessmentEnd),
                    Title = $"{_currentAssessment.AssessmentTitle} - Due Today",
                    Description = $"Your course '{_currentAssessment.AssessmentTitle}' is due today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = _currentAssessment.DueDate
                    }
                };

                await LocalNotificationCenter.Current.Show(endRequest);
            }

            await DatabaseService.UpdateAssessment(_currentAssessment);
            await Navigation.PopAsync();
        }

    }

    private bool ValidateStartAndEndDates(DateTime start, DateTime end)
    {
        if (end > start)
        {
            return true;
        } else
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