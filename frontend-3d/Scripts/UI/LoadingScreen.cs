using Godot;
using System;

public partial class LoadingScreen : Control
{
    [Export(PropertyHint.File, "*.tscn")] public string TargetScenePath = "res://Scenes/World.tscn";
    [Export] public ProgressBar ProgressBar;
    [Export] public Label ProgressLabel;

    private Godot.Collections.Array _progress = new Godot.Collections.Array();
    private bool _loadingStarted = false;

    public override void _Ready()
    {
        if (ProgressBar != null) ProgressBar.Value = 0;
        if (ProgressLabel != null) ProgressLabel.Text = "Loading... 0%";

        // Start loading the scene
        Error error = ResourceLoader.LoadThreadedRequest(TargetScenePath);
        if (error != Error.Ok)
        {
            GD.PushError($"LoadingScreen: Failed to start loading {TargetScenePath}: {error}");
        }
        else
        {
            _loadingStarted = true;
        }
    }

    public override void _Process(double delta)
    {
        if (!_loadingStarted) return;

        ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus(TargetScenePath, _progress);

        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                float progressValue = (float)_progress[0] * 100.0f;
                if (ProgressBar != null) ProgressBar.Value = progressValue;
                if (ProgressLabel != null) ProgressLabel.Text = $"Loading... {(int)progressValue}%";
                break;

            case ResourceLoader.ThreadLoadStatus.Loaded:
                var nextScene = (PackedScene)ResourceLoader.LoadThreadedGet(TargetScenePath);
                GetTree().ChangeSceneToPacked(nextScene);
                _loadingStarted = false;
                break;

            case ResourceLoader.ThreadLoadStatus.Failed:
                GD.PushError($"LoadingScreen: Loading failed for {TargetScenePath}");
                _loadingStarted = false;
                break;

            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                GD.PushError($"LoadingScreen: Invalid resource {TargetScenePath}");
                _loadingStarted = false;
                break;
        }
    }
}
