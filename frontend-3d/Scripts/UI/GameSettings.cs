using Godot;
using System;

public partial class GameSettings : Node
{
    public static GameSettings Instance { get; private set; }

    public string SelectedModel = "gpt2_small_base";
    public int SelectedEpoch = 1;
    public string BackendUrl = "http://localhost:8000";

    public override void _Ready()
    {
        Instance = this;
    }
}
