using Godot;
using System;

public class goto_3rd_person : Button
{
    bool was_pressed;
    public override void _Process(float delta)
    {
        if (!was_pressed && Pressed)
            Scene.Load("res://Scenes/3rd Person.tscn");
        was_pressed = Pressed;
    }
}
