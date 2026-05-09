using Godot;
using System;
using System.Collections.Generic;

public partial class Barca : RigidBody3D
{
    [ExportGroup("Buoyancy Settings")]
    [Export] public float BuoyancyConstant = 250.0f; // Force per meter of submergence per probe
    [Export] public float WaterLinearDamp = 1.5f;
    [Export] public float WaterAngularDamp = 3.0f;
    [Export] public float BoatHeight = 1.0f;

    [ExportGroup("Movement Settings")]
    [Export] public float ThrustForce = 800.0f;
    [Export] public float SteeringTorque = 500.0f;

    [ExportGroup("Wave Parameters")]
    [Export] public Vector4 WaveA = new Vector4(1.0f, 1.0f, 0.15f, 10.0f);
    [Export] public Vector4 WaveB = new Vector4(1.0f, 0.6f, 0.10f, 5.0f);
    [Export] public Vector4 WaveC = new Vector4(1.0f, 1.3f, 0.10f, 3.0f);

    private List<Marker3D> _probes = new List<Marker3D>();
    private bool _isInWater = true;
    private float _extraMass = 0.0f; // Tracked via CargoArea
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _Ready()
    {
        LinearDamp = 0.5f;
        AngularDamp = 0.5f;

        // Connect CargoArea signals
        var cargoArea = GetNodeOrNull<Area3D>("CargoArea");
        if (cargoArea != null)
        {
            cargoArea.BodyEntered += OnCargoAreaBodyEntered;
            cargoArea.BodyExited += OnCargoAreaBodyExited;
        }

        foreach (var child in GetChildren())
        {
            if (child is Marker3D marker && marker.Name.ToString().Contains("Probe"))
            {
                _probes.Add(marker);
            }
        }

        if (_probes.Count == 0)
        {
            CreateDefaultProbes();
        }
    }

    private void CreateDefaultProbes()
    {
        float offsetZ = 3.5f;
        float offsetX = 1.5f;
        Vector3[] positions = {
            new Vector3(offsetX, 0, offsetZ),
            new Vector3(-offsetX, 0, offsetZ),
            new Vector3(offsetX, 0, -offsetZ),
            new Vector3(-offsetX, 0, -offsetZ),
            new Vector3(0, 0, 0)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Marker3D probe = new Marker3D();
            probe.Name = $"Probe_{i}";
            probe.Position = positions[i];
            AddChild(probe);
            _probes.Add(probe);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isInWater)
        {
            LinearDamp = 0.1f;
            AngularDamp = 0.1f;
            return;
        }

        LinearDamp = WaterLinearDamp;
        AngularDamp = WaterAngularDamp;

        float time = (float)Time.GetTicksMsec() / 1000.0f;
        
        int probesInWater = 0;
        foreach (var probe in _probes)
        {
            Vector3 worldPos = probe.GlobalPosition;
            float waterHeight = GetWaveHeight(worldPos, time);

            float depth = waterHeight - worldPos.Y;
            if (depth > 0)
            {
                probesInWater++;
                // Buoyancy based on Archimedes principle (simplifed)
                // Force increases with depth
                float displacement = Mathf.Clamp(depth / BoatHeight, 0.0f, 2.0f);
                Vector3 buoyancyForce = Vector3.Up * displacement * BuoyancyConstant;
                
                ApplyForce(buoyancyForce, worldPos - GlobalPosition);

                // Additional drag based on depth for more stability
                Vector3 localPos = worldPos - GlobalPosition;
                Vector3 probeVelocity = LinearVelocity + AngularVelocity.Cross(localPos);
                ApplyForce(-probeVelocity * WaterLinearDamp * displacement, localPos);
            }
        }

        if (probesInWater > 0)
        {
            HandleInput(delta, probesInWater);
        }
    }

    private void HandleInput(double delta, int probesInWater)
    {
        float forwardInput = Input.GetAxis("ui_down", "ui_up");
        float steerInput = Input.GetAxis("ui_right", "ui_left");

        float forceScale = (float)probesInWater / _probes.Count;

        if (Mathf.Abs(forwardInput) > 0.05f)
        {
            ApplyCentralForce(GlobalTransform.Basis.Z * forwardInput * ThrustForce * forceScale);
        }

        if (Mathf.Abs(steerInput) > 0.05f)
        {
            ApplyTorque(Vector3.Up * steerInput * SteeringTorque * forceScale);
        }
    }

    private float GetWaveHeight(Vector3 p, float time)
    {
        Vector3 offset = Vector3.Zero;
        offset += CalculateGerstner(WaveA, p, time);
        offset += CalculateGerstner(WaveB, p, time);
        offset += CalculateGerstner(WaveC, p, time);
        return offset.Y;
    }

    private Vector3 CalculateGerstner(Vector4 wave, Vector3 p, float time)
    {
        float steepness = wave.Z;
        float wavelength = wave.W;
        float k = 2.0f * (float)Math.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k);
        Vector2 d = new Vector2(wave.X, wave.Y).Normalized();
        float f = k * (d.Dot(new Vector2(p.X, p.Z)) - c * time);
        float a = steepness / k;

        return new Vector3(
            d.X * (a * Mathf.Cos(f)),
            a * Mathf.Sin(f),
            d.Y * (a * Mathf.Cos(f))
        );
    }

    public void OnCargoAreaBodyEntered(Node3D body)
    {
        if (body is RigidBody3D rb && rb != this)
        {
            // We don't change the actual Mass property to avoid physics glitches,
            // but we can increase BuoyancyConstant requirement or just let it sink
            // Actually, the boat will naturally sink if we don't increase force
            GD.Print($"Cargo added: {body.Name}, Mass: {rb.Mass}");
        }
    }

    public void OnCargoAreaBodyExited(Node3D body)
    {
        if (body is RigidBody3D rb && rb != this)
        {
            GD.Print($"Cargo removed: {body.Name}");
        }
    }

    public void OnZonaApaBodyEntered(Node3D body)
    {
        if (body == this) _isInWater = true;
    }

    public void OnZonaApaBodyExited(Node3D body)
    {
        if (body == this) _isInWater = false;
    }
}
