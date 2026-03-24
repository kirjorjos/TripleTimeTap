using Godot;

public partial class QuitButton : Button {
	public void OnPressed() {
		GetTree().Quit();
	}
}
