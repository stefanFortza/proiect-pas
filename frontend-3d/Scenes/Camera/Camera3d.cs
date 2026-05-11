using Godot;
using System;

public partial class Camera3d : Camera3D
{
    [Export] public float MouseSensitivity = 0.3f;
    [Export] public float DefaultSpeed = 10.0f;
    [Export] public float FastSpeed = 30.0f;
    [Export] public float Smoothness = 10.0f;

    private Vector3 _rotation = Vector3.Zero;
    private Vector3 _velocity = Vector3.Zero;
    private bool _isCaptured = false;

    public override void _Ready()
    {
        _rotation = RotationDegrees;
        // Optional: Start captured if needed, but better to let user toggle
    }

    public override void _Input(InputEvent @event)
    {
        // Toggle mouse capture
        if (@event.IsActionPressed("ui_cancel") || (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right && mb.Pressed))
        {
            ToggleCapture(true);
        }
        else if (@event is InputEventMouseButton mbRelease && mbRelease.ButtonIndex == MouseButton.Right && !mbRelease.Pressed)
        {
            // ToggleCapture(false); // If you want hold-to-look
        }

        if (_isCaptured && @event is InputEventMouseMotion mouseMotion)
        {
            _rotation.Y -= mouseMotion.Relative.X * MouseSensitivity;
            _rotation.X -= mouseMotion.Relative.Y * MouseSensitivity;
            _rotation.X = Mathf.Clamp(_rotation.X, -89f, 89f);
            
            RotationDegrees = _rotation;
        }

        // Release capture on ESC
        if (@event.IsActionPressed("ui_cancel"))
        {
            ToggleCapture(false);
        }
    }

    private void ToggleCapture(bool capture)
    {
        _isCaptured = capture;
        Input.MouseMode = capture ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
    }

    public override void _Process(double delta)
    {
        if (!_isCaptured) return;

        float speed = Input.IsActionPressed("speed_modifier") || Input.IsKeyPressed(Key.Shift) ? FastSpeed : DefaultSpeed;
        Vector3 direction = Vector3.Zero;

        // Using standard keys if actions aren't defined
        if (Input.IsKeyPressed(Key.W)) direction -= Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.S)) direction += Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.A)) direction -= Transform.Basis.X;
        if (Input.IsKeyPressed(Key.D)) direction += Transform.Basis.X;
        if (Input.IsKeyPressed(Key.Q)) direction -= Transform.Basis.Y;
        if (Input.IsKeyPressed(Key.E)) direction += Transform.Basis.Y;

        if (direction != Vector3.Zero)
        {
            direction = direction.Normalized();
        }

        Vector3 targetVelocity = direction * speed;
        _velocity = _velocity.Lerp(targetVelocity, (float)delta * Smoothness);
        
        GlobalPosition += _velocity * (float)delta;
    }
}
