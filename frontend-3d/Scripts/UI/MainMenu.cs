using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{
    [ExportGroup("UI References")]
    [Export] public OptionButton ModelDropdown;
    [Export] public SpinBox EpochSpinner;
    [Export] public Button PlayButton;
    [Export] public LineEdit BackendUrlInput;
    [Export] public GameUIManager UIManager;
    [Export] public GameUI GameUIScript;

    [ExportGroup("Camera Transition")]
    [Export] public Node PCamMenu; 
    [Export] public Node PCamGame;
    [Export] public Camera3D MainCamera;

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
        GD.Print("MainMenu: Ready called");
        GD.Print($"MainMenu: PCamMenu={PCamMenu}, PCamGame={PCamGame}, MainCamera={MainCamera}");

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

        // Initialize state: Menu camera active
        if (PCamMenu != null) PCamMenu.Set("priority", 10);
        if (PCamGame != null) PCamGame.Set("priority", 0);
        
        // Disable free cam script initially
        if (MainCamera != null) MainCamera.SetProcess(false);
    }

    public void ResetToMenu()
    {
        Visible = true;
        // Transition cameras back
        if (PCamMenu != null) PCamMenu.Set("priority", 20);
        if (PCamGame != null) PCamGame.Set("priority", 0);
        
        // Disable camera control
        if (MainCamera != null) MainCamera.SetProcess(false);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        
        GD.Print("MainMenu: Reset to menu state");
    }

    private void OnPlayButtonPressed()
    {
        if (GameSettings.Instance == null) return;

        // Save settings
        GameSettings.Instance.SelectedModel = _models[ModelDropdown.Selected];
        GameSettings.Instance.SelectedEpoch = (int)EpochSpinner.Value;
        GameSettings.Instance.BackendUrl = BackendUrlInput.Text;

        GD.Print($"MainMenu: Transitioning to game view with {GameSettings.Instance.SelectedModel}");

        // Transition cameras - GDScript property is 'priority'
        if (PCamMenu != null) 
        {
            PCamMenu.Set("priority", 0);
            GD.Print("MainMenu: PCamMenu priority set to 0");
        }
        if (PCamGame != null) 
        {
            PCamGame.Set("priority", 20);
            GD.Print("MainMenu: PCamGame priority set to 20");
        }

        // UI Swap via Manager
        if (UIManager != null)
        {
            UIManager.SetState(GameUIManager.GameState.Playing);
        }
        
        if (GameUIScript != null)
        {
            GameUIScript.Initialize();
        }

        // Enable camera control after some time
        GetTree().CreateTimer(2.0f).Timeout += () => {
            if (MainCamera != null) 
            {
                MainCamera.SetProcess(true);
                GD.Print("MainMenu: MainCamera process enabled");
            }
        };
    }
}
