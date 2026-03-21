using Godot;
using System;

public partial class BlueButton001 : TextureButton {

	private int score = 0;
	Label label;
	private double lastTapTime = 0;
	private const double DoubleTapSecDelay = 0.25;
	private bool isActionHeld = false;
	private bool isDoubleTapping = false;
	private SceneTreeTimer clickTimer;

	enum Actions {
		cycleTime,
		jump,
		roll,
		clear
	}
	Actions lastAction;

	public override void _Ready() {
		label = GetNode<Label>("Label");
	}


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
		switch(currentAction) {
			case Actions.cycleTime:
				if (lastAction != currentAction) GD.Print("Time cycle called");
				break;
			case Actions.jump:
				label.Text = "Jump held";
				if (lastAction != currentAction) GD.Print("Jump called");
				break;
			case Actions.roll:
				label.Text = "Roll held";
				if (lastAction != currentAction) GD.Print("Roll called");
				break;
			case Actions.clear:
				label.Text = "Nothing held";
				if (lastAction != currentAction) GD.Print("Nothing called");
				break;
		}
		lastAction = currentAction;
	}
}
