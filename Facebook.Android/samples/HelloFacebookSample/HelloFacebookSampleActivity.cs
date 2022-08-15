using System;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Facebook;
using Xamarin.Facebook.AppEvents;
using Xamarin.Facebook.Login;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

[assembly: Permission(Name = Manifest.Permission.Internet)]
[assembly: Permission(Name = Manifest.Permission.WriteExternalStorage)]
[assembly: MetaData("com.facebook.sdk.ApplicationId", Value = "@string/app_id")]
[assembly: MetaData("com.facebook.sdk.ApplicationName", Value = "@string/app_name")]

namespace HelloFacebookSample
{
    [Activity(Label = "@string/app_name", MainLauncher = true, WindowSoftInputMode = SoftInput.AdjustResize, Exported = true)]
    public class HelloFacebookSampleActivity : Activity
    {
        private ICallbackManager callbackManager;
        private TextView greeting;
        private ProfileTracker profileTracker;
        private static bool initialized;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (!initialized)
            {
                initialized = true;
                FacebookSdk.AddLoggingBehavior(LoggingBehavior.Requests);
                FacebookSdk.AddLoggingBehavior(LoggingBehavior.IncludeRawResponses);
                FacebookSdk.AddLoggingBehavior(LoggingBehavior.Cache);
            }

            callbackManager = CallbackManagerFactory.Create();

            var loginCallback = new FacebookCallback<LoginResult>
            {
                HandleSuccess = loginResult => { UpdateUI(); },
                HandleCancel = () =>
                {
                    ShowAlert(GetString(Resource.String.cancelled), GetString(Resource.String.permission_not_granted));
                    UpdateUI();
                },
                HandleError = loginError =>
                {
                    ShowAlert(GetString(Resource.String.cancelled), GetString(Resource.String.permission_not_granted));
                    UpdateUI();
                }
            };

            LoginManager.Instance.RegisterCallback(callbackManager, loginCallback);

            SetContentView(Resource.Layout.main);

            profileTracker = new CustomProfileTracker
            {
                HandleCurrentProfileChanged = (oldProfile, currentProfile) => { UpdateUI(); }
            };

            greeting = FindViewById<TextView>(Resource.Id.greeting);
        }

        private void ShowAlert(string title, string msg, string buttonText = null)
        {
            new AlertDialog.Builder(Parent)
                .SetTitle(title)
                .SetMessage(msg)
                .SetPositiveButton(buttonText, (s2, e2) => { })
                .Show();
        }

        protected override void OnResume()
        {
            base.OnResume();

            AppEventsLogger.ActivateApp(Application);

            UpdateUI();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            profileTracker.StopTracking();
        }

        private void UpdateUI()
        {
            var enableButtons = AccessToken.CurrentAccessToken != null;
            var profile = Profile.CurrentProfile;

            if (enableButtons && profile != null)
            {
                greeting.Text = GetString(Resource.String.hello_user, new String(profile.FirstName));
            }
            else
            {
                greeting.Text = null;
            }
        }
    }

    internal class FacebookCallback<TResult> : Object, IFacebookCallback where TResult : Object
    {
        public Action HandleCancel { get; set; }
        public Action<FacebookException> HandleError { get; set; }
        public Action<TResult> HandleSuccess { get; set; }

        public void OnCancel()
        {
            HandleCancel?.Invoke();
        }

        public void OnError(FacebookException error)
        {
            HandleError?.Invoke(error);
        }

        public void OnSuccess(Object result)
        {
            HandleSuccess?.Invoke(result.JavaCast<TResult>());
        }
    }

    internal class CustomProfileTracker : ProfileTracker
    {
        public delegate void CurrentProfileChangedDelegate(Profile oldProfile, Profile currentProfile);

        public CurrentProfileChangedDelegate HandleCurrentProfileChanged { get; set; }

        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile currentProfile)
        {
            if (HandleCurrentProfileChanged is { } p)
            {
                p(oldProfile, currentProfile);
            }
        }
    }
}