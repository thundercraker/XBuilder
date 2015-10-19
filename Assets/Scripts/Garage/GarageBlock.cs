using UnityEngine;
using System;

public class GarageBlock : IEquatable<GarageBlock>, IEquatable<Vector2> {
	public Vector3 position;
	public string name;
	public BlockObjectScript.BlockType type;

	public GarageBlock(Vector3 position, String name, BlockObjectScript.BlockType type)
	{
		this.position = position;
		this.name = name;
		this.type = type;
	}

	public bool Equals(GarageBlock gb)
	{
		return position.Equals(gb.position);
	}

	public bool Equals(Vector2 pos)
	{
		if (pos.x.Equals (position.x) && pos.y.Equals (position.y))
			return true;
		else
			return false;
	}
}