using Godot;
using System;

public static class Extensions
{
	public static int NumSubtiles(this TileMap tileMap, int tileID)
		=> tileMap.TileSet.TileGetShapes(tileID).Count;

	public static Vector2 GetSubtileCoord(this TileMap tileMap, int tileID, int subtileID)
		=> (Vector2)((Godot.Collections.Dictionary) tileMap.TileSet.TileGetShapes(tileID)[subtileID])["autotile_coord"];

	public static Rect2 GetSubtileRegion(this TileMap tileMap, int tileID, int subtileID)
	{
		var autotileOffset = tileMap.GetSubtileCoord(tileID, subtileID);
		var tileSize = tileMap.TileSet.AutotileGetSize(tileID);
		var position = new Vector2(tileSize.x * autotileOffset.x, tileSize.y * autotileOffset.y);
		return new Rect2(position, tileSize);
	}
	public static int? GetNumberPressed(this InputEvent inputEvent) =>
		inputEvent is InputEventKey eventKey
			? GetNumberPressed(eventKey)
			: null;

	public static int? GetNumberPressed(this InputEventKey eventKey)
	{
		if (!eventKey.Pressed)
			return null;

		return eventKey.Scancode.TryNumberKeyScancodeToInt();
	}

	public static int? TryNumberKeyScancodeToInt(this uint n)
	{
		switch (n)
		{
			case (int)KeyList.Key0: 
			case (int)KeyList.Kp0: 
				return 0;

			case (int)KeyList.Key1: 
			case (int)KeyList.Kp1: 
				return 1;

			case (int)KeyList.Key2: 
			case (int)KeyList.Kp2: 
				return 2;

			case (int)KeyList.Key3: 
			case (int)KeyList.Kp3: 
				return 3;
				
			case (int)KeyList.Key4: 
			case (int)KeyList.Kp4: 
				return 4;

			case (int)KeyList.Key5: 
			case (int)KeyList.Kp5: 
				return 5;

			case (int)KeyList.Key6: 
			case (int)KeyList.Kp6: 
				return 6;

			case (int)KeyList.Key7: 
			case (int)KeyList.Kp7: 
				return 7;

			case (int)KeyList.Key8: 
			case (int)KeyList.Kp8: 
				return 8;

			case (int)KeyList.Key9: 
			case (int)KeyList.Kp9: 
				return 9;

			default: return null;
		}
	}

	public static float DegreesToRadians(this float degrees)
		=> degrees / 180 * (float) Math.PI;

	public static Vector2 Clamp(this Vector2 v, float minX, float maxX, float minY, float maxY)
		=> new Vector2(v.x.Clamp(minX, maxX), v.y.Clamp(minY, maxY));

	public static float Clamp(this float v, float min, float max)
		=> Math.Max(min, Math.Min(v, max));

}
