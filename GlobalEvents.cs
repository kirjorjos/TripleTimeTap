using Godot;
using System;

public partial class GlobalEvents : Node {
	private static int MinPlatformHeight = 2;
	private static int MaxPlatformHeight = 6;
	private static int MinPlatformWidth = 5;
	private static int MaxPlatformWidth = 30;
	private bool isActionHeld;
	private Actions lastAction;
	public static Past currentWorld;
	public static double runSpeed = 275.0;
	private const double jumpStrength = -400.0;
	private const double gravity = 1400.0;
	private const float collisionSnapTolerance = 0.05f;
	private double yVelocity = 0.0;
	private bool isRolling = false;
	private bool isMovementPaused = false;
	private static Random rng = new Random();
	private static int nextSpawnX = 0;
	private static int lastY = 0;
	private static int lastCleanupX = -100;
	private static int lowestPlatformY;

	public enum Actions {
		jump,
		roll,
		clear
	}

public struct SceneryItem {
	public Vector2I AtlasPos;
	public Vector2I Size;

	public SceneryItem(Vector2I pos, Vector2I size) {
		AtlasPos = pos;
		Size = size;
	}
}


	public override void _Input(InputEvent @event) {
		if (currentWorld == null || isMovementPaused) return;

		if (@event.IsActionPressed("MainButton")) {
			CharacterBody2D player = currentWorld.GetNode<CharacterBody2D>("Player");
			bool isGrounded = player.TestMove(player.GlobalTransform, new Vector2(0, 1));

			isActionHeld = true;
			if (isGrounded) {
				TickLoop(Actions.jump);
			} else {
				isRolling = true;
			}
		} else if (@event.IsActionReleased("MainButton")) {
			if (!isRolling) {
				TickLoop(Actions.jump);
			}

			isActionHeld = false;
			isRolling = false;
		}
	}

	public override void _Process(double delta) {
		if (currentWorld == null || isMovementPaused) return;
		currentWorld.TickLoop(isRolling);

		if (isActionHeld && isRolling) {
			TickLoop(Actions.roll);
		} else {
			TickLoop(Actions.clear);
		}
		CharacterBody2D player = currentWorld.GetNode<CharacterBody2D>("Player");
		AnimatedSprite2D animation = player.GetNode<AnimatedSprite2D>("PlayerSprite");

		bool isGrounded = player.TestMove(player.GlobalTransform, new Vector2(0, 1));
		if (!isGrounded) {
			yVelocity += gravity * delta;
			if (!isRolling && animation.Animation != "Jump") {
				animation.Play("Jump");
			}
		} else {
			if (yVelocity > 0) yVelocity = 0;
		}

		if (isRolling) {
			animation.Play("Roll");
		} else if (isGrounded) {
			animation.Play("Run");
		}
		Vector2 desiredMovement = new Vector2((float)runSpeed * (float)delta, (float)yVelocity * (float)delta);
		Vector2 resolvedMovement = ResolveMovement(player, desiredMovement);
		if (resolvedMovement.X < desiredMovement.X) {
			GD.Print("wall hit");
		}
		if (resolvedMovement.Y != desiredMovement.Y) {
			yVelocity = 0;
		}

		foreach (Node child in currentWorld.GetChildren()) {
			if (child is TileMapLayer layer) {
				layer.Position -= resolvedMovement;
			}
		}

		// Platform Generation
		if (currentWorld is Past timeWorld) {
			Vector2I playerTilePos = timeWorld.tileMap.LocalToMap(timeWorld.tileMap.ToLocal(player.GlobalPosition));
			if (playerTilePos.Y > lowestPlatformY + MaxPlatformHeight+2) {
				PauseMovement();
				if (currentWorld is Past pastWorld) {
					pastWorld.ShowControlOverlay();
				}
				return;
			}

			if (playerTilePos.X + 15 > nextSpawnX) {
				int gap = rng.Next(3, 7);
				int yShift = rng.Next(-2, 3);
				MakeMetaTile(new Vector2I(nextSpawnX + gap, lastY + yShift), timeWorld);
			}

			// Platform Cleanup
			int cleanupThreshold = playerTilePos.X - 10;
			if (cleanupThreshold > lastCleanupX) {
				for (int x = lastCleanupX; x < cleanupThreshold; x++) {
					for (int y = -20; y < 20; y++) {
						timeWorld.tileMap.SetCell(new Vector2I(x, y), -1);
					}
				}
				lastCleanupX = cleanupThreshold;
			}
		}
	}

	private void TickLoop(Actions currentAction) {
		if (isMovementPaused) return;

		CharacterBody2D player = currentWorld.GetNode<CharacterBody2D>("Player");
		CollisionShape2D collision = player.GetNode<CollisionShape2D>("PlayerCollision");

		bool isGrounded = player.TestMove(player.GlobalTransform, new Vector2(0, 1));

		switch(currentAction) {
			case Actions.jump:
				if (isGrounded) {
					yVelocity = jumpStrength;
				}
				break;
			case Actions.roll:
				if (!isRolling) {
					isRolling = true;
					collision.Scale = new Vector2(1, 0.5f);
				}
				break;
			case Actions.clear:
				if (isRolling) {
					isRolling = false;
					collision.Scale = new Vector2(1, 1.0f);
				}
				break;
		}
		lastAction = currentAction;
	}


	public static Vector2I MakeMetaTile(Vector2I topLeftPos, Past world) {
		TileMapLayer tileMap = world.tileMap;

		int width = rng.Next(MinPlatformWidth, MaxPlatformWidth);
		int height = rng.Next(MinPlatformHeight, MaxPlatformHeight);
		int sourceId = 0;

		var palette = world.Palette;
		topLeftPos = FindOpenPlatformPosition(topLeftPos, width, height, tileMap);
		GD.Print($"Generating platform at {topLeftPos} (Width: {width})");

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

		lowestPlatformY = Math.Max(lowestPlatformY, topLeftPos.Y + height - 1);
		nextSpawnX = topLeftPos.X + width;
		lastY = topLeftPos.Y;

		return new Vector2I(width, height);
	}

	private static Vector2I FindOpenPlatformPosition(Vector2I topLeftPos, int width, int height, TileMapLayer tileMap) {
		while (!IsPlatformAreaClear(topLeftPos, width, height, tileMap)) {
			topLeftPos.X++;
		}

		return topLeftPos;
	}

	private static bool IsPlatformAreaClear(Vector2I topLeftPos, int width, int height, TileMapLayer tileMap) {
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Vector2I cellPos = new(topLeftPos.X + x, topLeftPos.Y + y);
				if (tileMap.GetCellSourceId(cellPos) != -1) {
					return false;
				}
			}
		}

		return true;
	}

	private Vector2 ResolveMovement(CharacterBody2D player, Vector2 desiredMovement) {
		float horizontalDistanceToPlatform = ResolveDirectionalMovement(player, new Vector2(desiredMovement.X, 0));
		Transform2D transformAfterHorizontalMove = player.GlobalTransform.TranslatedLocal(new Vector2(horizontalDistanceToPlatform, 0));
		float verticalDistanceToPlatform = ResolveDirectionalMovement(player, new Vector2(0, desiredMovement.Y), transformAfterHorizontalMove);

		return new Vector2(horizontalDistanceToPlatform, verticalDistanceToPlatform);
	}

	private float ResolveDirectionalMovement(CharacterBody2D player, Vector2 axisMovement, Transform2D? startingTransform = null) {
		if (Mathf.IsZeroApprox(axisMovement.X) && Mathf.IsZeroApprox(axisMovement.Y)) {
			return 0;
		}

		Transform2D testTransform = startingTransform ?? player.GlobalTransform;
		if (!player.TestMove(testTransform, axisMovement)) {
			return axisMovement.X != 0 ? axisMovement.X : axisMovement.Y;
		}

		float minAllowedDistance = 0;
		float maxAllowedDistance = axisMovement.X != 0 ? Mathf.Abs(axisMovement.X) : Mathf.Abs(axisMovement.Y);
		float movementDirection = Mathf.Sign(axisMovement.X != 0 ? axisMovement.X : axisMovement.Y);

		while (maxAllowedDistance - minAllowedDistance > collisionSnapTolerance) {
			float totalDistance = (minAllowedDistance + maxAllowedDistance) * 0.5f;
			Vector2 totalMovement = (axisMovement.X == 0)
				? new Vector2(0, totalDistance * movementDirection)
				: new Vector2(totalDistance * movementDirection, 0);

			if (player.TestMove(testTransform, totalMovement)) {
				maxAllowedDistance = totalDistance;
			} else {
				minAllowedDistance = totalDistance;
			}
		}

		return minAllowedDistance * movementDirection;
	}

	public static void ResetPlatformGeneration() {
		nextSpawnX = 0;
		lastY = 0;
		lastCleanupX = -100;
		lowestPlatformY = int.MinValue;
	}

	public void ResumeMovement() {
		isActionHeld = false;
		isRolling = false;
		yVelocity = 0;
		isMovementPaused = false;
	}

	private void PauseMovement() {
		isActionHeld = false;
		isRolling = false;
		yVelocity = 0;
		isMovementPaused = true;
	}
}
