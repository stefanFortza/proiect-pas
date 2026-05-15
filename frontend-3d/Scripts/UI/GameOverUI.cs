using Godot;
using System;

public partial class GameOverUI : CanvasLayer
{
    [Export] public Ship PlayerShip;
    [Export] public Control UIContainer; // The panel containing the Game Over text and button
    [Export] public Button RetryButton;
    [Export(PropertyHint.File, "*.tscn")] public string LoadingScreenPath = "res://Scenes/UI/LoadingScreen.tscn";

    private bool _isGameOver = false;

    public override void _Ready()
    {
        // Hide UI at start
        if (UIContainer != null)
            UIContainer.Visible = false;

        if (PlayerShip != null)
        {
            PlayerShip.ShipSinking += OnGameOver;
            PlayerShip.ShipSunk += OnGameOver;
        }
        else
        {
            GD.PushWarning("GameOverUI: PlayerShip not assigned!");
        }

        if (RetryButton != null)
        {
            RetryButton.Pressed += OnRetryPressed;
        }
    }

    private void OnGameOver()
    {
        if (_isGameOver) return;
        
        _isGameOver = true;
        if (UIContainer != null)
        {
            UIContainer.Visible = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GD.Print("GameOverUI: Game Over triggered.");
        }
    }

    [Export] public GameUIManager UIManager;

    private void OnRetryPressed()
    {
        GD.Print("GameOverUI: Retrying...");
        
        if (UIManager != null)
        {
            UIManager.ResetGame();
            
            // Also reset MainMenu state
            var mainMenu = GetTree().Root.FindChild("MainMenu", true, false) as MainMenu;
            if (mainMenu != null)
            {
                mainMenu.ResetToMenu();
            }
        }
        else
        {
            GetTree().ReloadCurrentScene();
        }
    }

    public void ResetUI()
    {
        _isGameOver = false;
        if (UIContainer != null)
            UIContainer.Visible = false;
    }
}
