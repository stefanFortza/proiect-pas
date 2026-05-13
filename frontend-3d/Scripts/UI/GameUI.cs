using Godot;
using System;
using Refit;
using ProiectSimulareLimbaj.Api;
using System.Threading.Tasks;

public partial class GameUI : Control
{
    [ExportGroup("Inputs")]
    [Export] public LineEdit TextInput;
    [Export] public Button SendButton;
    
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
        // Use settings from the singleton (passed from Main Menu)
        if (GameSettings.Instance != null)
        {
            BackendUrl = GameSettings.Instance.BackendUrl;
            ModelName = GameSettings.Instance.SelectedModel;
            Epoch = GameSettings.Instance.SelectedEpoch;
        }

        _api = RestService.For<ISimulationApi>(BackendUrl);

        if (SendButton != null)
        {
            SendButton.Pressed += OnSendButtonPressed;
        }

        if (TextInput != null)
        {
            TextInput.TextSubmitted += (text) => OnSendButtonPressed();
        }
        
        if (ResultsPanel != null) ResultsPanel.Visible = false;
        
        GD.Print($"GameUI: Initialized for {ModelName} (Epoch {Epoch}) at {BackendUrl}");
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
}
