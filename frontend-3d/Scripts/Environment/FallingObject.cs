using Godot;
using System;

public partial class FallingObject : RigidBody3D
{
    [Export] public float LifeTime = 15.0f;
    [Export] public bool DestroyOnTimer = true;

    public override void _Ready()
    {
        if (DestroyOnTimer)
        {
            GetTree().CreateTimer(LifeTime).Timeout += OnLifeTimeTimeout;
        }
    }

    private void OnLifeTimeTimeout()
    {
        QueueFree();
    }
}
