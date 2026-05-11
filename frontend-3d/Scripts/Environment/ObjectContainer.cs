using Godot;
using System;
using System.Collections.Generic;

public partial class ObjectContainer : Node3D
{
	private Queue<Node3D> _objects = new Queue<Node3D>();
	[Export] public int MaxObjects = 50;

	public override void _Ready()
	{
		TopLevel = true; 
	}

	public void AddObject(Node3D obj, Vector3 globalPosition)
	{
		if (obj == null) return;

		GD.Print($"ObjectContainer: Adding object {obj.Name} to container.");
		AddChild(obj);
		obj.GlobalPosition = globalPosition;

		_objects.Enqueue(obj);
		if (_objects.Count > MaxObjects)
		{
			RemoveOldest();
		}
	}

	public void RemoveOldest()
	{
		if (_objects.Count == 0) return;

		Node3D oldest = _objects.Dequeue();
		if (IsInstanceValid(oldest))
		{
			oldest.QueueFree();
		}
	}

	public void ClearAll()
	{
		while (_objects.Count > 0)
		{
			Node3D obj = _objects.Dequeue();
			if (IsInstanceValid(obj))
			{
				obj.QueueFree();
			}
		}
	}
}
