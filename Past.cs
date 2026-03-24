using Godot;
using System;
using System.Collections.Generic;

public partial class Past : Node2D {
	public TileMapLayer tileMap { get; private set; }
	private CharacterBody2D player;
	private Control controlOverlay;
	private Label scoreNode;
	private int score = 0;
	public Dictionary<string, Vector2I[]> Palette { get; } = new Dictionary<string, Vector2I[]>() {
		{ "topLeft", new[] { new Vector2I(8, 0), new Vector2I(0, 3) } },
		{ "topRight", new[] { new Vector2I(11, 0), new Vector2I(7, 3) } },
		{ "bottomLeft", new[] { new Vector2I(8, 3) } },
		{ "bottomRight", new[] { new Vector2I(11, 3) } },
		{ "leftWall", new[] { new Vector2I(0, 0), new Vector2I(0, 1) } },
		{ "rightWall", new[] { new Vector2I(1, 0), new Vector2I(1, 1) } },
		{ "topMiddle", new[] { new Vector2I(1, 3), new Vector2I(2, 3), new Vector2I(3, 3), new Vector2I(4, 3), new Vector2I(5, 3), new Vector2I(6, 3), new Vector2I(9, 0), new Vector2I(10, 0) } },
		{ "bottomMiddle", new[] { new Vector2I(9, 3), new Vector2I(10, 3) } },
		{ "interior", new[] { new Vector2I(2, 0), new Vector2I(3, 0), new Vector2I(2, 1), new Vector2I(3, 1) } }
	};

	public List<GlobalEvents.SceneryItem> Scenery { get; } = new List<GlobalEvents.SceneryItem>() {
		new(new Vector2I(4, 0), new Vector2I(4, 3)), // Tree 1
		new(new Vector2I(8, 1), new Vector2I(4, 2)), // Tree 2
		new(new Vector2I(0, 2), new Vector2I(2, 1)), // Bush
		new(new Vector2I(2, 2), new Vector2I(2, 1))  // Rock
	};

	public override void _Ready() {
		GlobalEvents.currentWorld = this;
		tileMap = GetNode<TileMapLayer>("Layer0");
		player = GetNode<CharacterBody2D>("Player");
		controlOverlay = GetNode<Control>("CanvasLayer/Control/VBoxContainer");
		scoreNode = GetNode<Label>("CanvasLayer/Control/Score");

		TriggerPlatformInitialization();
	}

	public void TickLoop(bool isRolling) {
		score++;
		if (isRolling) score++; // double score when rolling
		scoreNode.Text = $"{score}";
	}

	public void ShowControlOverlay() {
		if (controlOverlay != null) {
			controlOverlay.Visible = true;
		}
	}

	public void HideControlOverlay() {
		if (controlOverlay != null) {
			controlOverlay.Visible = false;
		}
	}

	public void OnMainMenuButtonPressed() {
		GetTree().ChangeSceneToFile("res://Title.tscn");
	}

	public void OnRestartButtonPressed() {
		tileMap.Clear();
		tileMap.Position = Vector2.Zero;
		score = 0;

		GlobalEvents.ResetPlatformGeneration();
		HideControlOverlay();
		TriggerPlatformInitialization();

		GlobalEvents globalEvents = GetNodeOrNull<GlobalEvents>("/root/GlobalEvents");
		globalEvents.ResumeMovement();
	}

	private void TriggerPlatformInitialization() {
		if (player != null && tileMap != null) {
			tileMap.Position = Vector2.Zero;
			Vector2I playerTilePos = tileMap.LocalToMap(tileMap.ToLocal(player.GlobalPosition));
			
			int startX = playerTilePos.X - 1; 
			int startY = playerTilePos.Y + 1; 
			
			GlobalEvents.MakeMetaTile(new Vector2I(startX, startY), this);
		}
	}
}
