using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TravelCompanion
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) 
                != Permission.Granted ||
                CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) 
                != Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessFineLocation, 
                    Manifest.Permission.AccessCoarseLocation }, 0);
            }
        }
    }
}
