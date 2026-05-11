using Godot;
using System;
using System.Collections.Generic;

public partial class ObjectSpawner : Node3D
{
    [Export] public List<PackedScene> ScenesToSpawn = new List<PackedScene>();
    [Export] public float SpawnInterval = 2.0f;
    [Export] public Vector3 SpawnAreaSize = new Vector3(4.0f, 0.0f, 8.0f);
    [Export] public float SpawnHeight = 5.0f;
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
            _spawnTimer.Timeout += SpawnRandom;
            AddChild(_spawnTimer);
        }
    }

    public void SpawnRandom()
    {
        if (ScenesToSpawn.Count == 0) return;

        int index = _rng.RandiRange(0, ScenesToSpawn.Count - 1);
        SpawnAtIndex(index);
    }

    public void SpawnAtIndex(int index)
    {
        if (index < 0 || index >= ScenesToSpawn.Count) return;

        PackedScene scene = ScenesToSpawn[index];
        if (scene == null) return;

        Node3D instance = scene.Instantiate<Node3D>();
        
        // Calculăm poziția globală pentru a fi deasupra spawner-ului
        float x = _rng.RandfRange(-SpawnAreaSize.X / 2.0f, SpawnAreaSize.X / 2.0f);
        float z = _rng.RandfRange(-SpawnAreaSize.Z / 2.0f, SpawnAreaSize.Z / 2.0f);
        
        // Obținem părintele (ex: World) pentru a adăuga obiectul acolo
        // Astfel obiectele sunt independente de mișcarea navei după spawn
        Node parent = GetParent();
        if (parent == null) parent = GetTree().Root;
        
        parent.AddChild(instance);

        // Setăm poziția globală bazată pe transformarea spawner-ului
        Vector3 spawnOffset = new Vector3(x, SpawnHeight, z);
        instance.GlobalPosition = GlobalPosition + GlobalTransform.Basis * spawnOffset;
    }

    // Metode modulare cerute de utilizator
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
