using Godot;
using System;

public partial class ShipController : Node
{
    [Export] public ObjectSpawner Spawner;
    public ObjectContainer Container;

    public override void _Ready()
    {
        if (Spawner != null && Container != null)
        {
            Spawner.ObjectSpawned += OnObjectSpawned;
            GD.Print("ShipController: Spawner and Container connected.");
        }
        else
        {
            GD.PushWarning("ShipController: Spawner or Container not assigned!");
        }
    }

    private void OnObjectSpawned(Node3D instance, Vector3 globalPosition)
    {
        GD.Print($"ShipController: Received spawn signal for {instance.Name} at {globalPosition}");
        if (Container != null)
        {
            Container.AddObject(instance, globalPosition);
        }
        else
        {
            GD.PushError("ShipController: Container is null, object freed.");
            instance.QueueFree();
        }
    }

    public void RequestDeletion()
    {
        Container?.RemoveOldest();
    }

    public void ClearAll()
    {
        Container?.ClearAll();
    }
}
