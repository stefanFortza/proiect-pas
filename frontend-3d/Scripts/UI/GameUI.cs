using Godot;
using System;
using Refit;
using ProiectSimulareLimbaj.Api;
using System.Threading.Tasks;
using System.Net.Http;

public partial class GameUI : Control
{
    [ExportGroup("Inputs")]
    [Export] public LineEdit TextInput;
    [Export] public Button SendButton;
    [Export] public Button DebugSpawnButton;

    [ExportGroup("Results Display")]
    [Export] public RichTextLabel BotResponseLabel;
    [Export] public Label GrammarScoreLabel;
    [Export] public Label SyntaxScoreLabel;
    [Export] public Label MeaningScoreLabel;
    [Export] public Label RelevanceScoreLabel;
    [Export] public Label StatusLabel;
    [Export] public Control ResultsPanel;
    [Export] public Label SpawnNotificationLabel;

    [ExportGroup("Game Logic")]
    [Export] public ObjectSpawner Spawner;
    [Export] public string BackendUrl = "https://tvnue234s9plaq-8000.proxy.runpod.net/";
    [Export] public string ModelName = "gpt2_small_base";
    [Export] public int Epoch = 1;
    [Export] public int LowScoreThreshold = 50;

    private ISimulationApi _api;
    private Tween _notificationTween;

    public override void _Ready()
    {
        Initialize();

        if (ResultsPanel != null) ResultsPanel.Visible = false;
        if (SpawnNotificationLabel != null) 
        {
            SpawnNotificationLabel.Modulate = new Color(1, 1, 1, 0); // Start invisible
            SpawnNotificationLabel.Text = "";
        }

        GD.Print("GameUI: Ready.");
    }

    public void Initialize()
    {
        // Use settings from the singleton (passed from Main Menu)
        if (GameSettings.Instance != null)
        {
            BackendUrl = GameSettings.Instance.BackendUrl;
            ModelName = GameSettings.Instance.SelectedModel;
            Epoch = GameSettings.Instance.SelectedEpoch;
        }

        var httpClient = new System.Net.Http.HttpClient
        {
            BaseAddress = new Uri(BackendUrl),
            Timeout = TimeSpan.FromMinutes(10)
        };

        _api = RestService.For<ISimulationApi>(httpClient);

        if (SendButton != null && !SendButton.IsConnected(Button.SignalName.Pressed, Callable.From(OnSendButtonPressed)))
        {
            SendButton.Pressed += OnSendButtonPressed;
        }

        if (DebugSpawnButton != null && !DebugSpawnButton.IsConnected(Button.SignalName.Pressed, Callable.From(OnDebugSpawnPressed)))
        {
            DebugSpawnButton.Pressed += OnDebugSpawnPressed;
        }

        if (TextInput != null && !TextInput.IsConnected(LineEdit.SignalName.TextSubmitted, Callable.From<string>((text) => OnSendButtonPressed())))
        {
            TextInput.TextSubmitted += (text) => OnSendButtonPressed();
        }

        GD.Print($"GameUI: Initialized for {ModelName} (Epoch {Epoch}) at {BackendUrl}");
    }

    private void OnDebugSpawnPressed()
    {
        if (Spawner != null)
        {
            GD.Print("GameUI: Debug spawn button pressed.");
            Spawner.SpawnRandom();
            ShowSpawnNotification("Debug: Obiect spawnat!", new Color(1, 1, 0)); // Yellow for debug
        }
        else
        {
            GD.PushWarning("GameUI: Spawner not assigned!");
        }
    }

    private void ShowSpawnNotification(string message, Color color)
    {
        if (SpawnNotificationLabel == null) return;

        if (_notificationTween != null && _notificationTween.IsRunning())
        {
            _notificationTween.Kill();
        }

        SpawnNotificationLabel.Text = message;
        SpawnNotificationLabel.Modulate = color;
        SpawnNotificationLabel.SelfModulate = new Color(color.R, color.G, color.B, 1);
        
        _notificationTween = CreateTween();
        _notificationTween.TweenProperty(SpawnNotificationLabel, "modulate:a", 1.0f, 0.5f);
        _notificationTween.Chain().TweenProperty(SpawnNotificationLabel, "modulate:a", 0.0f, 2.0f).SetDelay(1.5f);
    }

    private async void OnSendButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(TextInput?.Text)) return;

        string prompt = TextInput.Text;
        TextInput.Text = "";
        TextInput.Editable = false;
        SendButton.Disabled = true;

        if (StatusLabel != null) 
        {
            StatusLabel.Text = "Waiting for LLM response...";
            StatusLabel.SelfModulate = new Color(1, 1, 1); // Reset color
        }
        if (ResultsPanel != null) ResultsPanel.Visible = true;

        try
        {
            var request = new GenerateRequest
            {
                Prompt = prompt,
                ModelName = ModelName,
                Epoch = Epoch,
                Temperature = 0.7f,
                MaxTokens = 60
            };

            var response = await _api.GenerateAsync(request);

            // Update UI with bot response
            if (BotResponseLabel != null)
            {
                BotResponseLabel.Text = $"[b]Bot:[/b] {response.Response}";
            }

            int finalScore = 0;
            if (response.EvaluationScores != null && string.IsNullOrEmpty(response.EvaluationScores.Error))
            {
                var scores = response.EvaluationScores;

                // Update Score Labels
                if (GrammarScoreLabel != null) GrammarScoreLabel.Text = $"Grammar: {scores.GrammaticalStructure}%";
                if (SyntaxScoreLabel != null) SyntaxScoreLabel.Text = $"Syntax: {scores.SyntacticalStructure}%";
                if (MeaningScoreLabel != null) MeaningScoreLabel.Text = $"Meaning: {scores.Meaning}%";
                if (RelevanceScoreLabel != null) RelevanceScoreLabel.Text = $"Relevance: {scores.Relevance}%";

                // Calculate average score for spawning weight
                finalScore = (scores.GrammaticalStructure +
                              scores.SyntacticalStructure +
                              scores.Meaning +
                              scores.Relevance) / 4;

                if (StatusLabel != null) 
                {
                    StatusLabel.Text = $"Evaluation complete. Average: {finalScore}%";
                    StatusLabel.SelfModulate = finalScore < LowScoreThreshold ? new Color(1, 0.3f, 0.3f) : new Color(0.3f, 1, 0.3f);
                }
            }
            else
            {
                GD.PushWarning("GameUI: Evaluation data missing.");
                if (StatusLabel != null) StatusLabel.Text = "Evaluation failed (Gemini error).";
                finalScore = 10;

                if (GrammarScoreLabel != null) GrammarScoreLabel.Text = "Grammar: N/A";
                if (SyntaxScoreLabel != null) SyntaxScoreLabel.Text = "Syntax: N/A";
                if (MeaningScoreLabel != null) MeaningScoreLabel.Text = "Meaning: N/A";
                if (RelevanceScoreLabel != null) RelevanceScoreLabel.Text = "Relevance: N/A";
            }

            // Spawn object based on score (Lower score = heavier object = faster sinking)
            if (Spawner != null)
            {
                // Invert score for spawning: Low score -> Heavier object
                int spawnScore = 100 - finalScore;
                Spawner.SpawnByScore(spawnScore);

                if (finalScore < LowScoreThreshold)
                {
                    ShowSpawnNotification("Scor mic! Obiect greu spawnat!", new Color(1, 0.2f, 0.2f));
                }
                else
                {
                    ShowSpawnNotification("Scor bun! Obiect ușor spawnat.", new Color(0.2f, 1, 0.2f));
                }
            }
        }
        catch (Exception e)
        {
            GD.PushError($"GameUI: Error calling backend: {e.Message}");
            if (StatusLabel != null) StatusLabel.Text = "Error: " + e.Message;
        }
        finally
        {
            TextInput.Editable = true;
            SendButton.Disabled = false;
            TextInput.GrabFocus();
        }
    }

    public void ResetUI()
    {
        if (TextInput != null) TextInput.Text = "";
        if (BotResponseLabel != null) BotResponseLabel.Text = "";
        if (ResultsPanel != null) ResultsPanel.Visible = false;
        if (StatusLabel != null) StatusLabel.Text = "Enter text to start...";

        if (GrammarScoreLabel != null) GrammarScoreLabel.Text = "Grammar: -";
        if (SyntaxScoreLabel != null) SyntaxScoreLabel.Text = "Syntax: -";
        if (MeaningScoreLabel != null) MeaningScoreLabel.Text = "Meaning: -";
        if (RelevanceScoreLabel != null) RelevanceScoreLabel.Text = "Relevance: -";
    }
}
