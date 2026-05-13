using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class GrassGenerator : Node3D
{
    [ExportGroup("Mesh & Material")]
    [Export] public Mesh GrassMesh;
    [Export] public ShaderMaterial GrassMaterial;

    [ExportGroup("Distribution")]
    [Export] public int GridSize = 100;
    [Export] public float Spacing = 0.5f;
    [Export] public float Threshold = -0.1f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float Density = 0.5f;

    [ExportGroup("Noise")]
    [Export] public float Frequency = 0.1f;
    [Export] public FastNoiseLite.NoiseTypeEnum NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

    [ExportGroup("Terrain Alignment")]
    [Export(PropertyHint.Layers3DPhysics)] public uint TerrainLayer = 1;
    [Export(PropertyHint.Layers3DPhysics)] public uint WaterLayer = 2;
    [Export(PropertyHint.Layers3DPhysics)] public uint AvoidLayer = 4;
    [Export] public float RaycastHeight = 100.0f;

    [Export] public bool AlignToNormal = false;
    [Export] public bool UseRaycast = true;

    [ExportGroup("Editor Actions")]
    [Export]
    public bool Regenerate
    {
        get => false;
        set { if (value) Generate(); }
    }

    private MultiMeshInstance3D _multiMeshInstance;
    private FastNoiseLite _noise;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        Generate();
    }

    public void Generate()
    {
        _rng.Randomize();
        GD.Print($"GrassGenerator: Starting generation at {GlobalPosition}. Mask: {TerrainLayer | WaterLayer | AvoidLayer}. Mesh: {(GrassMesh != null ? GrassMesh.ResourceName : "NULL")}");
        if (!IsInsideTree()) 
        {
            GD.Print("GrassGenerator: Not inside tree!");
            return;
        }

        // Cleanup existing children
        int childCount = GetChildCount();
        foreach (Node child in GetChildren())
        {
            if (Engine.IsEditorHint())
                child.Free();
            else
                child.QueueFree();
        }
        if (childCount > 0) GD.Print($"GrassGenerator: Cleaned up {childCount} children.");

        if (GrassMesh == null)
        {
            GD.PushWarning("GrassGenerator: GrassMesh is not set!");
            return;
        }

        InitializeNoise();
        
        _multiMeshInstance = new MultiMeshInstance3D();
        _multiMeshInstance.Name = "GrassMultiMesh";
        AddChild(_multiMeshInstance);

        MultiMesh multiMesh = new MultiMesh();
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.Mesh = GrassMesh;
        _multiMeshInstance.Multimesh = multiMesh;
        _multiMeshInstance.MaterialOverride = GrassMaterial;

        List<Transform3D> transforms = new List<Transform3D>();
        var spaceState = GetWorld3D().DirectSpaceState;

        float offset = (GridSize * Spacing) / 2.0f;
        int rayHits = 0;
        int waterAvoids = 0;
        int noiseSkips = 0;
        int densitySkips = 0;

        for (int x = 0; x < GridSize; x++)
        {
            for (int z = 0; z < GridSize; z++)
            {
                float noiseVal = _noise.GetNoise2D(x, z);
                if (noiseVal < Threshold) 
                {
                    noiseSkips++;
                    continue;
                }
                
                if (_rng.Randf() > Density) 
                {
                    densitySkips++;
                    continue;
                }

                float jitterX = _rng.RandfRange(-0.4f, 0.4f) * Spacing;
                float jitterZ = _rng.RandfRange(-0.4f, 0.4f) * Spacing;

                Vector3 localPos = new Vector3(
                    x * Spacing - offset + jitterX,
                    0,
                    z * Spacing - offset + jitterZ
                );

                Vector3 globalHorizontalPos = GlobalTransform * localPos;
                Vector3 groundPos = globalHorizontalPos;
                Vector3 normal = Vector3.Up;

                if (UseRaycast)
                {
                    Vector3 rayStart = globalHorizontalPos + Vector3.Up * RaycastHeight;
                    Vector3 rayEnd = globalHorizontalPos + Vector3.Down * RaycastHeight;

                    uint combinedMask = TerrainLayer | WaterLayer | AvoidLayer;
                    var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd, combinedMask);
                    var result = spaceState?.IntersectRay(query) ?? new Godot.Collections.Dictionary();

                    if (result.Count > 0)
                    {
                        rayHits++;
                        var collider = (CollisionObject3D)result["collider"];
                        uint hitLayer = collider.CollisionLayer;

                        if ((hitLayer & WaterLayer) != 0 || (hitLayer & AvoidLayer) != 0) 
                        {
                            waterAvoids++;
                            continue;
                        }

                        groundPos = (Vector3)result["position"];
                        normal = (Vector3)result["normal"];
                    }
                    else
                    {
                        continue;
                    }
                }

                // Create transform
                Transform3D t = Transform3D.Identity;
                
                // Rotation & Alignment
                if (AlignToNormal && normal.IsNormalized())
                {
                    Vector3 v3Up = normal;
                    Vector3 v3Forward = Vector3.Forward;
                    if (Mathf.Abs(v3Up.Dot(v3Forward)) > 0.99f)
                        v3Forward = Vector3.Right;
                    
                    Vector3 v3Right = v3Up.Cross(v3Forward).Normalized();
                    v3Forward = v3Right.Cross(v3Up).Normalized();
                    
                    t.Basis = new Basis(v3Right, v3Up, v3Forward);
                }
                
                t = t.RotatedLocal(Vector3.Up, _rng.RandfRange(0, Mathf.Pi * 2));
                
                // Scale
                float s = _rng.RandfRange(0.6f, 1.2f);
                t = t.ScaledLocal(new Vector3(s, s, s));
                
                // Position
                t.Origin = groundPos;

                transforms.Add(t);
                if (transforms.Count == 1) GD.Print($"GrassGenerator: First instance added at {groundPos}");
            }
        }

        multiMesh.UseCustomData = true;
        multiMesh.InstanceCount = transforms.Count;
        for (int i = 0; i < transforms.Count; i++)
        {
            multiMesh.SetInstanceTransform(i, transforms[i]);
            multiMesh.SetInstanceCustomData(i, new Color(_rng.Randf(), _rng.Randf(), 0, 0));
        }

        GD.Print($"GrassGenerator: Finished. Instances: {transforms.Count}, RayHits: {rayHits}, NoiseSkips: {noiseSkips}, DensitySkips: {densitySkips}, WaterAvoids: {waterAvoids}");
    }

    private void InitializeNoise()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = (int)GD.Randi();
        _noise.Frequency = Frequency;
        _noise.NoiseType = NoiseType;
    }
}
