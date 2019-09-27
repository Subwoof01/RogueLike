using Godot;
using System;

public class Player : Sprite
{
    public Vector2 MapPosition { get; set; }

    private Node _game;

    private void TryMove(int x, int y) 
    {
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _game = GetNode("/root/Game");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
