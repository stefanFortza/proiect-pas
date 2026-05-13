using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{
    [Export] public OptionButton ModelDropdown;
    [Export] public SpinBox EpochSpinner;
    [Export] public Button PlayButton;
    [Export] public LineEdit BackendUrlInput;
    [Export(PropertyHint.File, "*.tscn")] public string WorldScenePath = "res://Scenes/World.tscn";

    private readonly List<string> _models = new List<string>
    {
        "gpt2_small_base",
        "gpt2_small_full",
        "gpt2_small_replay",
        "pythia_1.4b",
        "gpt2_large_frozen",
        "gpt2_large_stable"
    };

    public override void _Ready()
    {
        if (ModelDropdown != null)
        {
            ModelDropdown.Clear();
            foreach (var model in _models)
            {
                ModelDropdown.AddItem(model);
            }
        }

        if (BackendUrlInput != null)
        {
            BackendUrlInput.Text = GameSettings.Instance.BackendUrl;
        }

        if (PlayButton != null)
        {
            PlayButton.Pressed += OnPlayButtonPressed;
        }
    }

    private void OnPlayButtonPressed()
    {
        if (GameSettings.Instance == null)
        {
            GD.PushError("GameSettings singleton not found! Make sure it's added to Autoloads.");
            return;
        }

        GameSettings.Instance.SelectedModel = _models[ModelDropdown.Selected];
        GameSettings.Instance.SelectedEpoch = (int)EpochSpinner.Value;
        GameSettings.Instance.BackendUrl = BackendUrlInput.Text;

        GD.Print($"MainMenu: Starting game with {GameSettings.Instance.SelectedModel} at epoch {GameSettings.Instance.SelectedEpoch}");
        GetTree().ChangeSceneToFile(WorldScenePath);
    }
}
