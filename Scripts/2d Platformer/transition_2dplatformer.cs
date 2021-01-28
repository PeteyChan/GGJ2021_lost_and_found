using Godot;
using System;

public class transition_2dplatformer : Button
{
    bool was_pressed;
    public override void _Process(float delta)
    {
        if (!was_pressed && Pressed)
        {
            Scene.Load("res://Scenes/2d Platformer.tscn");
        }
        was_pressed = Pressed;
    }
}
