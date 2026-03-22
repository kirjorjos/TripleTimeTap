using Godot;
using System.Collections.Generic;

public struct SceneryItem {
	public Vector2I AtlasPos;
	public Vector2I Size;

	public SceneryItem(Vector2I pos, Vector2I size) {
		AtlasPos = pos;
		Size = size;
	}
}

public interface TimeWorld {
	void TickLoop(GlobalEvents.Actions currentAction);
	Dictionary<string, Vector2I[]> Palette { get; }
	List<SceneryItem> Scenery { get; }
	TileMapLayer tileMap { get; }
}