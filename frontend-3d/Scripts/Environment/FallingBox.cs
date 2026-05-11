using Godot;
using System;

public partial class FallingBox : RigidBody3D
{
    [Export] public float LifeTime = 10.0f;
    [Export] public bool ExplodeOnTimer = true;

    public override void _Ready()
    {
        if (ExplodeOnTimer)
        {
            GetTree().CreateTimer(LifeTime).Timeout += OnLifeTimeTimeout;
        }
    }

    private void OnLifeTimeTimeout()
    {
        // Aici se poate adăuga un efect de particule sau sunet înainte de QueueFree
        QueueFree();
    }
}
