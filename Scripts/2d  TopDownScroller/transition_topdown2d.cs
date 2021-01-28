using Godot;
using System;

public class transition_topdown2d : Button
{
    bool waspressed = false;

    public override void _Pressed()
    {
        if (!waspressed && Pressed)
        {
            Scene.Load("res://Scenes/2d Top Down.tscn");
        }
        waspressed = Pressed;
    }
}
