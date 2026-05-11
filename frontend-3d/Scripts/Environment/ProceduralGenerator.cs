using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ProceduralGenerator : Node3D
{
    [ExportGroup("Prefabs")]
    [Export] public Godot.Collections.Array<PackedScene> ForestPrefabs = new Godot.Collections.Array<PackedScene>();

    [ExportGroup("Editor Actions")]
    [Export]
    public bool Regenerate
    {
        get => false;
        set
        {
            if (value)
            {
                _rng.Randomize();
                InitializeNoise();
                Generate();
            }
        }
    }

    [ExportGroup("Grid Settings")]
    [Export] public int GridSize = 60;
    [Export] public float Spacing = 2.5f;
    [Export] public float Threshold = 0.2f; 
    [Export(PropertyHint.Range, "0,1,0.05")] public float GlobalDensity = 0.7f;

    [ExportGroup("Cellular Automata")]
    [Export] public int Iterations = 3;
    [Export] public int BirthThreshold = 5; 
    [Export] public int DeathThreshold = 3; 

    [ExportGroup("Noise Settings")]
    [Export] public float Frequency = 0.05f;
    [Export] public FastNoiseLite.NoiseTypeEnum NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

    [ExportGroup("Terrain Alignment")]
    [Export(PropertyHint.Layers3DPhysics)] public uint TerrainLayer = 1;
    [Export(PropertyHint.Layers3DPhysics)] public uint WaterLayer = 2;
    [Export(PropertyHint.Layers3DPhysics)] public uint AvoidLayer = 4;
    [Export] public float RaycastHeight = 100.0f;
    [Export] public bool AlignToNormal = false;

    private bool[,] _grid;
    private FastNoiseLite _noise;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        
        _rng.Randomize();
        InitializeNoise();
        Generate();
    }

    private void InitializeNoise()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = (int)_rng.Randi();
        _noise.Frequency = Frequency;
        _noise.NoiseType = NoiseType;
    }

    public void Generate()
    {
        if (!IsInsideTree()) return;

        // 1. Initial generation using Perlin Noise
        _grid = new bool[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                float noiseVal = _noise.GetNoise2D(x, y);
                _grid[x, y] = noiseVal > Threshold;
            }
        }

        // 2. Refine using Cellular Automata
        for (int i = 0; i < Iterations; i++)
        {
            ApplyCellularAutomata();
        }

        // 3. Instantiate objects
        SpawnGrid();
    }

    private void ApplyCellularAutomata()
    {
        bool[,] newGrid = new bool[GridSize, GridSize];

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                int neighbors = CountNeighbors(x, y);

                if (_grid[x, y])
                {
                    // Survivor rule
                    newGrid[x, y] = neighbors >= DeathThreshold;
                }
                else
                {
                    // Birth rule
                    newGrid[x, y] = neighbors >= BirthThreshold;
                }
            }
        }

        _grid = newGrid;
    }

    private int CountNeighbors(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                int nx = x + i;
                int ny = y + j;

                if (nx >= 0 && nx < GridSize && ny >= 0 && ny < GridSize)
                {
                    if (_grid[nx, ny]) count++;
                }
            }
        }
        return count;
    }

    private void SpawnGrid()
    {
        // Clear existing children
        foreach (Node child in GetChildren())
        {
            if (child is Node3D)
            {
                child.Free(); // Use Free in editor for immediate cleanup
            }
        }

        if (ForestPrefabs == null || ForestPrefabs.Count == 0) return;

        var spaceState = GetWorld3D().DirectSpaceState;
        float offset = (GridSize * Spacing) / 2.0f;

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_grid[x, y])
                {
                    // 1. Density Check (skip some cells to reduce crowding)
                    if (_rng.Randf() > GlobalDensity) continue;

                    // 2. Pick Random Prefab
                    int prefabIndex = _rng.RandiRange(0, ForestPrefabs.Count - 1);
                    PackedScene selectedPrefab = ForestPrefabs[prefabIndex];
                    if (selectedPrefab == null) continue;

                    // Calculate horizontal position relative to this node
                    float jitterX = _rng.RandfRange(-0.3f, 0.3f) * Spacing;
                    float jitterY = _rng.RandfRange(-0.3f, 0.3f) * Spacing;
                    
                    Vector3 localPos = new Vector3(
                        x * Spacing - offset + jitterX, 
                        0, 
                        y * Spacing - offset + jitterY
                    );
                    
                    // Raycast to find ground, detect water and avoidance layer (bridge/houses)
                    Vector3 globalHorizontalPos = GlobalTransform * localPos;
                    Vector3 rayStart = globalHorizontalPos + Vector3.Up * RaycastHeight;
                    Vector3 rayEnd = globalHorizontalPos + Vector3.Down * RaycastHeight;
                    
                    // Combined mask: Terrain OR Water OR Avoid
                    uint combinedMask = TerrainLayer | WaterLayer | AvoidLayer;
                    var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd, combinedMask);
                    var result = spaceState.IntersectRay(query);

                    if (result.Count > 0)
                    {
                        var collider = (CollisionObject3D)result["collider"];
                        uint hitLayer = collider.CollisionLayer;

                        // 3. Avoidance logic: Skip if it hit Water or Avoid Layer
                        if ((hitLayer & WaterLayer) != 0 || (hitLayer & AvoidLayer) != 0)
                        {
                            continue;
                        }

                        Vector3 groundPos = (Vector3)result["position"];
                        Vector3 normal = (Vector3)result["normal"];

                        Node3D instance = selectedPrefab.Instantiate<Node3D>();
                        AddChild(instance);
                        instance.GlobalPosition = groundPos;

                        // Random rotation
                        instance.Rotation = new Vector3(0, _rng.RandfRange(0, Mathf.Pi * 2), 0);
                        
                        if (AlignToNormal && normal.IsNormalized())
                        {
                            Vector3 newUp = normal;
                            Vector3 newForward = instance.Transform.Basis.Z;
                            if (Mathf.Abs(newUp.Dot(newForward)) > 0.99f)
                                newForward = instance.Transform.Basis.X;
                                
                            instance.LookAt(instance.GlobalPosition + newForward, newUp);
                        }

                        float s = _rng.RandfRange(0.7f, 1.3f);
                        instance.Scale = new Vector3(s, s, s);
                    }
                }
            }
        }
    }
}

