using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using QuizGeograficzny.Data;
using QuizGeograficzny.Models;
using QuizGeograficzny.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Dispatching;
using STimer = System.Timers.Timer;

namespace QuizGeograficzny.Views;

[QueryProperty(nameof(Difficulty), "difficulty")]
public partial class QuizPage : ContentPage
{
    private List<Question> _questions = new();
    private int _currentIndex = 0;
    private int _score = 0;
    private string _difficulty = "³atwy";


    private const int QuestionTimeSeconds = 15;
    private int _timeLeft = QuestionTimeSeconds;
    private STimer? _timer;


    private double _lastX, _lastY, _lastZ;
    private DateTime _lastShakeTime = DateTime.MinValue;
    private readonly double _shakeThreshold = 1.7;

    private int _isProcessingAnswer = 0;

    public QuizPage()
    {
        InitializeComponent();
    }

    public string Difficulty
    {
        get => _difficulty;
        set
        {
            _difficulty = value ?? "³atwy";
            _ = LoadAndShowAsync();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnShakeDetected;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("B³¹d startu akcelerometru: " + ex.Message);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged -= OnShakeDetected;
                if (Accelerometer.Default.IsMonitoring)
                    Accelerometer.Default.Stop();
            }
        }
        catch { }

        StopTimer();
    }

    private async Task LoadAndShowAsync()
    {
        var all = await QuestionsData.GetAllQuestionsAsync();

        _questions = all.OrderBy(_ => Guid.NewGuid()).Take(10).ToList();
        _currentIndex = 0;
        _score = 0;
        DisplayQuestion();
    }

    private void DisplayQuestion()
    {
        StopTimer();

        if (_questions == null || _questions.Count == 0)
        {
            QuestionLabel.Text = "Brak pytañ.";
            ProgressLabel.Text = "";
            HideAnswerButtons();
            return;
        }

        if (_currentIndex >= _questions.Count)
        {
            _ = EndQuizAsync();
            return;
        }

        var q = _questions[_currentIndex];

        ProgressLabel.Text = $"Pytanie {_currentIndex + 1} / {_questions.Count}";
        QuestionLabel.Text = q.QuestionText ?? "";

        AnswerButton1.Text = q.Answers.ElementAtOrDefault(0) ?? "";
        AnswerButton2.Text = q.Answers.ElementAtOrDefault(1) ?? "";
        AnswerButton3.Text = q.Answers.ElementAtOrDefault(2) ?? "";
        AnswerButton4.Text = q.Answers.ElementAtOrDefault(3) ?? "";

        ResetAnswerButtons();

        if (!string.IsNullOrEmpty(q.ImageUrl))
        {
            QuestionImage.IsVisible = true;
            QuestionImage.Source = q.ImageUrl;
        }
        else
        {
            QuestionImage.IsVisible = false;
            QuestionImage.Source = null;
        }

        StartTimer();
        AnimateFadeIn(this);
    }

    private void ResetAnswerButtons()
    {
        var buttons = new[] { AnswerButton1, AnswerButton2, AnswerButton3, AnswerButton4 };
        foreach (var b in buttons)
        {
            if (b is null) continue;
            b.BackgroundColor = Color.FromArgb("#F0F0F0");
            b.TextColor = Colors.Black;
            b.IsEnabled = true;
            b.IsVisible = true;
        }
    }

    private void HideAnswerButtons()
    {
        AnswerButton1.IsVisible = AnswerButton2.IsVisible = AnswerButton3.IsVisible = AnswerButton4.IsVisible = false;
    }

    private void StartTimer()
    {
        StopTimer();

        _timeLeft = QuestionTimeSeconds;
        QuestionTimerLabel.Text = $"{_timeLeft}s";
        QuestionTimerProgressBar.Progress = 1;
        TrySetTimerColorByTime(_timeLeft);

        _timer = new STimer(1000);
        _timer.AutoReset = true;
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Dispose();
            _timer = null;
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        _timeLeft--;

   
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Interlocked.Exchange(ref _isProcessingAnswer, 1) == 1)
                return; 

            try
            {
                if (_timeLeft < 0) _timeLeft = 0;
                QuestionTimerLabel.Text = $"{_timeLeft}s";
                QuestionTimerProgressBar.Progress = (double)_timeLeft / QuestionTimeSeconds;

                TrySetTimerColorByTime(_timeLeft);

                if (_timeLeft <= 0)
                {
                    StopTimer();
                    await HandleTimeoutAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Timer_Elapsed error: " + ex);
            }
            finally
            {
                Interlocked.Exchange(ref _isProcessingAnswer, 0);
            }
        });
    }

    private void TrySetTimerColorByTime(int timeLeft)
    {
        try
        {
            if (timeLeft <= 5)
                QuestionTimerProgressBar.ProgressColor = Colors.Red;
            else if (timeLeft <= 10)
                QuestionTimerProgressBar.ProgressColor = Colors.Orange;
            else
                QuestionTimerProgressBar.ProgressColor = Colors.Green;
        }
        catch
        {

        }
    }

    private async Task HandleTimeoutAsync()
    {
        int minus = _difficulty.ToLower() switch
        {
            "³atwy" => 2,
            "œredni" => 1,
            _ => 0
        };

        if (minus > 0)
        {
            _score -= minus;
            TryVibrateShort(100);
            await AnimatePulseAsync(QuestionTimerProgressBar, Colors.Red);
            await DisplayAlert("Czas min¹³", $"-{minus} pkt (brak odpowiedzi)", "OK");
        }
        else
        {
            TryVibrateShort(80);
            await AnimatePulseAsync(QuestionTimerProgressBar, Colors.Red);
            await DisplayAlert("Czas min¹³", "Brak punktów (brak odpowiedzi)", "OK");
        }

     
        if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.Android)
        {
            _currentIndex++;
            DisplayQuestion();
            return;
        }

        ProgressLabel.Text += " — potrz¹œnij, aby przejœæ dalej";
    }

    private void TryVibrateShort(int ms)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(ms));
        }
        catch { }
    }


    private async void OnAnswerClicked(object sender, EventArgs e)
    {
      
        if (Interlocked.Exchange(ref _isProcessingAnswer, 1) == 1)
            return;

        try
        {
            if (sender is not Button btn) return;
            if (_questions == null || _questions.Count == 0) return;
            if (_currentIndex >= _questions.Count) return;

       
            StopTimer();
            AnswerButton1.IsEnabled = AnswerButton2.IsEnabled = AnswerButton3.IsEnabled = AnswerButton4.IsEnabled = false;

            var q = _questions[_currentIndex];

            int chosenIndex = btn == AnswerButton1 ? 0 :
                              btn == AnswerButton2 ? 1 :
                              btn == AnswerButton3 ? 2 : 3;

            int plus = _difficulty.ToLower() switch
            {
                "³atwy" => 1,
                "œredni" => 2,
                "trudny" => 3,
                "bardzo trudny" => 5,
                _ => 1
            };
            int minus = _difficulty.ToLower() switch
            {
                "³atwy" => 2,
                "œredni" => 1,
                _ => 0
            };

            bool correct = chosenIndex == q.CorrectAnswerIndex;

   
            btn.BackgroundColor = correct ? Colors.LightGreen : Colors.IndianRed;
            TryVibrateShort(correct ? 40 : 160);
            await AnimatePulseAsync(QuestionTimerProgressBar, correct ? Colors.Green : Colors.Red);

  
            if (correct)
                _score += plus;
            else
                _score -= minus;

         
            string pointsMsg = correct ? $"+{plus}" : (minus > 0 ? $"-{minus}" : "0");
            Color pointsColor = correct ? Colors.LimeGreen : Colors.IndianRed;
            await ShowFloatingPointsLabel(pointsMsg, pointsColor);

            
            if (_currentIndex < _questions.Count - 1)
            {
                _currentIndex++;
                DisplayQuestion();
            }
            else
            {
                await EndQuizAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("B³¹d w OnAnswerClicked: " + ex);
        }
        finally
        {
           
            Interlocked.Exchange(ref _isProcessingAnswer, 0);
        }
    }


    private void OnShakeDetected(object? sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading;
        double deltaX = data.Acceleration.X - _lastX;
        double deltaY = data.Acceleration.Y - _lastY;
        double deltaZ = data.Acceleration.Z - _lastZ;

        double acceleration = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

        _lastX = data.Acceleration.X;
        _lastY = data.Acceleration.Y;
        _lastZ = data.Acceleration.Z;

        if (acceleration > _shakeThreshold && (DateTime.UtcNow - _lastShakeTime).TotalMilliseconds > 800)
        {
        
            if (Interlocked.Exchange(ref _isProcessingAnswer, 1) == 1)
                return;

            _lastShakeTime = DateTime.UtcNow;

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_currentIndex < _questions.Count - 1)
                    {
                        _currentIndex++;
                        DisplayQuestion();
                    }
                    else
                    {
                        _ = EndQuizAsync();
                    }
                });
            }
            finally
            {
         
                Interlocked.Exchange(ref _isProcessingAnswer, 0);
            }
        }
    }

    private async Task EndQuizAsync()
    {
        StopTimer();

        if (_score < 0) _score = 0;

        try
        {
            string defaultName = "Imie";
            string name = await DisplayPromptAsync("Koniec quizu", "Podaj nick do tablicy wyników (Anuluj aby pomin¹æ):", "Zapisz", "Pomiñ", defaultName, -1, Keyboard.Text);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var entry = new ScoreEntry
                {
                    PlayerName = name,
                    Score = _score,
                    Date = DateTime.UtcNow,
                    Difficulty = _difficulty
                };

                await ScoreboardService.AddAsync(entry);
            }
        }
        catch { }

        await Shell.Current.GoToAsync($"///result?score={_score}", animate: true);
    }

    private async Task AnimatePulseAsync(VisualElement element, Color flashColor)
    {
        if (element == null) return;

        try
        {
            try { QuestionTimerProgressBar.ProgressColor = flashColor; } catch { }

            await element.ScaleTo(1.06, 120, Easing.CubicInOut);
            await Task.Delay(80);
            await element.ScaleTo(1.0, 120, Easing.CubicOut);

            TrySetTimerColorByTime(_timeLeft);
        }
        catch { }
    }

  
    private async Task ShowFloatingPointsLabel(string text, Color color)
    {
        var label = new Label
        {
            Text = text + " pkt",
            TextColor = color,
            FontSize = 36,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Opacity = 0
        };

        if (Content is Layout layout)
            layout.Children.Add(label);

  
        await label.FadeTo(1, 200);
        await label.ScaleTo(1.2, 300, Easing.CubicOut);
        await Task.Delay(400);
        await label.FadeTo(0, 300);
        await label.ScaleTo(0.8, 300, Easing.CubicIn);

        if (Content is Layout layout2)
            layout2.Children.Remove(label);
    }

    private static async void AnimateFadeIn(VisualElement element)
    {
        try
        {
            element.Opacity = 0;
            await element.FadeTo(1, 400, Easing.CubicInOut);
        }
        catch { }
    }

    private static async void AnimateFadeOut(VisualElement element, Action onComplete)
    {
        try
        {
            await element.FadeTo(0, 240, Easing.CubicInOut);
            onComplete?.Invoke();
            await element.FadeTo(1, 240, Easing.CubicInOut);
        }
        catch
        {
            onComplete?.Invoke();
        }
    }
}
