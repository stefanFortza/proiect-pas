using Godot;
using System;
using System.Collections.Generic;

public partial class Barca : RigidBody3D
{
	[Export] public float BuoyancyForce = 20.0f;
	[Export] public float WaterDrag = 2.0f;
	[Export] public float WaterAngularDrag = 2.0f;
	[Export] public float BoatHeight = 0.5f;
	[Export] public float ThrustForce = 15.0f;
	[Export] public float SteeringTorque = 10.0f;

	// Gerstner Wave Parameters (should match the shader)
	[Export] public Vector4 WaveA = new Vector4(1.0f, 1.0f, 0.15f, 10.0f);
	[Export] public Vector4 WaveB = new Vector4(1.0f, 0.6f, 0.10f, 5.0f);
	[Export] public Vector4 WaveC = new Vector4(1.0f, 1.3f, 0.10f, 3.0f);

	private List<Marker3D> _probes = new List<Marker3D>();
	private bool _isInWater = true;

	public override void _Ready()
	{
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
		float offset = 1.0f;
		string[] names = { "Probe_Front", "Probe_Back", "Probe_Left", "Probe_Right" };
		Vector3[] positions = {
			new Vector3(0, 0, offset),
			new Vector3(0, 0, -offset),
			new Vector3(offset, 0, 0),
			new Vector3(-offset, 0, 0)
		};

		for (int i = 0; i < 4; i++)
		{
			Marker3D probe = new Marker3D();
			probe.Name = names[i];
			probe.Position = positions[i];
			AddChild(probe);
			_probes.Add(probe);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isInWater) return;

		float time = (float)Time.GetTicksMsec() / 1000.0f;
		float buoyancyPerProbe = BuoyancyForce / _probes.Count;

		int probesInWater = 0;
		foreach (var probe in _probes)
		{
			Vector3 worldPos = probe.GlobalPosition;
			float waterHeight = GetWaveHeight(worldPos, time);

			float depth = waterHeight - worldPos.Y;
			if (depth > 0)
			{
				probesInWater++;
				float displacement = Mathf.Clamp(depth / BoatHeight, 0.0f, 1.0f);
				Vector3 force = Vector3.Up * displacement * buoyancyPerProbe;
				ApplyForce(force, worldPos - GlobalPosition);
				// Apply drag
				Vector3 localPos = worldPos - GlobalPosition;
				Vector3 probeVelocity = LinearVelocity + AngularVelocity.Cross(localPos);
				ApplyForce(-probeVelocity * WaterDrag / _probes.Count, localPos);
			}
		}

		if (probesInWater > 0)
		{
			float forwardInput = Input.GetAxis("ui_down", "ui_up");
			float steerInput = Input.GetAxis("ui_right", "ui_left");

			if (Mathf.Abs(forwardInput) > 0.1f)
			{
				ApplyCentralForce(GlobalTransform.Basis.Z * forwardInput * ThrustForce);
			}

			if (Mathf.Abs(steerInput) > 0.1f)
			{
				ApplyTorque(Vector3.Up * steerInput * SteeringTorque);
			}
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

	public void OnZonaApaBodyEntered(Node3D body)
	{
		if (body == this) _isInWater = true;
	}

	public void OnZonaApaBodyExited(Node3D body)
	{
		if (body == this) _isInWater = false;
	}
}
