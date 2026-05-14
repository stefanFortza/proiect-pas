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

    [ExportGroup("Game Logic")]
    [Export] public ObjectSpawner Spawner;
    [Export] public string BackendUrl = "http://localhost:8000";
    [Export] public string ModelName = "gpt2_small_base";
    [Export] public int Epoch = 1;

    private ISimulationApi _api;

    public override void _Ready()
    {
        Initialize();

        if (ResultsPanel != null) ResultsPanel.Visible = false;

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
        }
        else
        {
            GD.PushWarning("GameUI: Spawner not assigned!");
        }
    }

    private async void OnSendButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(TextInput?.Text)) return;

        string prompt = TextInput.Text;
        TextInput.Text = "";
        TextInput.Editable = false;
        SendButton.Disabled = true;

        if (StatusLabel != null) StatusLabel.Text = "Waiting for LLM response...";
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

                if (StatusLabel != null) StatusLabel.Text = $"Evaluation complete. Average: {finalScore}%";
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

            // Spawn object based on score (Higher score = heavier object = faster sinking)
            if (Spawner != null)
            {
                Spawner.SpawnByScore(finalScore);
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
