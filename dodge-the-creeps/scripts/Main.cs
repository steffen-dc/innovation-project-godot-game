using Godot;
using System;

public class Main : Node
{
    [Export]
    public PackedScene MobScene; // this will show up under script variables in the inspector of the Main scene (link mob scene with this)
    [Export]
    public PackedScene SpeedPotionScene;

    public int Score;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Randomize();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void GameOver() // This is triggered by the Hit signal of the Player scene
    {
        GetNode<Timer>("MobTimer").Stop();
        GetNode<Timer>("ScoreTimer").Stop();
        GetNode<Timer>("SpeedPotionTimer").Stop();
        GetNode<HUD>("HUD").ShowGameOver();
        GetNode<AudioStreamPlayer>("Music").Stop();
        GetNode<AudioStreamPlayer>("DeathSound").Play();
    }

    // linked to StartGame signal in HUD scene
    public void NewGame()
    {
        // Note that for calling Godot-provided methods with strings,
        // we have to use the original Godot snake_case name.
        GetTree().CallGroup("mobs", "queue_free");

        Score = 0;

        var player = GetNode<Player>("Player");
        var startPosition = GetNode<Position2D>("StartPosition");
        player.Start(startPosition.Position);

        GetNode<Timer>("StartTimer").Start();
        GetNode<AudioStreamPlayer>("Music").Play();

        var hud = GetNode<HUD>("HUD");
        hud.UpdateScore(Score);
        hud.ShowMessage("Get Ready!");
    }

    public void OnScoreTimerTimeout()
    {
        Score++;
        GetNode<HUD>("HUD").UpdateScore(Score);
    }

    public void OnStartTimerTimeout()
    {
        GetNode<Timer>("MobTimer").Start();
        GetNode<Timer>("ScoreTimer").Start();
        GetNode<Timer>("SpeedPotionTimer").Start();
    }

    public void OnMobTimerTimeout()
    {
        // Create a new instance of the Mob scene.
        var mob = (Mob)MobScene.Instance();

        // Choose a random location on Path2D.
        var mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
        mobSpawnLocation.Offset = GD.Randi();

        // Set the mob's direction perpendicular to the path direction.
        float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;

        // Set the mob's position to a random location.
        mob.Position = mobSpawnLocation.Position;

        // Add some randomness to the direction.
        direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
        mob.Rotation = direction;

        // increase velocity with score
        float minVelocity = 150.0f + (Score*3);
        float maxVelocity = 250.0f + (Score*3);
        var velocity = new Vector2((float)GD.RandRange(minVelocity, maxVelocity), 0);
        mob.LinearVelocity = velocity.Rotated(direction);

        // increase mob spawn rate the higher the score is
        float waitTime = 1.0f - (Score * 0.0016f);
        GetNode<Timer>("MobTimer").WaitTime = waitTime;

        //update info labels
        GetNode<HUD>("HUD").UpdateCreepMinSpeedLabel(minVelocity);
        GetNode<HUD>("HUD").UpdateCreepMaxSpeedLabel(maxVelocity);
        GetNode<HUD>("HUD").UpdateCreepSpawnRateLabel(waitTime);

        // Spawn the mob by adding it to the Main scene.
        AddChild(mob);
    }

    public void OnSpeedPotionTimerTimeout()
    {
        var potion = (SpeedPotion)SpeedPotionScene.Instance();

        potion.Position = new Vector2(GD.Randi() % 1080, GD.Randi() % 700);

        AddChild(potion);
    }
}
