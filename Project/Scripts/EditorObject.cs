using Godot;
using System;

public class EditorObject : Node2D
{
	// Use this to prevent swapping out of edit mode without a player spawn set
	private bool canPlayLevel = false; 
	private bool canPlace = true;
	private bool isPanning = false;
	private Node2D level;
	private Node2D camContainer;
	private Camera2D editorCamera;
	private TileMap tileMap;
	
	private PackedScene playerScene;
	private Player playerNode;

	private Sprite currentMouseSprite = new Sprite() ;
	private Texture tileSpriteForMouse;
	private Texture playerSpriteForMouse;

	private Vector2 levelSpawnPoint = new Vector2(0, 0);

	private bool isPlacingPlayer = false;
	private bool isPlacingTile = false;
	private const int tileID = 0; // This won't change unless we implement a sprite sheet

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

		editorCamera = camContainer.GetNode<Camera2D>("Camera");
		editorCamera.Current = true;

		tileMap = level.GetNode<TileMap>("TileMap");
		systemPopup = GetNode<FileDialog>("/root/LevelEditor/UI/FileDialog");

		tileSpriteForMouse = (Texture)GD.Load("res://Resources/Images/LevelEditorBlock.png");
		playerSpriteForMouse = (Texture)GD.Load("res://Resources/Images/hippie_idle_0.png");

		Color temp = currentMouseSprite.Modulate;
		temp.a = 0.5F;
		currentMouseSprite.Modulate = temp;
		AddChild(currentMouseSprite, true);
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

			moveEditor();
		}

		if (Input.IsActionPressed("save_level"))
		{
			Global.fileDialogShowing = true;
			fileBoxMode = PopupMode.SAVE;
			systemPopup.Mode = FileDialog.ModeEnum.SaveFile;
			systemPopup.Show();
		}
		
		if (Input.IsActionPressed("open_level"))
		{
			Global.fileDialogShowing = true;
			fileBoxMode = PopupMode.LOAD;
			systemPopup.Mode = FileDialog.ModeEnum.OpenFile;
			systemPopup.Show();
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
					camContainer.GlobalPosition -= new Vector2((referencePoint.x - relativeMousePoint.x), (referencePoint.y - relativeMousePoint.y));
				}
			}

			//Key input events
			if (inputEvent is InputEventKey key)
		{
			// Code to select tile to be placed
			if (key.IsPressed() && key.Scancode == (int)KeyList.T)
			{
				currentMouseSprite.Scale = new Vector2(1, 1);
				currentMouseSprite.Texture = tileSpriteForMouse;
				isPlacingTile = true;
			}
			
			// Code to select player to be placed
			if (key.IsPressed() && key.Scancode == (int)KeyList.P)
			{
				isPlacingTile = false;
				isPlacingPlayer = true;
				currentMouseSprite.Scale = new Vector2(-2, 2);
				currentMouseSprite.Texture = playerSpriteForMouse;
			}

			if (key.IsPressed() && key.Scancode == (int)KeyList.Delete)
			{
				currentMouseSprite.Texture = null;
				isPlacingPlayer = false;
				isPlacingTile = false;
			}
		}
		}
	}

	private void placePlayer()
	{
		if(playerNode == null)
		{
			playerNode = (Player)playerScene.Instance();
			GetNode("/root/LevelEditor/Level/PlayerNode").AddChild(playerNode);
			canPlayLevel = true;
		}
        playerNode.Owner = level;
		playerNode.Position = levelSpawnPoint;
		isPlacingPlayer = false;
		currentMouseSprite.Texture = null;
	}

	private void placeTile()
	{
		Vector2 mousePos = tileMap.WorldToMap(GetGlobalMousePosition());
		tileMap.SetCell((int)mousePos.x, (int)mousePos.y, tileID);
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
    }
}