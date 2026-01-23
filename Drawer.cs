using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Linq.Expressions;
public partial class Drawer : Node2D
{
	public int size = 32;
	public int index = 0;
	public Dictionary<Godot.Vector2, Pixel> pixels = new Dictionary<Godot.Vector2, Pixel>();
	public int start = 0;
	public Godot.Collections.Array<Color> values = new Godot.Collections.Array<Color>();
	public Godot.Vector2 coord;
	public PixelBoss boss;
	public bool hasChanged = false;
	public bool temphasChanged = false;
	bool firsgone = true;
	[Export]
	ShaderMaterial myShader;

	Pixel air = new Pixel(new Color(0,0,0,0),0,0);
	int wait = 0;
	int updateCount = 0;
	public bool hasGone = false;

	public Godot.Collections.Array<int> Xvals = new Godot.Collections.Array<int>();
	
	public override void _Ready()
	{
		
		int PixelCount = 48000;
		boss = (PixelBoss)GetParent();
		for(int x = 0; x < size; x++)
		{
			Xvals.Add(x);
			for(int y = 0; y < size; y++)
			{	

				pixels[new Godot.Vector2(x,y)] = new Pixel(boss.colSheet[start],start,start);
				values.Add(boss.colSheet[start]);
				PixelCount -= 1;
				if(PixelCount <= 0){

					start = 0;
					
				}
			}
		}
		Xvals.Shuffle();
		//GD.Print(values);
		myShader.SetShaderParameter("values",values);
		
		//QueueRedraw();
	}

	public override void _Process(double delta)
	{
		if (hasChanged && wait%2 == coord.Y%2)
		{	
			processPixels();

		}
		else if (hasChanged)
		{
			myShader.SetShaderParameter("values",values);
		}
		wait += 1;
		if(wait == 4)
		{
			wait = 0;
		}
	}

	public override void _Draw(){
		
		
		//for(int x = 0; x < size; x++)
		//{
		//	for(int y = 0; y < size; y++)
		//	{
		//		Rect2 newRect = new Rect2(new Godot.Vector2(x,y), new Godot.Vector2(1,1));
		//		int element = pixels[new Godot.Vector2(x,y)];
		//		DrawRect(newRect,boss.colSheet[element],true);
	//	}
		//}
		
		firsgone = false;
	}
	public void processPixels(){
		
		updateCount += 1;
		temphasChanged = false;
		for(int x = size; x > 0; x--)
		{
			for(int y = size; y > 0; y--)
			{
				processSingle(new Godot.Vector2(Xvals[x - 1],y - 1));
				
			}
		}
		hasChanged = temphasChanged;
		if(hasChanged){
			//Array<int> ints = (Array<int>)pixels.Values;
			
			//QueueRedraw();
		}
	}

	public void processSingle(Godot.Vector2 pixel){
		
		
		pixels[pixel].updateCount = updateCount;
		int result = pixels[pixel].pixelPhysType;
		if(result == 1)
		{	pixels[pixel].updateCount = 0;
			bool moveRes = movePixel(pixel, new Godot.Vector2(0,1), []);
			if (moveRes)
			{
				return;
			}
			if (boss.Swap(pixel, pixels[pixel], this, new Godot.Vector2(0,1)))
			{
				return;
			}
			moveRes = movePixel(pixel, new Godot.Vector2(1,0), [new Vector2(1,1)]);
			if (moveRes)
			{
				return;
			}
			moveRes = movePixel(pixel, new Godot.Vector2(-1,0),[new Vector2(-1,1)]);
		}

		if(result == 3)
		{	
			
			bool moveRes = movePixel(pixel, new Godot.Vector2(0,1), []);
			if (moveRes)
			{
				return;
			}
			if (boss.Swap(pixel, pixels[pixel], this, new Godot.Vector2(0,1)))
			{
				return;
			}
			Vector2 velo = pixels[pixel].velocity;
			Pixel myPixel = pixels[pixel];
			if((velo.X == 0.0 && updateCount%2 == 0) || velo.X ==1.0){
				moveRes = movePixel(pixel, new Godot.Vector2(1,0), []);
				if (moveRes)
				{
					myPixel.velocity = new Godot.Vector2(1,0);
					return;
				}
				moveRes = movePixel(pixel, new Godot.Vector2(-1,0),[]);
				if (moveRes)
				{
					myPixel.velocity = new Godot.Vector2(-1,0);
					return;
				}
			}
			else
			{
				moveRes = movePixel(pixel, new Godot.Vector2(-1,0),[]);
				if (moveRes)
				{
					myPixel.velocity = new Godot.Vector2(-1,0);
					return;
				}
				moveRes = movePixel(pixel, new Godot.Vector2(1,0), []);
				if (moveRes)
				{
					myPixel.velocity = new Godot.Vector2(1,0);
					return;
				}
			}
			myPixel.velocity = new Godot.Vector2(0,0);
			if(updateCount % 10 == 0)
			{
				boss.nearMix(pixel, pixels[pixel],this,new Vector2(1,0));
				boss.nearMix(pixel, pixels[pixel],this,new Vector2(-1,0));
			}
			
		}

	}
	

	public bool movePixel(Godot.Vector2 pixel, Godot.Vector2 direction, Array<Vector2> reqs){
		if(pixels.ContainsKey(pixel + direction)){
			if(pixels[pixel + direction].pixelPhysType == 0)
			{	
				if(reqs.Count > 0){
					for(int i = 0; i < reqs.Count; i++)
					{
						Vector2 currentReq = reqs[i];
						if(boss.getNeighborPixel(this, pixel, currentReq).pixelPhysType != 0)
						{
							return false;
						}
					}
				}
				
				pixels[pixel + direction] = pixels[pixel];
				pixels[pixel + direction].velocity = direction;
				
				int valueLoco = (int)pixel.X + (int)direction.X + (32 * (int)(pixel.Y + direction.Y));
				values[valueLoco] = pixels[pixel].pixelType;
				pixels[pixel] = air;
				valueLoco = (int)pixel.X + (32 * (int)(pixel.Y));
				values[valueLoco] = new Color(0,0,0,0);

				temphasChanged = true;			
				return true;
			}
		}
		else if (boss.transferPixel(this, pixel, direction, reqs))
		{
			pixels[pixel] = air;
			int valueLoco = (int)pixel.X + (32 * (int)(pixel.Y));
			values[valueLoco] = new Color(0,0,0,0);
			temphasChanged = true;
			return true;
		}
		return false;
	}
}
