using Godot;
using System;

public partial class GlobalEvents : Node
{
	private static int MinPlatformHeight = 2;
	private static int MaxPlatformHeight = 6;
	private static int MinPlatformWidth = 2;
	private static int MaxPlatformWidth = 15;
	private bool isActionHeld;
	private bool isDoubleTapping;
	private double lastTapTime;
	private SceneTreeTimer clickTimer;
	private Actions lastAction;
	public static Node2D currentWorld;

	public enum Actions {
		cycleTime,
		jump,
		roll,
		clear
	}

	public const double DoubleTapSecDelay = 0.25;

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("MainButton")) {
			double currentTime = Time.GetTicksMsec() / 1000.0;
			isActionHeld = true;

			if (currentTime - lastTapTime < DoubleTapSecDelay) {
				isDoubleTapping = true;
				lastTapTime = -1.0;
			} else {
				isDoubleTapping = false;
				lastTapTime = currentTime;
				
				clickTimer = GetTree().CreateTimer(DoubleTapSecDelay);
				clickTimer.Timeout += OnSingleTapTimeout;
			}
		} else if (@event.IsActionReleased("MainButton")) {
			isActionHeld = false;
		}
	}

	private void OnSingleTapTimeout() {
		if (!isDoubleTapping && !isActionHeld) TickLoop(Actions.cycleTime);
	}

	public override void _Process(double delta) {
		if (isActionHeld) {
			TickLoop(isDoubleTapping ? Actions.roll : Actions.jump);
		} else {
			TickLoop(Actions.clear);
		}
	}

	private void TickLoop(Actions currentAction) {
		// switch(currentAction) {
		// 	case Actions.cycleTime:
		// 		if (lastAction != currentAction) GD.Print("Time cycle called");
		// 		break;
		// 	case Actions.jump:
		// 		label.Text = "Jump held";
		// 		if (lastAction != currentAction) GD.Print("Jump called");
		// 		break;
		// 	case Actions.roll:
		// 		label.Text = "Roll held";
		// 		if (lastAction != currentAction) GD.Print("Roll called");
		// 		break;
		// 	case Actions.clear:
		// 		label.Text = "Nothing held";
		// 		if (lastAction != currentAction) GD.Print("Nothing called");
		// 		break;
		// }
		lastAction = currentAction;
	}


	public static Vector2I MakeMetaTile(Vector2I topLeftPos, TimeWorld world) {
		TileMapLayer tileMap = world.tileMap;

		Random rng = new();

		int width = rng.Next(MinPlatformWidth, MaxPlatformWidth);
		int height = rng.Next(MinPlatformHeight, MaxPlatformHeight);
		int sourceId = 0;

		var palette = world.Palette;

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Vector2I cellPos = new(topLeftPos.X + x, topLeftPos.Y + y);
				string type;
				int alternative = 0;

				if (y == 0) {
					if (x == 0) type = "topLeft";
					else if (x == width - 1) type = "topRight";
					else type = "topMiddle";
				} else if (y == height - 1) {
					if (x == 0) type = "bottomLeft";
					else if (x == width - 1) type = "bottomRight";
					else type = "bottomMiddle";
				} else {
					if (x == 0) type = "leftWall";
					else if (x == width - 1) type = "rightWall";
					else type = "interior";
				}

				if (palette.ContainsKey(type)) {
					Vector2I[] options = palette[type];
					Vector2I atlasPos = options[rng.Next(options.Length)];
					if (atlasPos.Y < 0) atlasPos.Y = 0;
					tileMap.SetCell(cellPos, sourceId, atlasPos, alternative);
				}
			}
		}

		// Scenery
		var scenery = world.Scenery;
		if (scenery != null && scenery.Count > 0) {
			for (int x = 1; x < width - 1; x++) {
				if (rng.NextDouble() > 0.4) continue;

				SceneryItem item = scenery[rng.Next(scenery.Count)];
				if (x + item.Size.X < width) {
					int verticalOffset = (item.Size.Y > 1) ? 1 : 0;
					tileMap.SetCell(new Vector2I(topLeftPos.X + x, topLeftPos.Y - item.Size.Y + verticalOffset), sourceId, item.AtlasPos, 0);
					x += item.Size.X; 
				}
			}
		}

		return new Vector2I(width, height);
	}
	}

