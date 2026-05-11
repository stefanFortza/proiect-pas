using Godot;
using System.Collections.Generic;

public partial class Ship : RigidBody3D
{
    [Export] public float WaterLevelY = 0.0f; 
    [Export] public float BuoyancyFactor = 2.0f; 
    [Export] public float WaterDrag = 0.95f; 

    private List<Marker3D> _buoyancyProbes = new List<Marker3D>();
    private float _gravity;

    public override void _Ready()
    {
        // Extragerea valorii gravitaționale setate la nivel de proiect
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

        // Colectarea dinamică a probelor de plutire
        foreach (Node child in GetChildren())
        {
            if (child is Marker3D marker)
            {
                _buoyancyProbes.Add(marker);
            }
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (_buoyancyProbes.Count == 0) return;

        bool isTouchingWater = false;

        foreach (Marker3D probe in _buoyancyProbes)
        {
            // Calculul adâncimii individuale pentru fiecare probă
            float depth = WaterLevelY - probe.GlobalPosition.Y;

            if (depth > 0)
            {
                isTouchingWater = true;
                
                // Aplicarea Principiului lui Arhimede distribuit per probă
                float buoyancyForce = (depth * BuoyancyFactor * Mass * _gravity) / _buoyancyProbes.Count;
                
                // Calculul ofsetului local pentru aplicarea corectă a momentului forței (Torque)
                Vector3 localPosition = probe.GlobalPosition - GlobalPosition;
                state.ApplyForce(Vector3.Up * buoyancyForce, localPosition);
            }
        }

        // Aplicarea frecării hidrodinamice pentru stabilizarea sistemului
        if (isTouchingWater)
        {
            state.LinearVelocity *= WaterDrag;
            state.AngularVelocity *= WaterDrag;
        }
    }
}
