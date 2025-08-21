using WGUMauiApp.Classes;
using WGUMauiApp.Services;
using System.Text.RegularExpressions;
using Plugin.LocalNotification;

namespace WGUMauiApp;

public partial class EditCoursePage : ContentPage
{
	private Course _currentCourse;
	public EditCoursePage(Course selectedCourse)
	{
		InitializeComponent();
		_currentCourse = selectedCourse;
		LoadCourseData();
	}

    private void LoadCourseData()
    {
        TitleEntry.Text = _currentCourse.Title;
        StartDatePicker.Date = _currentCourse.StartDate;
        EndDatePicker.Date = _currentCourse.EndDate;
        InstructorNameEntry.Text = _currentCourse.InstructorName;
        InstructorNumberEntry.Text = _currentCourse.InstructorNumber;
        InstructorEmailEntry.Text = _currentCourse.InstructorEmail;
    }

    private async void DeleteCourseButton_Clicked(object sender, EventArgs e)
    {
        await DatabaseService.DeleteCourse(_currentCourse.Id);
        await NotificationHelper.ClearAllCourseNotifications(_currentCourse.Id);
        await Navigation.PopAsync();
    }

    public enum NotificationType
    {
        CourseStart = 1,
        CourseEnd = 2
    }

    public static class NotificationHelper
    {
        public static int GenerateNotificationId(int courseId, NotificationType type)
        {
            return courseId * 100 + (int)type;
        }

        public static async Task ClearAllCourseNotifications(int courseId)
        {
            var startId = GenerateNotificationId(courseId, NotificationType.CourseStart);
            var endId = GenerateNotificationId(courseId, NotificationType.CourseEnd);

            LocalNotificationCenter.Current.Cancel(startId);
            LocalNotificationCenter.Current.Cancel(endId);
        }
    }

        private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        if (ValidateInstructorInfo(InstructorNameEntry.Text, InstructorNumberEntry.Text, InstructorEmailEntry.Text) && ValidateStartAndEndDates(StartDatePicker.Date, EndDatePicker.Date))
        {
            _currentCourse.Title = TitleEntry.Text;
            _currentCourse.StartDate = StartDatePicker.Date;
            _currentCourse.EndDate = EndDatePicker.Date;
            _currentCourse.Status = StatusPicker.SelectedItem?.ToString();
            _currentCourse.InstructorName = InstructorNameEntry.Text;
            _currentCourse.InstructorNumber = InstructorNumberEntry.Text;
            _currentCourse.InstructorEmail = InstructorEmailEntry.Text;
            await NotificationHelper.ClearAllCourseNotifications(_currentCourse.Id);

            if (StartDateNotificationsSwitch.IsToggled)
            {
                var startRequest = new NotificationRequest
                {
                    NotificationId = NotificationHelper.GenerateNotificationId(_currentCourse.Id, NotificationType.CourseStart),
                    Title = $"{_currentCourse.Title} - Starting Today",
                    Description = $"Your course '{_currentCourse.Title}' starts today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = _currentCourse.StartDate
                    }
                };
                await LocalNotificationCenter.Current.Show(startRequest);
            }

            if (EndDateNotificationsSwitch.IsToggled && _currentCourse.EndDate > DateTime.Now)
            {
                var endRequest = new NotificationRequest
                {
                    NotificationId = NotificationHelper.GenerateNotificationId(_currentCourse.Id, NotificationType.CourseEnd),
                    Title = $"{_currentCourse.Title} - Ending Today",
                    Description = $"Your course '{_currentCourse.Title}' ends today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = _currentCourse.EndDate
                    }
                };

                await LocalNotificationCenter.Current.Show(endRequest);
            }

            await DatabaseService.UpdateCourse(_currentCourse);
            await Navigation.PopAsync();
        }
    }

    private bool ValidateInstructorInfo(string name, string phoneNumber, string emailAddress)
    {
        bool validator = false;
        var regexPhoneNumber = new Regex("\\d\\d\\d-\\d\\d\\d-\\d\\d\\d\\d");
        var regexEmailAddress = new Regex("[A-Za-z0-9]+@[A-Za-z0-9]+\\.[A-Za-z0-9]+");
        if (string.IsNullOrEmpty(name))
        {
            DisplayMessageForInvalidName();
        } else if (!regexPhoneNumber.IsMatch(phoneNumber))
        {
            DisplayMessageForInvalidNumber();
        } else if (!regexEmailAddress.IsMatch(emailAddress))
        {
            DisplayMessageForInvalidEmailAddress();
        } else
        {
            validator = true;
        }
        return validator;
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

    private async void DisplayMessageForInvalidEmailAddress()
    {
        await DisplayAlert("Validation error", "Please enter a valid email address and try again.", "OK");
    }

    private async void DisplayMessageForInvalidNumber()
    {
        await DisplayAlert("Validation error", "Please enter a valid phone number and try again.", "OK");
    }

    private async void DisplayMessageForInvalidName()
    {
        await DisplayAlert("Validation error", "Please enter a valid instructor name and try again.", "OK");
    }

    private async void DisplayMessageForInvalidDates()
    {
        await DisplayAlert("Validation error", "Please enter a start date that is before the end date.", "OK");
    }
}
