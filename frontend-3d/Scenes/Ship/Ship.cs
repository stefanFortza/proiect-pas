using Godot;
using System.Collections.Generic;

public partial class Ship : RigidBody3D
{
    [ExportGroup("Water Settings")]
    [Export] public float WaterLevelY = -47.9871f;
    [Export] public Vector3 WaterPosition = new Vector3(-35.2002f, -47.9871f, 230.115f);
    [Export] public float WaterPlaneSize = 500.0f;

    [ExportGroup("Buoyancy Physics")]
    [Export] public float BuoyancyFactor = 1.0f; // Reajustat. Lasă la 1.0 sau maxim 1.5 pentru 1000kg.
    [Export] public float WaterLinearDamp = 4.0f;
    [Export] public float WaterAngularDamp = 4.0f;

    [ExportGroup("Wave Settings (Shader Match)")]
    [Export] public float WaveHeight = 4.0f;
    [Export] public Vector2 WaveDirection = new Vector2(1.0f, -0.15f);
    [Export] public float WaveSpeed = 0.04f;

    [ExportGroup("Architecture")]
    [Export] public Node3D ProbeContainer;

    private List<Marker3D> _buoyancyProbes = new List<Marker3D>();
    private float _gravity;
    private FastNoiseLite _noise;

    public override void _Ready()
    {
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

        // Configurare zgomot conform water.tres
        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
        _noise.Frequency = 0.0868f;
        _noise.FractalType = FastNoiseLite.FractalTypeEnum.None;
        _noise.CellularDistanceFunction = FastNoiseLite.CellularDistanceFunctionEnum.EuclideanSquared;
        _noise.CellularJitter = 1.015f;

        // VALIDARE CRITICĂ: Previne căderea infinită dacă uiți să asignezi containerul
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

        if (_buoyancyProbes.Count == 0)
        {
            GD.PushWarning("Avertisment (Ship.cs): ProbeContainer este gol. Barca nu va avea flotabilitate.");
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

        // Returnează valori între -1.0 și 1.0
        float noiseVal = _noise.GetNoise2D(finalU * 512.0f, finalV * 512.0f);

        // // Dacă shader-ul tău de apă mișcă vertecșii și sub WaterLevelY, folosește direct valoarea de noise:
        return WaterLevelY + (noiseVal * WaveHeight);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
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
}