using Godot;
using System;

public class Player : Area2D
{
    // This defines a custom signal called "hit" that we will have our player emit (send out) when it collides with an enemy
    [Signal]
    public delegate void Hit();
    [Signal]
    public delegate void UpdateEnergyBar(double energy);

    [Export]
    public int Speed = 400;
    public Vector2 ScreenSize;

    private double _energy = 100;
    private int _exhaustDuration = 0;
    private bool _spinning;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Hide(); // hide player on game start
        ScreenSize = GetViewportRect().Size;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        var velocity = Vector2.Zero; // The player's movement vector. (by default, the player should not be moving)

        if (_exhaustDuration > 0) _exhaustDuration--;

        if (Input.IsActionPressed("spin_attack") && _energy > 0)
        {
            DoSpinAttack();
        }
        else
        {
            RotationDegrees = 0;
            _spinning = false;
        }

        if (Input.IsActionPressed("move_right"))
        {
            velocity.x += 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            velocity.x -= 1;
        }

        if (Input.IsActionPressed("move_down"))
        {
            velocity.y += 1;
        }

        if (Input.IsActionPressed("move_up"))
        {
            velocity.y -= 1;
        }

        var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");


        // check whether the player is moving so we can call play() or stop() on the AnimatedSprite.
        if (velocity.Length() > 0)
        {
            // normalize speed for when moving diagonally (else the player moves faster diagonally)
            velocity = velocity.Normalized() * Speed;
            animatedSprite.Play();
        }
        else
        {
            animatedSprite.Stop();
        }


        // update the player's position
        // use clamp() to prevent it from leaving the screen. Clamping a value means restricting it to a given range.
        Position += velocity * delta;
        Position = new Vector2(
            x: Mathf.Clamp(Position.x, 0, ScreenSize.x),
            y: Mathf.Clamp(Position.y, 0, ScreenSize.y)
        );


        // update animation depending on direction
        if (velocity.x != 0)
        {
            animatedSprite.Animation = "walk";
            animatedSprite.FlipV = false;
            animatedSprite.FlipH = velocity.x < 0;
        }
        else if (velocity.y != 0)
        {
            animatedSprite.Animation = "up";
            animatedSprite.FlipV = velocity.y > 0;
        }
    }


    // Each time an enemy hits the player, the (Hit) signal is going to be emitted.We need to disable the player's collision so that we don't trigger the hit signal more than once.
    public void OnPlayerBodyEntered(PhysicsBody2D body)
    {
        if(_spinning) return;

        Hide(); // Player disappears after being hit.
        EmitSignal(nameof(Hit));

        // Must be deferred as we can't change physics properties on a physics callback.
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    }

    public void OnPlayerAreaEntered(Area2D area)
    {
        if (area is SpeedPotion)
        {
            area.QueueFree();
        }
    }

    public void Start(Vector2 pos)
    {
        Position = pos;
        Show();
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        
        _energy = 100;
        EmitSignal(nameof(UpdateEnergyBar), _energy);
    }

    public void DoSpinAttack(){
        _spinning = true;
        _energy -= 0.25;
        if (_energy <= 0) _exhaustDuration = 250;
        Rotate(0.1f);
        EmitSignal(nameof(UpdateEnergyBar), _energy);
    }

    public void OnEnergyTimerTimeout(){
        if (_energy >= 100 || _exhaustDuration > 0 ) return;
        _energy++;
        EmitSignal(nameof(UpdateEnergyBar), _energy);
    }
}
