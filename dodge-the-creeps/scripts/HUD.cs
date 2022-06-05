using Godot;
using System;

public class HUD : CanvasLayer
{
    [Signal]
    public delegate void StartGame(); // StartButton has been pressed

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNode<ProgressBar>("EnergyBar").Hide();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void ShowMessage(string text)
    {
        var message = GetNode<Label>("Message");
        message.Text = text;
        message.Show();

        GetNode<Timer>("MessageTimer").Start();
    }

    async public void ShowGameOver()
    {
        ShowMessage("Game Over");

        var messageTimer = GetNode<Timer>("MessageTimer");
        await ToSignal(messageTimer, "timeout");

        var message = GetNode<Label>("Message");
        message.Text = "Dodge the Creeps!";
        message.Show();

        await ToSignal(GetTree().CreateTimer(1), "timeout");
        GetNode<ProgressBar>("EnergyBar").Hide();
        GetNode<Button>("StartButton").Show();
    }

    // This function is called by Main whenever the score changes.
    public void UpdateScore(int score)
    {
        GetNode<Label>("ScoreLabel").Text = score.ToString();
    }

    public void UpdatePlayerSpeedLabel(int speed)
    {
        string text = "PLAYER SPEED: " + speed.ToString();
        GetNode<Label>("PlayerSpeedLabel").Text = text;
    }

    public void UpdateCreepMinSpeedLabel(float speed)
    {
        string text = "CREEP MIN SPEED: " + speed.ToString();
        GetNode<Label>("CreepMinSpeedLabel").Text = text;
    }

    public void UpdateCreepMaxSpeedLabel(float speed)
    {
        string text = "CREEP MAX SPEED: " + speed.ToString();
        GetNode<Label>("CreepMaxSpeedLabel").Text = text;
    }

    public void UpdateCreepSpawnRateLabel(float spawnRate)
    {
        string text = "CREEP SPAWN RATE: " + spawnRate.ToString() + "s";
        GetNode<Label>("CreepSpawnRateLabel").Text = text;
    }

    public void OnStartButtonPressed()
    {
        GetNode<Button>("StartButton").Hide();
        GetNode<ProgressBar>("EnergyBar").Show();
        EmitSignal("StartGame");
    }

    public void OnMessageTimerTimeout()
    {
        GetNode<Label>("Message").Hide();
    }

    public void OnUpdateEnergyBar(double energy){
        GetNode<ProgressBar>("EnergyBar").Value = energy;
    }
}
