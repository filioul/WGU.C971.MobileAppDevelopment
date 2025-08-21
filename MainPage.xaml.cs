
using Plugin.LocalNotification;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WGUMauiApp.Classes;
using WGUMauiApp.Services;


namespace WGUMauiApp
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private bool _hasInitialized = false;

    

        public async Task RequestNotificationPermissionOnce()
        {
            bool hasAskedBefore = Preferences.Get("notification_permission_asked", false);

            if (hasAskedBefore)
            {
                return; 
            }

            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 13)
            {
                var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                Preferences.Set("notification_permission_asked", true);
            }
            else
            {
                Preferences.Set("notification_permission_asked", true);
            }
        }

        private async Task CreateDataForEvaluation()
        {
            var termInventory = await DatabaseService.GetTerms();
            if (termInventory.Terms.Count == 0)
            {
                DateTime defaultStart = DateTime.Now;
                DateTime defaultEnd = defaultStart.AddMonths(6);
                Term newTerm = new Term("New Term", defaultStart, defaultEnd);
                var termID = await DatabaseService.AddTerm(newTerm);

                Course newCourse = new Course("New Course", termID, defaultStart, defaultEnd, "In Progress", "Annika Patel", "555-123-4567", "Annika.Patel@strimeuniversity.edu", "No notes.");
                var courseID = await DatabaseService.AddCourse(newCourse);

                var newPerformanceAssessment = new Assessment("New Assessment", defaultStart, defaultEnd, "Performance Assessment", courseID);
                await DatabaseService.AddAssessment(newPerformanceAssessment);

                var newObjectiveAssessment = new Assessment("New Assessment", defaultStart, defaultEnd, "Objective Assessment", courseID);
                await DatabaseService.AddAssessment(newObjectiveAssessment);
            }
        }

        private async Task LoadAllCourses()
        {
            try
            {
                List<Course> allCourses = await DatabaseService.GetAllCourses();

                var inProgressCourses = allCourses.Where(c => c.Status == "In Progress").ToList();
                var upcomingCourses = allCourses.Where(c => c.Status == "Upcoming").ToList();
                var completedCourses = allCourses.Where(c => c.Status == "Completed").ToList();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    activeCollectionView.ItemsSource = inProgressCourses;
                    upcomingCollectionView.ItemsSource = upcomingCourses;
                    completedCollectionView.ItemsSource = completedCourses;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex}");
            }
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_hasInitialized)
            {
                // Add a small delay to ensure UI is fully loaded
                await Task.Delay(100);

                await RequestNotificationPermissionOnce();
                await DatabaseService.Init();
                await CreateDataForEvaluation();
                _hasInitialized = true;
            }

            // Always refresh courses
            await LoadAllCourses();
        }
       
        
        private async void ViewCourseButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedCourse = button?.BindingContext as Course;
            if (selectedCourse != null)
            {
                await Navigation.PushAsync(new CourseDetailsPage(selectedCourse));
            }
        }
        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<object>(this, "RefreshData");
            base.OnDisappearing();
        }
    }
}

