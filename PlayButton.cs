using Godot;
using System;

public partial class PlayButton : Button
{
	public void OnPressed() {
		GetTree().ChangeSceneToFile("res://Past.tscn");
	}
}
