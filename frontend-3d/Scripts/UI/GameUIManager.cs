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

    [Export] public Node MainMenuUI;
    [Export] public Node GameUI;
    [Export] public Node GameOverUI;

    private GameState _currentState = GameState.Menu;

    public override void _Ready()
    {
        SetState(GameState.Menu);
    }

    public void SetState(GameState newState)
    {
        _currentState = newState;

        SetNodeVisible(MainMenuUI, newState == GameState.Menu);
        SetNodeVisible(GameUI, newState == GameState.Playing);
        SetNodeVisible(GameOverUI, newState == GameState.GameOver);

        GD.Print($"GameUIManager: State changed to {newState}");
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
