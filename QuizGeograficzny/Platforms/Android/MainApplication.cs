using Android.App;
using Android.Runtime;
using Plugin.Fingerprint;

namespace QuizGeograficzny
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnCreate()
        {
            base.OnCreate();
           
            CrossFingerprint.SetCurrentActivityResolver(() => Platform.CurrentActivity);
        }
    }
}