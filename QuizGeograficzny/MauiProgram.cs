using Microsoft.Extensions.Logging;
using QuizGeograficzny.Services;
using QuizGeograficzny.ViewModels;
using QuizGeograficzny.Views;

#if ANDROID
using QuizGeograficzny.Platforms.Android.Services;
#endif

namespace QuizGeograficzny
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<ChatService>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ChatPage>();

#if ANDROID
            builder.Services.AddSingleton<ILightSensorService, AndroidLightSensorService>();
#else
            builder.Services.AddSingleton<ILightSensorService, DefaultLightSensorService>();
#endif

            return builder.Build();
        }
    }
}