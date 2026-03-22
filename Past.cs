using Godot;
using System;
using System.Collections.Generic;

public partial class Past : Node2D, TimeWorld {
	public TileMapLayer tileMap { get; private set; }
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

	public List<SceneryItem> Scenery { get; } = new List<SceneryItem>() {
		new(new Vector2I(4, 0), new Vector2I(4, 3)), // Tree 1
		new(new Vector2I(8, 1), new Vector2I(4, 2)), // Tree 2
		new(new Vector2I(0, 2), new Vector2I(2, 1)), // Bush
		new(new Vector2I(2, 2), new Vector2I(2, 1))  // Rock
	};

	public override void _Ready() {
		GlobalEvents.currentWorld = this;
		tileMap = GetNodeOrNull<TileMapLayer>("Layer0");

		CharacterBody2D player = GetNodeOrNull<CharacterBody2D>("CharacterBody2D");
		if (player != null && tileMap != null) {
			Vector2I playerTilePos = tileMap.LocalToMap(tileMap.ToLocal(player.GlobalPosition));
			
			int startX = playerTilePos.X - 1; 
			int startY = playerTilePos.Y + 1; 
			
			GlobalEvents.MakeMetaTile(new Vector2I(startX, startY), this);
		}
	}

	public void TickLoop(GlobalEvents.Actions currentAction) {
		
	}
}
