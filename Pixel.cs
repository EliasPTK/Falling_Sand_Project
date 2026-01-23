using Godot;
using System;

public partial class Pixel: GodotObject 
{
	public Color pixelType = new Color(0,0,0,0);
	public int pixelPhysType = 0;
	public Vector2 velocity = new Vector2(0,0);
	public int updateCount = 0;
	public float density = 0;

	public Pixel(Color type, int subtype, float vardensity)
	{
		pixelType = type;
		pixelPhysType = subtype;
		density = vardensity;
	}
	
}
