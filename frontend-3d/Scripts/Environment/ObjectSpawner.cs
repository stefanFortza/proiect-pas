using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class ObjectSpawner : Node3D
{
    [Signal] public delegate void ObjectSpawnedEventHandler(Node3D instance, Vector3 globalPosition);

    [Export] public Array<PackedScene> ScenesToSpawn = new Array<PackedScene>();
    [Export] public float SpawnInterval = 2.0f;
    [Export] public Vector3 SpawnAreaSize = new Vector3(4.0f, 0.0f, 8.0f);
    [Export] public float SpawnHeight = 5.0f;
    [Export] public float ScaleMin = 0.8f;
    [Export] public float ScaleMax = 1.2f;
    [Export] public bool AutoSpawn = true;

    private Timer _spawnTimer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();

        if (AutoSpawn)
        {
            _spawnTimer = new Timer();
            _spawnTimer.WaitTime = SpawnInterval;
            _spawnTimer.Autostart = true;
            _spawnTimer.OneShot = false; // Just to be sure
            _spawnTimer.Timeout += SpawnRandom;
            AddChild(_spawnTimer);
            _spawnTimer.Start(); // Explicit start call
            GD.Print($"ObjectSpawner: Timer started with interval {SpawnInterval}s");
        }
        else
        {
            GD.Print("ObjectSpawner: AutoSpawn is disabled.");
        }
    }

    public void SpawnRandom()
    {
        GD.Print("ObjectSpawner: SpawnRandom triggered.");
        if (ScenesToSpawn == null || ScenesToSpawn.Count == 0) 
        {
            GD.PushWarning("ObjectSpawner: ScenesToSpawn is empty!");
            return;
        }

        int index = _rng.RandiRange(0, ScenesToSpawn.Count - 1);
        SpawnAtIndex(index);
    }

    public void SpawnByScore(int score)
    {
        if (ScenesToSpawn == null || ScenesToSpawn.Count == 0) return;

        // Map score 0-100 to index 0-Count
        int index = Mathf.Clamp((score * (ScenesToSpawn.Count - 1)) / 100, 0, ScenesToSpawn.Count - 1);
        GD.Print($"ObjectSpawner: Spawning object for score {score} at index {index}");
        SpawnAtIndex(index);
    }

    public void SpawnAtIndex(int index)
    {
        if (ScenesToSpawn == null || index < 0 || index >= ScenesToSpawn.Count) return;

        PackedScene scene = ScenesToSpawn[index];
        if (scene == null) return;

        Node3D instance = scene.Instantiate<Node3D>();

        // Calculăm poziția relativă în aria de spawn
        float x = _rng.RandfRange(-SpawnAreaSize.X / 2.0f, SpawnAreaSize.X / 2.0f);
        float z = _rng.RandfRange(-SpawnAreaSize.Z / 2.0f, SpawnAreaSize.Z / 2.0f);

        // Calculăm poziția globală bazată pe transformarea spawner-ului
        Vector3 spawnOffset = new Vector3(x, SpawnHeight, z);
        Vector3 globalPos = GlobalPosition + GlobalTransform.Basis * spawnOffset;

        // Adăugăm o rotație random pentru varietate
        instance.RotationDegrees = new Vector3(
            _rng.RandfRange(0, 360),
            _rng.RandfRange(0, 360),
            _rng.RandfRange(0, 360)
        );

        // Adăugăm o variație de scală
        float scale = _rng.RandfRange(ScaleMin, ScaleMax);
        instance.Scale = new Vector3(scale, scale, scale);

        EmitSignal(SignalName.ObjectSpawned, instance, globalPos);
    }

    // Metode modulare
    public void AddScene(PackedScene scene)
    {
        if (scene != null && !ScenesToSpawn.Contains(scene))
        {
            ScenesToSpawn.Add(scene);
        }
    }

    public void RemoveScene(PackedScene scene)
    {
        if (ScenesToSpawn.Contains(scene))
        {
            ScenesToSpawn.Remove(scene);
        }
    }

    public void ClearScenes()
    {
        ScenesToSpawn.Clear();
    }
}
