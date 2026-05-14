using Godot;
using System;

public partial class GameUIManager : Node
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    [Export] public Node MainMenuUINode;
    [Export] public Node GameUINode;
    [Export] public Node GameOverUINode;
    [Export] public Ship PlayerShip;

    private GameState _currentState = GameState.Menu;

    public override void _Ready()
    {
        SetState(GameState.Menu);
        if (PlayerShip != null)
        {
            PlayerShip.ShipSinking += () => SetState(GameState.GameOver);
            PlayerShip.ShipSunk += () => SetState(GameState.GameOver);
        }
        else
        {
            GD.PushWarning("GameUIManager: PlayerShip not assigned!");
        }
    }

    public void SetState(GameState newState)
    {
        _currentState = newState;

        SetNodeVisible(MainMenuUINode, newState == GameState.Menu);
        SetNodeVisible(GameUINode, newState == GameState.Playing);
        SetNodeVisible(GameOverUINode, newState == GameState.GameOver);

        // Control spawner based on state
        if (GameUINode is GameUI gameUIScript && gameUIScript.Spawner != null)
        {
            if (newState == GameState.Playing)
                gameUIScript.Spawner.StartSpawner();
            else
                gameUIScript.Spawner.StopSpawner();
        }

        GD.Print($"GameUIManager: State changed to {newState}");
    }

    public void ResetGame()
    {
        if (PlayerShip != null)
        {
            PlayerShip.ResetShip();
        }

        if (GameUINode is GameUI gameUIScript)
        {
            gameUIScript.ResetUI();
        }
        
        if (GameOverUINode is GameOverUI gameOverUIScript)
        {
            gameOverUIScript.ResetUI();
        }

        SetState(GameState.Menu);
    }

    private void SetNodeVisible(Node node, bool visible)
    {
        if (node == null) return;

        if (node is Control c)
        {
            c.Visible = visible;
        }
        else if (node is CanvasLayer cl)
        {
            cl.Visible = visible;
        }
    }
}
