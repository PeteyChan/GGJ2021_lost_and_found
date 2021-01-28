using Godot;
using System;

public class goto_firstperson_scene : Button
{
    bool was_pressed;
    public override void _Process(float delta)
    {
        Input.SetMouseMode(Input.MouseMode.Visible);
        if (!was_pressed && Pressed)
        {
            Scene.Load("res://Scenes/FirstPerson.tscn");
        }
        was_pressed = Pressed;
    }
}
