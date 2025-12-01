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

namespace QuizGeograficzny.Views
{
    [QueryProperty(nameof(Difficulty), "difficulty")]
    [QueryProperty(nameof(Mode), "mode")]
    [QueryProperty(nameof(ProfileId), "profileId")]
    public partial class QuizPage : ContentPage
    {
        private List<Question> _questions = new();
        private List<Question> _remaining = new();
        private int _currentIndex = 0;
        private int _score = 0;
        private string _difficulty = "³atwy";
        private string _mode = "normal";
        private string _profileId = string.Empty;
        private int _questionsAnswered = 0;
        private int _correctAnswers = 0;

        private bool _isSurvivalMode => string.Equals(_mode, "survival", StringComparison.OrdinalIgnoreCase);
        private bool _isRankingMode => string.Equals(_mode, "ranking", StringComparison.OrdinalIgnoreCase);

        private int _survivalStreak = 0;

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
                _difficulty = string.IsNullOrWhiteSpace(value) ? "³atwy" : value;
                _ = LoadAndShowAsync();
            }
        }

        public string Mode
        {
            get => _mode;
            set
            {
                _mode = string.IsNullOrWhiteSpace(value) ? "normal" : value;
                _ = LoadAndShowAsync();
            }
        }

        public string ProfileId
        {
            get => _profileId;
            set
            {
                _profileId = value ?? string.Empty;
            }
        }

        private string DifficultyKey()
        {
            try
            {
                return (_difficulty ?? "³atwy").Trim().ToLowerInvariant();
            }
            catch
            {
                return "³atwy";
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
            try
            {
                // Reset per-run counters
                _questionsAnswered = 0;
                _correctAnswers = 0;

                if (_isRankingMode)
                {
                    var all = await QuestionsData.GetAllQuestionsAsync();
                    if (all == null || all.Count == 0)
                    {
                        QuestionLabel.Text = "Brak pytañ.";
                        HideAnswerButtons();
                        return;
                    }

                    var easy = all.Where(q => (q.Difficulty ?? "").Trim().ToLowerInvariant() == "³atwy").ToList();
                    var medium = all.Where(q => (q.Difficulty ?? "").Trim().ToLowerInvariant() == "œredni").ToList();
                    var hard = all.Where(q =>
                    {
                        var d = (q.Difficulty ?? "").Trim().ToLowerInvariant();
                        return d == "trudny" || d == "bardzo trudny";
                    }).ToList();

                    ShuffleList(easy); ShuffleList(medium); ShuffleList(hard);

                    var selected = new List<Question>();
                    selected.AddRange(easy.Take(3));
                    selected.AddRange(medium.Take(3));
                    selected.AddRange(hard.Take(4));

                    var remaining = all.Except(selected).OrderBy(_ => Guid.NewGuid()).ToList();
                    while (selected.Count < 10 && remaining.Count > 0)
                    {
                        selected.Add(remaining[0]);
                        remaining.RemoveAt(0);
                    }

                    _questions = selected.OrderBy(_ => Guid.NewGuid()).ToList();
                    _currentIndex = 0;
                    _score = 0;
                    _survivalStreak = 0;
                }
                else if (_isSurvivalMode)
                {
                    var all = await QuestionsData.GetAllQuestionsAsync();
                    _questions = all.OrderBy(_ => Guid.NewGuid()).ToList();

                    _remaining = _questions.ToList();
                    ShuffleList(_remaining);
                    _survivalStreak = 0;
                    _score = 0;
                    _currentIndex = 0;
                }
                else
                {
                    var filteredList = await QuestionsData.GetByDifficultyAsync(DifficultyKey());
                    if (filteredList == null || filteredList.Count == 0)
                    {
                        filteredList = await QuestionsData.GetAllQuestionsAsync();
                    }

                    _questions = filteredList.OrderBy(_ => Guid.NewGuid()).Take(10).ToList();
                    _currentIndex = 0;
                    _score = 0;
                }

                DisplayQuestion();
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadAndShowAsync error: " + ex);
                _questions = await QuestionsData.GetAllQuestionsAsync();
                ShuffleList(_questions);
                DisplayQuestion();
            }
        }

        private static void ShuffleList<T>(List<T> list)
        {
            var rnd = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void DisplayQuestion()
        {
            StopTimer();

            Question q = null;

            if (_isSurvivalMode)
            {
                if (_remaining == null || _remaining.Count == 0)
                {
                    _remaining = _questions.ToList();
                    ShuffleList(_remaining);
                }

                if (_remaining == null || _remaining.Count == 0)
                {
                    QuestionLabel.Text = "Brak pytañ.";
                    ProgressLabel.Text = "";
                    HideAnswerButtons();
                    return;
                }

                q = _remaining[0];
            }
            else
            {
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

                q = _questions[_currentIndex];
            }

            if (_isSurvivalMode)
                ProgressLabel.Text = $"Survival: {_survivalStreak} pkt";
            else if (_isRankingMode)
                ProgressLabel.Text = $"Ranking: {_currentIndex + 1} / {_questions.Count}";
            else
                ProgressLabel.Text = $"Pytanie {_currentIndex + 1} / {_questions.Count}";

            QuestionLabel.Text = q?.QuestionText ?? "";

            AnswerButton1.Text = q?.Answers.ElementAtOrDefault(0) ?? "";
            AnswerButton2.Text = q?.Answers.ElementAtOrDefault(1) ?? "";
            AnswerButton3.Text = q?.Answers.ElementAtOrDefault(2) ?? "";
            AnswerButton4.Text = q?.Answers.ElementAtOrDefault(3) ?? "";

            ResetAnswerButtons();

            if (!string.IsNullOrEmpty(q?.ImageUrl))
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

#if WINDOWS
            QuestionLabel.TextColor = Colors.Black;
            AnswerButton1.TextColor = Colors.Black;
            AnswerButton2.TextColor = Colors.Black;
            AnswerButton3.TextColor = Colors.Black;
            AnswerButton4.TextColor = Colors.Black;
#endif
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
            if (_isSurvivalMode)
            {
                TryVibrateShort(120);
                await AnimatePulseAsync(QuestionTimerProgressBar, Colors.Red);
                await DisplayAlert("Czas min¹³", "Koniec trybu Survival.", "OK");
                await EndQuizAsync();
                return;
            }

            int minus = DifficultyKey() switch
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

            ProgressLabel.Text += " — potwierdŸ, aby przejœæ dalej";
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

                if (_isSurvivalMode)
                {
                    if (_remaining == null || _remaining.Count == 0)
                    {
                        await EndQuizAsync();
                        return;
                    }
                }
                else
                {
                    if (_questions == null || _questions.Count == 0) return;
                    if (_currentIndex >= _questions.Count) return;
                }

                StopTimer();

                AnswerButton1.IsEnabled = AnswerButton2.IsEnabled = AnswerButton3.IsEnabled = AnswerButton4.IsEnabled = false;

                Question q = _isSurvivalMode ? _remaining[0] : _questions[_currentIndex];

                int chosenIndex = btn == AnswerButton1 ? 0 :
                         btn == AnswerButton2 ? 1 :
                         btn == AnswerButton3 ? 2 : 3;

                int plus = DifficultyKey() switch
                {
                    "³atwy" => 1,
                    "œredni" => 2,
                    "trudny" => 3,
                    "bardzo trudny" => 5,
                    _ => 1
                };

                int minus = _isSurvivalMode ? 0 : (DifficultyKey() switch
                {
                    "³atwy" => 2,
                    "œredni" => 1,
                    _ => 0
                });

                bool correct = chosenIndex == q.CorrectAnswerIndex;
                _questionsAnswered++;
                if (correct) _correctAnswers++;
                btn.BackgroundColor = correct ? Colors.LightGreen : Colors.IndianRed;
                TryVibrateShort(correct ? 40 : 160);
                await AnimatePulseAsync(QuestionTimerProgressBar, correct ? Colors.Green : Colors.Red);

                if (correct)
                {
                    if (_isRankingMode)
                        _score += 1;
                    else
                        _score += plus;
                }
                else
                {
                    if (!_isSurvivalMode)
                    {
                     
                        if (_isRankingMode)
                            _score -= 1;
                        else
                            _score -= minus;
                    }
                }

                string pointsMsg = correct ? $"+{(_isRankingMode ? 1 : plus)}" : (minus > 0 || _isRankingMode ? $"-{(_isRankingMode ? 1 : minus)}" : "0");
                Color pointsColor = correct ? Colors.LimeGreen : Colors.IndianRed;
                await ShowFloatingPointsLabel(pointsMsg, pointsColor);

                if (_isSurvivalMode)
                {
                    if (correct)
                    {
                        _survivalStreak++;

                        if (_remaining.Count > 0)
                            _remaining.RemoveAt(0);

                        ProgressLabel.Text = $"Survival: {_survivalStreak} pkt";
                        DisplayQuestion();
                    }
                    else
                    {
                        await EndQuizAsync();
                    }
                }
                else
                {
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
                        if (_isSurvivalMode)
                        {
                            return;
                        }

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
                
                var stats = StatsService.GetStats();
                string defaultName = !string.IsNullOrWhiteSpace(stats.PlayerName) ? stats.PlayerName : "Imie";

              
                if (!string.IsNullOrWhiteSpace(_profileId))
                    defaultName = _profileId;

                string name = await DisplayPromptAsync("Koniec quizu", "Podaj nick do tablicy wyników (Anuluj aby pomin¹æ):", "Zapisz", "Pomiñ", defaultName, -1, Keyboard.Text);

                string playerNameToUse = defaultName;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    playerNameToUse = name.Trim();

            
                    var s = StatsService.GetStats();
                    s.PlayerName = playerNameToUse;
                    StatsService.SaveStats(s);
                }

                if (!string.IsNullOrWhiteSpace(playerNameToUse))
                {
                    var entry = new ScoreEntry
                    {
                        PlayerName = playerNameToUse,
                        Score = _score,
                        Date = DateTime.UtcNow,
                        Difficulty = _isSurvivalMode ? "survival" : (_isRankingMode ? "ranking" : DifficultyKey())
                    };

                 
                    await ScoreboardService.AddAsync(entry);
                }

                await StatsService.RecordGameAsync(_score, _correctAnswers, _questionsAnswered);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EndQuizAsync error: " + ex);
            }

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
}
