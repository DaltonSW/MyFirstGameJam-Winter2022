using Godot;
using System;

public class EditorObject : Node2D
{
	// Use this to prevent swapping out of edit mode without a player spawn set
	private bool canPlayLevel = false; 
	public bool canPlace = true;
	private bool isPanning = false;
	private Node2D level;
	private Node2D camContainer;
	private Camera2D editorCamera;
	private Camera2D playerCamera;
	private TileMap tileMap;
	
	private PackedScene playerScene;
	private Player playerNode;

	public Sprite currentMouseSprite = new Sprite();
	private Texture tileSpriteForMouse;
	private Texture playerSpriteForMouse;

	public Vector2 levelSpawnPoint;

	public bool isPlacingPlayer = false;
	public bool isPlacingTile = false;
	private const int tileID = 0; // This won't change unless we implement a sprite sheet
	private int subtileID = 0;

	private Vector2 relativeMousePoint = new Vector2(0, 0);

	private int CAM_SPEED = 5;

	public PopupMode fileBoxMode = PopupMode.NULL;
	public FileDialog systemPopup;

	public enum PopupMode
	{
		NULL = -1,
		SAVE = 0,
		LOAD = 1,
	}

	public override void _Ready()
	{
		playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");

		level = GetNode<Node2D>("/root/LevelEditor/Level");
		camContainer = GetNode<Node2D>("/root/LevelEditor/CamContainer");

		playerCamera = camContainer.GetNode<Camera2D>("PlayerCamera");
		editorCamera = camContainer.GetNode<Camera2D>("EditorCamera");
		editorCamera.Current = true;

		tileMap = level.GetNode<TileMap>("TileMap");
		systemPopup = GetNode<FileDialog>("/root/LevelEditor/UI/FileDialog");

		tileSpriteForMouse = (Texture)GD.Load("res://Resources/Images/LevelEditorBlocks.png");
		playerSpriteForMouse = (Texture)GD.Load("res://Resources/Images/hippie_idle_0.png");

		Color temp = currentMouseSprite.Modulate;
		temp.a = 0.5F;
		currentMouseSprite.Modulate = temp;
		AddChild(currentMouseSprite, true);

		playerNode = (Player)playerScene.Instance();
		GetNode("/root/LevelEditor/Level/PlayerNode").AddChild(playerNode);
		playerNode.Hide();
	}

	public override void _Process(float delta)
	{
		GlobalPosition = GetGlobalMousePosition();

		if(!Global.fileDialogShowing)
		{
			if(currentMouseSprite.Texture != null)
			{
				currentMouseSprite.GlobalPosition = GetGlobalMousePosition();
			}

			if (Input.IsActionPressed("mb_middle"))
			{
				isPanning = true;
				relativeMousePoint = GetViewport().GetMousePosition();
			}

			else
			{
				isPanning = false;
			}

			if (Input.IsActionPressed("mb_left") && isPlacingTile && canPlace)
			{
				placeTile();
			}

			if (Input.IsActionPressed("mb_right"))
			{
				removeTile();
			}

			if (!Global.isPlaying)
			{
				moveEditor();
			}
		}

		if (Input.IsActionPressed("save_level"))
		{
			Global.fileDialogShowing = true;
			fileBoxMode = PopupMode.SAVE;
			systemPopup.Mode = FileDialog.ModeEnum.SaveFile;
			systemPopup.Show();
			GetNode<Control>("/root/LevelEditor/UI/DebugControls").Hide();
			GetNode<Label>("/root/LevelEditor/UI/BasicInstructions").Hide();
		}
		
		if (Input.IsActionPressed("open_level"))
		{
			Global.fileDialogShowing = true;
			fileBoxMode = PopupMode.LOAD;
			systemPopup.Mode = FileDialog.ModeEnum.OpenFile;
			systemPopup.Show();
			GetNode<Control>("/root/LevelEditor/UI/DebugControl").Hide();
			GetNode<Label>("/root/LevelEditor/UI/BasicInstructions").Hide();
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if(!Global.fileDialogShowing)
		{
			// Mouse button events
			if (inputEvent is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.IsPressed())
				{
					if (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp)
					{
						editorCamera.Zoom -= new Vector2(0.1F, 0.1F);
					}

					if (mouseEvent.ButtonIndex == (int)ButtonList.WheelDown)
					{
						editorCamera.Zoom += new Vector2(0.1F, 0.1F);
					}

					if (mouseEvent.ButtonIndex == (int)ButtonList.Left)
					{
						if (isPlacingPlayer && canPlace)
						{
							levelSpawnPoint = GetGlobalMousePosition();
							placePlayer();
						}
					}

					if (mouseEvent.ButtonIndex == (int)ButtonList.Right)
					{
						if (isPlacingPlayer && canPlace)
						{
							if(playerNode != null)
							{
								placePlayer();
							}
							currentMouseSprite.Texture = null;
						}

						else
						{
							removeTile();
						}
					}
				}
			}

			//Mouse motion events
			if (inputEvent is InputEventMouseMotion mouseMotion)
			{
				if (isPanning)
				{
					Vector2 referencePoint = GetViewport().GetMousePosition();
					editorCamera.GlobalPosition -= new Vector2((referencePoint.x - relativeMousePoint.x), (referencePoint.y - relativeMousePoint.y));
				}
			}

			//Key input events
			if (inputEvent is InputEventKey key)
			{
				// Code to select tile to be placed
				if (key.IsPressed() && key.Scancode == (int)KeyList.T)
				{
					equipTile();
				}

				// Code to select player to be placed
				if (key.IsPressed() && key.Scancode == (int)KeyList.P)
				{
					isPlacingTile = false;
					isPlacingPlayer = true;
					currentMouseSprite.Scale = new Vector2(-2, 2);
					currentMouseSprite.Texture = playerSpriteForMouse;
					currentMouseSprite.RegionEnabled = false;
				}

				if (key.IsPressed() && key.Scancode == (int)KeyList.Delete)
				{
					currentMouseSprite.Texture = null;
					isPlacingPlayer = false;
					isPlacingTile = false;
				}

				int? maybeNumPressed = key.GetNumberPressed();
				if (maybeNumPressed is int numPressed
					&& 1 <= numPressed && numPressed <= tileMap.NumSubtiles(tileID))
				{
					subtileID = numPressed - 1;
					equipTile();
				}
			}
		}
	}

	private void equipTile()
	{
		currentMouseSprite.Scale = new Vector2(2, 2);
		currentMouseSprite.Texture = tileSpriteForMouse;
		currentMouseSprite.RegionRect = tileMap.GetSubtileRegion(tileID, subtileID);
		currentMouseSprite.RegionEnabled = true;
		isPlacingTile = true;
	}

	private void placePlayer()
	{
		if(playerNode == null)
		{
			playerNode = (Player)playerScene.Instance();
			GetNode("/root/LevelEditor/Level/PlayerNode").AddChild(playerNode);
			canPlayLevel = true;
			GetNode<PlayerCamera>("/root/LevelEditor/CamContainer/PlayerCamera").loadPlayer();
		}
		playerNode.Show();
		levelSpawnPoint = GetGlobalMousePosition();
		playerNode.Owner = level;
		playerNode.Position = levelSpawnPoint;
		isPlacingPlayer = false;
		currentMouseSprite.Texture = null;
		Position2D levelSpawn = level.GetNode<Position2D>("SpawnPoint");
		levelSpawn.Position = levelSpawnPoint;
	}

	private void placeTile()
	{
		Vector2 mousePos = tileMap.WorldToMap(GetGlobalMousePosition());
		tileMap.SetCell((int)(mousePos.x / tileMap.Scale.x), (int)(mousePos.y / tileMap.Scale.y), tileID, autotileCoord: tileMap.GetSubtileCoord(tileID, subtileID));
	}

	private void removeTile()
	{
		Vector2 mousePos = tileMap.WorldToMap(GetGlobalMousePosition());
		tileMap.SetCell((int)mousePos.x, (int)mousePos.y, -1);
	}

	private void moveEditor()
	{
		if (Input.IsActionPressed("w"))
		{
			camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x, camContainer.GlobalPosition.y - CAM_SPEED);
		}
		
		if (Input.IsActionPressed("a"))
		{
			camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x - CAM_SPEED, camContainer.GlobalPosition.y);
		}
		
		if (Input.IsActionPressed("s"))
		{
			camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x, camContainer.GlobalPosition.y + CAM_SPEED);
		}
		
		if (Input.IsActionPressed("d"))
		{
			camContainer.GlobalPosition = new Vector2(camContainer.GlobalPosition.x + CAM_SPEED, camContainer.GlobalPosition.y);
		}
	}

	public void saveLevel()
	{
		PackedScene toSave = new PackedScene();
		tileMap.Owner = level;
		level.GetNode("PlayerNode").Owner = level;
		toSave.Pack(level);
		ResourceSaver.Save(systemPopup.CurrentPath, toSave);
	}

	public void loadLevel()
	{
		PackedScene toLoad = new PackedScene();
		toLoad = (PackedScene)ResourceLoader.Load(systemPopup.CurrentPath);
		Node2D newLevel = (Node2D)toLoad.Instance();
		GetParent().RemoveChild(level);
		level.QueueFree();
		GetParent().AddChild(newLevel);
		tileMap = GetParent().GetNode<TileMap>("Level/TileMap");
		level = newLevel;
		Position2D spawnPoint = new Position2D();
		spawnPoint.GlobalPosition = levelSpawnPoint;
		level.AddChild(spawnPoint);
	}

	public void swapCameras()
	{
		if(editorCamera.Current)
		{
			playerCamera.Current = true;
			editorCamera.Current = false;
		}

		else if(playerCamera.Current)
		{
			playerCamera.Current = false;
			editorCamera.Current = true;
		}
	}
}

public static class TileMapExtensions
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
}
