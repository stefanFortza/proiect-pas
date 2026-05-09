using Godot;
using System;

public partial class Barca : RigidBody3D
{
	[Export] public float BuoyancyForce = 15.0f;
	[Export] public float WaterDrag = 1.0f;
	[Export] public float WaterAngularDrag = 1.0f;

	private bool _isInWater = false;
	private float _waterLevel = 0.0f;

	public override void _Ready()
	{
		// Optional: Initialize water level based on ZonaApa if needed
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isInWater)
		{
			// Simple buoyancy: apply force based on how deep the boat is
			// For simplicity, we assume the water level is at Y = 0
			float depth = _waterLevel - GlobalPosition.Y;
			
			if (depth > -0.5f) // Adjust based on boat height
			{
				float displacement = Mathf.Clamp(depth + 0.5f, 0.0f, 1.0f);
				ApplyCentralForce(Vector3.Up * displacement * BuoyancyForce);
				
				// Apply drag to simulate water resistance
				ApplyCentralForce(-LinearVelocity * WaterDrag);
				ApplyTorque(-AngularVelocity * WaterAngularDrag);
			}
		}
	}

	public void OnZonaApaBodyEntered(Node3D body)
	{
		if (body == this)
		{
			_isInWater = true;
		}
	}

	public void OnZonaApaBodyExited(Node3D body)
	{
		if (body == this)
		{
			_isInWater = false;
		}
	}
}
