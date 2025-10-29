using QuizGeograficzny.Data;
using QuizGeograficzny.Models;
using QuizGeograficzny.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

namespace QuizGeograficzny.Views;

[QueryProperty(nameof(Difficulty), "difficulty")]
public partial class QuizPage : ContentPage
{
    private List<Question> _questions = new();
    private int _currentIndex = 0;
    private int _score = 0;
    private string _difficulty = "≥atwy";

    private bool _canAdvanceByShake = false;
    private double _shakeThreshold = 2.2;
    private DateTime _lastShakeTime = DateTime.MinValue;
    private TimeSpan _shakeCooldown = TimeSpan.FromMilliseconds(800);

    public QuizPage()
    {
        InitializeComponent();
    }

    public string Difficulty
    {
        get => _difficulty;
        set
        {
            _difficulty = value ?? "≥atwy";
            _ = LoadAndShowAsync();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Accelerometer.Default.IsSupported)
        {
            try
            {
                Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

                if (!Accelerometer.IsMonitoring)
                    Accelerometer.Start(SensorSpeed.UI);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B≥πd akcelerometru: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Akcelerometr nie jest dostÍpny na tym urzπdzeniu.");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (Accelerometer.Default.IsSupported)
        {
            try
            {
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;

                if (Accelerometer.IsMonitoring)
                    Accelerometer.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B≥πd zatrzymania akcelerometru: {ex.Message}");
            }
        }
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading;
        double magnitude = Math.Sqrt(a.Acceleration.X * a.Acceleration.X
                                     + a.Acceleration.Y * a.Acceleration.Y
                                     + a.Acceleration.Z * a.Acceleration.Z);

        if (magnitude >= _shakeThreshold)
        {
            var now = DateTime.UtcNow;
            if (now - _lastShakeTime > _shakeCooldown)
            {
                _lastShakeTime = now;
                if (_canAdvanceByShake)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _canAdvanceByShake = false;
                        AdvanceAfterShake();
                    });
                }
            }
        }
    }

    private void AdvanceAfterShake()
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
    }

    private async Task LoadAndShowAsync()
    {
        await LoadQuestionsAsync();
        DisplayQuestion();
    }

    private async Task LoadQuestionsAsync()
    {
        var list = await QuestionsData.GetByDifficultyAsync(_difficulty);
        if (list == null || list.Count == 0)
        {
            _questions = await QuestionsData.GetAllQuestionsAsync();
        }
        else
        {
            _questions = list;
        }

        _questions = TakeRandom(_questions, 10);
        _currentIndex = 0;
        _score = 0;
    }

    private static List<T> TakeRandom<T>(List<T> source, int count)
    {
        var rnd = new Random();
        var list = new List<T>(source);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list.Take(Math.Min(count, list.Count)).ToList();
    }

    private void DisplayQuestion()
    {
        AnswerButton1.IsEnabled = AnswerButton2.IsEnabled = AnswerButton3.IsEnabled = AnswerButton4.IsEnabled = true;
        _canAdvanceByShake = false;

        if (_questions == null || _questions.Count == 0)
        {
            QuestionLabel.Text = "Brak pytaÒ.";
            ProgressLabel.Text = "";
            AnswerButton1.IsVisible = AnswerButton2.IsVisible = AnswerButton3.IsVisible = AnswerButton4.IsVisible = false;
            return;
        }

        if (_currentIndex >= _questions.Count)
        {
            _ = EndQuizAsync();
            return;
        }

        var q = _questions[_currentIndex];

        ProgressLabel.Text = $"Pytanie {_currentIndex + 1} / {_questions.Count}";
        QuestionLabel.Text = q.QuestionText;

        AnswerButton1.Text = q.Answers.Length > 0 ? q.Answers[0] : "";
        AnswerButton2.Text = q.Answers.Length > 1 ? q.Answers[1] : "";
        AnswerButton3.Text = q.Answers.Length > 2 ? q.Answers[2] : "";
        AnswerButton4.Text = q.Answers.Length > 3 ? q.Answers[3] : "";

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
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (_questions == null || _questions.Count == 0) return;
        if (_currentIndex >= _questions.Count)
        {
            await EndQuizAsync();
            return;
        }

        AnswerButton1.IsEnabled = AnswerButton2.IsEnabled = AnswerButton3.IsEnabled = AnswerButton4.IsEnabled = false;

        var q = _questions[_currentIndex];

        int chosenIndex = btn == AnswerButton1 ? 0 :
                          btn == AnswerButton2 ? 1 :
                          btn == AnswerButton3 ? 2 : 3;

        int plus = _difficulty.ToLower() switch
        {
            "≥atwy" => 1,
            "úredni" => 2,
            "trudny" => 3,
            "bardzo trudny" => 5,
            _ => 1
        };
        int minus = _difficulty.ToLower() switch
        {
            "≥atwy" => 2,
            "úredni" => 1,
            _ => 0
        };

        if (chosenIndex == q.CorrectAnswerIndex)
        {
            _score += plus;
            await DisplayAlert("Poprawnie", $"+{plus} pkt", "OK");
        }
        else
        {
            _score -= minus;
            if (minus > 0)
                await DisplayAlert("èle", $"-{minus} pkt", "OK");
            else
                await DisplayAlert("èle", "0 pkt", "OK");
        }

        _canAdvanceByShake = true;
        ProgressLabel.Text += " ó potrzπúnij, aby przejúÊ dalej";
    }

    private async Task EndQuizAsync()
    {
        if (_score < 0) _score = 0;

        try
        {
            string defaultName = "Anon";
            string name = await DisplayPromptAsync("Koniec quizu", "Podaj nick do tablicy wynikÛw (Anuluj aby pominπÊ):", "Zapisz", "PomiÒ", defaultName, -1, Keyboard.Text);

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
        catch
        {
        }

        await Shell.Current.GoToAsync($"///result?score={_score}", animate: true);
    }
}
