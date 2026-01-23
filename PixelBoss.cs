using Godot;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public partial class PixelBoss : Node2D
{
	[Export]
	public Godot.Collections.Array<Color> colSheet { get; set; }
	Dictionary<Godot.Vector2, Drawer> World = new Dictionary<Godot.Vector2, Drawer>();
	public int WorldSize = 15;
	float waitMax = 0.00f;
	int currentWait = 0;

	Pixel sand = new Pixel(new Color(0,0,0,1),1,5);
	Pixel stone = new Pixel(new Color(0,0,0,1),2,6);

	Pixel water = new Pixel(new Color(0,0,0,1),3,1);

	Pixel blood = new Pixel(new Color(0,0,0,1),3,2);

	Pixel currentPixel = new Pixel(new Color(0,0,0,1),1,0);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sand.pixelType = colSheet[1];
		stone.pixelType = colSheet[2];
		water.pixelType = colSheet[3];
		blood.pixelType = colSheet[4];
		

		var drawscene = GD.Load<PackedScene>("res://drawer.tscn");
		for(int x = (WorldSize - 1); x >= 0; x--)
		{
			for(int y = (WorldSize - 1); y >= 0; y--)
			{
				Drawer inst = (Drawer)drawscene.Instantiate();
				
				if(y == 10)
				{
					inst.start = 2;
				}
				
				AddChild(inst);
				Vector2 coord = new Vector2(x,y);
				inst.GlobalPosition = coord *32;
				inst.coord = coord;
				World[coord] = inst;
			}
		}
		GD.Print(GetChildCount());
		Vector2 firstResult = drawerToWorld(World[new Vector2(1,1)], new Vector2(5,5));
		//Should be (37,37)
		GD.Print(firstResult);

		Vector2 secondResult = WhichWorld(new Vector2(37,37));
		//Should be (1,1)
		GD.Print(secondResult);

		Vector2 thirdResult = WorldtoDrawer(new Vector2(37,37));
		//Should be (5,5)
		GD.Print(thirdResult);
		Vector2 part1 = drawerToWorld(World[new Vector2(1,2)], new Vector2(30,31));
		GD.Print(part1);
		
		Vector2 lastResult = WorldtoDrawer(drawerToWorld(World[new Vector2(1,2)], new Vector2(30,31)) + new Vector2(0,1));
		GD.Print(lastResult);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if (Input.IsActionJustPressed("Space"))
		//{
		//	Update();
		//}
		//Update();
		//GD.Print(Engine.GetFramesPerSecond());
		if (Input.IsActionJustPressed("1"))
		{
			currentPixel = sand;
			((Label)GetNode("Label")).Text = "1";
		}
		if (Input.IsActionJustPressed("2"))
		{
			currentPixel = stone;
			((Label)GetNode("Label")).Text = "2";
		}
		if (Input.IsActionJustPressed("3"))
		{
			currentPixel = water;
			((Label)GetNode("Label")).Text = "3";
		}
		if (Input.IsActionJustPressed("4"))
		{
			currentPixel = blood;
			((Label)GetNode("Label")).Text = "4";
		}
		if (Input.IsActionPressed("Click"))
		{
			Paint(2, currentPixel);
		}
		
	}

	public bool Swap(Godot.Vector2 pixel, Pixel pixelObj, Drawer box, Vector2 dir)
	{
		Vector2 worldCoord = drawerToWorld(box, pixel);
		Vector2 newCoord = worldCoord + dir;
		Vector2 whichWorl = WhichWorld(newCoord);
		Drawer thisWorld = World[whichWorl];
		Vector2 localCoord = WorldtoDrawer(newCoord);
		Pixel otherPixel = thisWorld.pixels[localCoord];
		float density = otherPixel.density;
		if(otherPixel.pixelPhysType != 3)
		{
			return false;
		}
		if(pixelObj.density <= density)
		{
			return false;
		}
		
		Pixel tempPix = new Pixel(otherPixel.pixelType,otherPixel.pixelPhysType,density);
		thisWorld.pixels[localCoord] = new Pixel(pixelObj.pixelType,pixelObj.pixelPhysType,pixelObj.density);
		box.pixels[pixel] = tempPix;

		int valueLoco = (int)localCoord.X + (32 * (int)(localCoord.Y));
		thisWorld.values[valueLoco] = thisWorld.pixels[localCoord].pixelType;

		int othervalueLoco = (int)pixel.X + (32 * (int)(pixel.Y));
		box.values[othervalueLoco] = thisWorld.pixels[pixel].pixelType;

		box.hasChanged = true;
		box.temphasChanged = true;
		thisWorld.hasChanged = true;
		thisWorld.temphasChanged = true;
		if(pixelObj.pixelPhysType == 3)
		{
			Mix(pixel,pixelObj,box,localCoord,otherPixel,thisWorld);

			
			
		}

		return true;
	}
	
	public void nearMix(Godot.Vector2 pixel, Pixel pixelOBJ, Drawer box, Vector2 dir)
	{
		Vector2 worldCoord = drawerToWorld(box, pixel);
		Vector2 newCoord = worldCoord +dir;
		Vector2 whichWorl = WhichWorld(newCoord);
		Drawer thisWorld = World[whichWorl];
		Vector2 localCoord = WorldtoDrawer(newCoord); 
		if (!thisWorld.pixels.ContainsKey(localCoord))
		{
			GD.Print(localCoord);
		}
		Pixel otherPixel = thisWorld.pixels[localCoord];
		if(otherPixel.pixelPhysType != 3)
		{
			return;
		}
		otherPixel.pixelType = otherPixel.pixelType.Lerp(pixelOBJ.pixelType, 0.25f);
		otherPixel.density = (pixelOBJ.density + otherPixel.density)/2;
		int valueLoco = (int)localCoord.X + (32 * (int)(localCoord.Y));
		thisWorld.values[valueLoco] = otherPixel.pixelType;
		thisWorld.temphasChanged = true;
		thisWorld.hasChanged = true;

		pixelOBJ.pixelType = pixelOBJ.pixelType.Lerp(otherPixel.pixelType, 0.5f);
		pixelOBJ.density = (pixelOBJ.density + otherPixel.density)/2;
		valueLoco = (int)pixel.X + (32 * (int)(pixel.Y));
		box.values[valueLoco] = pixelOBJ.pixelType;
		box.temphasChanged = true;
		box.hasChanged = true;
	}

	public void Mix(Godot.Vector2 toppixel, Pixel toppixelObj, Drawer TopBox,Godot.Vector2 botpixel, Pixel botpixelObj, Drawer BottomBox)
	{
		
		
		botpixelObj.pixelType = botpixelObj.pixelType.Lerp(toppixelObj.pixelType, 0.5f);
		botpixelObj.density = (toppixelObj.density + botpixelObj.density)/2;
		int valueLoco = (int)botpixel.X + (32 * (int)(botpixel.Y));
		BottomBox.values[valueLoco] = botpixelObj.pixelType;
		BottomBox.hasChanged = true;

		toppixelObj.pixelType = botpixelObj.pixelType;
		toppixelObj.density = botpixelObj.density;
		valueLoco = (int)toppixel.X + (32 * (int)(toppixel.Y));
		TopBox.values[valueLoco] = toppixelObj.pixelType;
				
			
		
	}

	public void Paint(int size, Pixel pixel)
	{	
		Godot.Collections.Array<Vector2> points = [new Vector2(0,1),new Vector2(0,-1),new Vector2(1,0),new Vector2(-1,0),new Vector2(1,1),new Vector2(-1,1),new Vector2(1,-1),new Vector2(-1,-1)];
		Vector2 mousePos = GetGlobalMousePosition();
		for(int i = 1; i <= size;i++)
		{
			for(int x = 0; x < points.Count; x++)
			{
				Vector2 newPos = mousePos + points[x] * i;
				Vector2 paintedPos1 = WorldtoDrawer(newPos);
				Vector2 worldcoord1 = WhichWorld(newPos);
				Drawer drawnWorld1 = World[worldcoord1];
				drawnWorld1.pixels[paintedPos1] = new Pixel(pixel.pixelType,pixel.pixelPhysType,pixel.density);
				drawnWorld1.pixels[paintedPos1].updateCount = -5;
				int valueLoco1 = (int)paintedPos1.X + (32 * (int)(paintedPos1.Y));
				drawnWorld1.values[valueLoco1] = pixel.pixelType;
				drawnWorld1.hasChanged = true;
			}
		}
		Vector2 paintedPos = WorldtoDrawer(mousePos);
		Vector2 worldcoord = WhichWorld(mousePos);
		Drawer drawnWorld = World[worldcoord];
		drawnWorld.pixels[paintedPos] = new Pixel(pixel.pixelType,pixel.pixelPhysType,pixel.density);
		drawnWorld.pixels[paintedPos].updateCount = -5;
		int valueLoco = (int)paintedPos.X + (32 * (int)(paintedPos.Y));
		drawnWorld.values[valueLoco] = pixel.pixelType;
		drawnWorld.hasChanged = true;
	}
	public void Update()
	{
		
		for(int x = WorldSize; x > 0; x--)
		{
			for(int y = WorldSize; y > 0; y--)
			{	
				
				Drawer child = World[new Vector2(x-1, y-1)];
				if (child.hasChanged)
				{
					child.processPixels();
				}
			}
		}
		//await ToSignal(GetTree().CreateTimer(0.0005f), SceneTreeTimer.SignalName.Timeout);
		//Update();
	}	

	public Vector2 drawerToWorld(Drawer box,Vector2 pixel)
	{
		return pixel + (32 * box.coord);
	}

	public Vector2 WhichWorld(Vector2 worldcoord)
	{
		int xCoordRemainder = (int)worldcoord.X % 32;
		int yCoordRemainder = (int)worldcoord.Y % 32;
		int xCoord = ((int)worldcoord.X - xCoordRemainder)/32;
		int yCoord = ((int)worldcoord.Y - yCoordRemainder)/32;
		return new Vector2(xCoord,yCoord);
	}
	public Vector2 WorldtoDrawer(Vector2 worldcoord)
	{
		int xCoord = (int)worldcoord.X % 32;
		int yCoord = (int)worldcoord.Y % 32;
		if(xCoord < 0)
		{
			GD.Print("-1" + worldcoord.X.ToString());
		}
		return new Vector2(xCoord,yCoord);
	}

	public Pixel getNeighborPixel(Drawer box,Vector2 pixel, Vector2 direction)
	{
		Vector2 worldCoord = drawerToWorld(box,pixel) + direction;
		Vector2 coordOfWorld = WhichWorld(worldCoord);
		Vector2 localCoord = WorldtoDrawer(worldCoord);
		return World[coordOfWorld].pixels[localCoord];
	}
	public bool transferPixel(Drawer box,Vector2 pixel, Vector2 direction, Godot.Collections.Array<Vector2> reqs)
	{
		Vector2 nextBoxCoord = box.coord + direction;
		if(World.ContainsKey(nextBoxCoord) == false)
		{
			return false;
		}
		Drawer nextBox = World[nextBoxCoord];
		Vector2 newLoco = WorldtoDrawer(drawerToWorld(box, pixel) + direction);
		
		
		int boxResult = nextBox.pixels[newLoco].pixelPhysType;
		if(boxResult == 0){
			if(reqs.Count > 0){
				for(int i = 0; i < reqs.Count; i++)
					{
						Vector2 currentReq = reqs[i];
						if(getNeighborPixel(box, pixel, currentReq).pixelPhysType != 0)
						{
							return false;
						}
					}
			}
			nextBox.pixels[newLoco] = box.pixels[pixel];
			nextBox.pixels[newLoco].velocity = direction;
			nextBox.pixels[newLoco].updateCount = nextBox.pixels[newLoco].updateCount - 1;
			
			int valueLoco = (int)newLoco.X  + (32 * (int)(newLoco.Y));
			nextBox.values[valueLoco] = box.pixels[pixel].pixelType;
			nextBox.hasChanged = true;
			return true;
		}
		return false;
	}
}
