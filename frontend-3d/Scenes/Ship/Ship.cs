using Godot;
using System;
using System.Collections.Generic;

public partial class Ship : RigidBody3D
{
    public enum ShipState
    {
        Sailing,
        Sinking,
        Sunk
    }

    [Signal] public delegate void ShipSinkingEventHandler();
    [Signal] public delegate void ShipSunkEventHandler();

    [ExportGroup("State Machine")]
    [Export] public ShipState CurrentState = ShipState.Sailing;
    [Export] public float SinkingThreshold = -2.0f; // Depth below water level to trigger sinking
    [Export] public float SinkingDamping = 2.0f;

    [ExportGroup("Water Settings")]
    [Export] public float WaterLevelY = -47.9871f;
    [Export] public Vector3 WaterPosition = new Vector3(-35.2002f, -47.9871f, 230.115f);
    [Export] public float WaterPlaneSize = 500.0f;

    [ExportGroup("Buoyancy Physics")]
    [Export] public float BuoyancyFactor = 1.0f;
    [Export] public float WaterLinearDamp = 4.0f;
    [Export] public float WaterAngularDamp = 4.0f;

    [ExportGroup("Wave Settings (Shader Match)")]
    [Export] public float WaveHeight = 4.0f;
    [Export] public Vector2 WaveDirection = new Vector2(1.0f, -0.15f);
    [Export] public float WaveSpeed = 0.04f;

    [ExportGroup("Architecture")]
    [Export] public Node3D ProbeContainer;
    [Export] public ShipController ShipController;
    [Export] public ObjectContainer ObjectContainer;

    private List<Marker3D> _buoyancyProbes = new List<Marker3D>();
    private float _gravity;
    private FastNoiseLite _noise;

    public override void _EnterTree()
    {
        base._EnterTree();
        if (ShipController != null)
            ShipController.Container = ObjectContainer;
    }

    public override void _Ready()
    {
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
        _noise.Frequency = 0.0868f;
        _noise.FractalType = FastNoiseLite.FractalTypeEnum.None;
        _noise.CellularDistanceFunction = FastNoiseLite.CellularDistanceFunctionEnum.EuclideanSquared;
        _noise.CellularJitter = 1.015f;

        if (ProbeContainer == null)
        {
            GD.PushError("CRITICAL (Ship.cs): ProbeContainer nu a fost asignat în Inspector!");
            return;
        }

        foreach (Node child in ProbeContainer.GetChildren())
        {
            if (child is Marker3D marker)
            {
                _buoyancyProbes.Add(marker);
            }
        }
    }

    private float GetWaterHeight(Vector3 globalPos)
    {
        float time = (float)Time.GetTicksMsec() / 1000.0f;
        float relativeX = globalPos.X - WaterPosition.X;
        float relativeZ = globalPos.Z - WaterPosition.Z;

        float u = (relativeX / WaterPlaneSize) + 0.5f;
        float v = (relativeZ / WaterPlaneSize) + 0.5f;

        Vector2 offset = time * WaveSpeed * WaveDirection;
        float finalU = u + offset.X;
        float finalV = v + offset.Y;

        float noiseVal = _noise.GetNoise2D(finalU * 512.0f, finalV * 512.0f);
        return WaterLevelY + (noiseVal * WaveHeight);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        switch (CurrentState)
        {
            case ShipState.Sailing:
                HandleSailingPhysics(state);
                CheckForSinking();
                break;
            case ShipState.Sinking:
                HandleSinkingPhysics(state);
                break;
            case ShipState.Sunk:
                // No forces applied
                break;
        }
    }

    private void HandleSailingPhysics(PhysicsDirectBodyState3D state)
    {
        if (_buoyancyProbes.Count == 0) return;

        int submergedProbes = 0;

        foreach (Marker3D probe in _buoyancyProbes)
        {
            float currentWaterHeight = GetWaterHeight(probe.GlobalPosition);
            float depth = currentWaterHeight - probe.GlobalPosition.Y;

            if (depth > 0)
            {
                submergedProbes++;
                float buoyancyForce = (depth * BuoyancyFactor * Mass * _gravity) / _buoyancyProbes.Count;
                Vector3 localPosition = probe.GlobalPosition - GlobalPosition;
                state.ApplyForce(Vector3.Up * buoyancyForce, localPosition);
            }
        }

        if (submergedProbes > 0)
        {
            float submergeRatio = (float)submergedProbes / _buoyancyProbes.Count;
            LinearDamp = WaterLinearDamp * submergeRatio;
            AngularDamp = WaterAngularDamp * submergeRatio;
        }
        else
        {
            LinearDamp = 0.0f;
            AngularDamp = 0.0f;
        }
    }

    private void CheckForSinking()
    {
        float currentWaterHeight = GetWaterHeight(GlobalPosition);
        float depth = currentWaterHeight - GlobalPosition.Y;

        if (depth > -SinkingThreshold)
        {
            GD.Print("Ship: Sinking state triggered!");
            CurrentState = ShipState.Sinking;
            EmitSignal(SignalName.ShipSinking);
        }
    }

    private void HandleSinkingPhysics(PhysicsDirectBodyState3D state)
    {
        LinearDamp = SinkingDamping;
        AngularDamp = SinkingDamping;

        state.ApplyForce(Vector3.Down * Mass * 2.0f);

        if (GlobalPosition.Y < WaterLevelY - 50.0f)
        {
            CurrentState = ShipState.Sunk;
            GD.Print("Ship: Sunk.");
            EmitSignal(SignalName.ShipSunk);
        }
    }
}
