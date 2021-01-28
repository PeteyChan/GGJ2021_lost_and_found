using Godot;
using System;
using System.Collections.Generic;
using _Core;
using Events;

public static class Debug
{
    public static bool Enable = true;
    static List<string> Labels = new List<string>();
    static List<(Vector2 origin, Vector2 end, Color color, float thickness)> Line2D = new List<(Vector2 origin, Vector2 end, Color color, float thickness)>();
    static List<(Vector3 origin, Vector3 end, Color color)> Line3D = new List<(Vector3 origin, Vector3 end, Color color)>();

    /// <summary>
    /// Logs message to output
    /// </summary>
    public static void Log(params object[] items)
    {
        string log = "";
        foreach (var item in items)
        {
            log += item == null ? "NULL" : item.ToString();
            log += " ";
        }
        GD.Print(log);
        _Core.Console.Log(log);
    }

    static System.Text.StringBuilder builder = new System.Text.StringBuilder();
    /// <summary>
    /// Draws a message to the screen
    /// Messages are cleared every frame
    /// </summary>
    public static void Label(params object[] args)
    {
        if (!Enable) return;
        builder.Clear();
        foreach (var item in args)
        {
            if (item == null)
                builder.Append("NULL");
            else
                builder.Append(item);
            builder.Append(" ");
        }
        builder.Append("\n");
        Labels.Add(builder.ToString());
    }

    public static void DrawLine3D(Vector3 global_origin, Vector3 global_end, Color color)
    {
        if (!Enable) return;
        Line3D.Add((global_origin, global_end, color));
    }

    public static void DrawLine2D(Vector2 global_origin, Vector2 global_end, Color color, float thickness = 1)
    {
        if (!Enable) return;
        Line2D.Add((global_origin, global_end, color, thickness));
    }

    public static void DrawBox2D(Vector2 origin, Vector2 scale, Color color, float thickness = 1f, bool cross = false)
    {
        var minx = origin.x - scale.x;
        var miny = origin.y - scale.y;
        var maxx = origin.x + scale.x;
        var maxy = origin.y + scale.y;

        Debug.DrawLine2D(new Vector2(minx, miny), new Vector2(minx, maxy), color, thickness);
        Debug.DrawLine2D(new Vector2(minx, maxy), new Vector2(maxx, maxy), color, thickness);
        Debug.DrawLine2D(new Vector2(maxx, maxy), new Vector2(maxx, miny), color, thickness);
        Debug.DrawLine2D(new Vector2(maxx, miny), new Vector2(minx, miny), color, thickness);
        if (cross)
        {
            Debug.DrawLine2D(new Vector2(minx, miny), new Vector2(maxx, maxy), color, thickness);
            Debug.DrawLine2D(new Vector2(minx, maxy), new Vector2(maxx, miny), color, thickness);
        }
    }

    public static void DrawCircle2D(Vector2 origin, float radius, Color color)
    {
        float offset = 360f / 16f;
        for (float angle = 0; angle < 360; angle += offset)
        {
            var deg = Mathf.Deg2Rad(angle);
            var deg2 = Mathf.Deg2Rad(angle + offset);
            var pos = origin + new Vector2(Mathf.Cos(deg), Mathf.Sin(deg)) * radius;
            var pos2 = origin + new Vector2(Mathf.Cos(deg2), Mathf.Sin(deg2)) * radius;
            Debug.DrawLine2D(pos, pos2, color);
        }
    }

    public static void DrawCircle3D(Vector3 origin, float radius, Color color)
    {
        var cam = Scene.Current.GetViewport().GetCamera();
        if (Node.IsInstanceValid(cam))
            DrawCircle3D(origin, cam.GlobalTransform.basis.z, radius, color);
    }

    readonly static float arc_angle = Mathf.Deg2Rad(360f / 16f);
    public static void DrawCircle3D(Vector3 origin, Vector3 normal, float radius, Color color)
    {
        if (normal == Vector3.Zero)
            return;
        if (normal == Vector3.Up)
            normal.x += 0.00001f;

        normal = normal.Normalized();
        Transform t = Transform.Identity;
        t = t.LookingAt(normal, Vector3.Up);
        t.origin = origin;

        var angle = 0f;
        var start = t.Xform(new Vector3(radius, 0, 0));
        for (int i = 0; i < 16; ++i)
        {
            angle += arc_angle;
            var end = t.Xform(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
            DrawLine3D(start, end, color);
            start = end;
        }
    }

    public enum TimeFrame
    {
        microseconds,
        milliseconds,
        seconds
    }

    public static void Test(string name, Action test)
        => Test(name, 1, test);

    public static void Test(string name, int iterations, Action test, TimeFrame time = TimeFrame.milliseconds)
    {
        TimeSpan start = Time.timespan_since_startup;
        for (int i = 0; i < iterations; ++i)
            test();
        TimeSpan end = Time.timespan_since_startup - start;
        string time_value;
        switch (time)
        {
            case TimeFrame.microseconds:
                time_value = $"{end.TotalMilliseconds / 1000: 0.00}us";
                break;
            case TimeFrame.milliseconds:
                time_value = $"{end.TotalMilliseconds: 0.00}ms";
                break;
            default:
                time_value = $"{end.TotalSeconds: 0.00}s";
                break;
        }
        Debug.Log(name, time_value, (iterations > 1 ? iterations.ToString() + "x" : ""));
    }

    [DispatchOrder(int.MinValue)]
    class DebugSystem :
        IDispatcher<Events.FrameUpdate>
    {
        public void OnDispatch(FrameUpdate args)
        {
            DrawDebugLabels.UpdateLabel();
            DrawDebugLine2D.UpdateDrawLine2D();
            DrawDebugLine3D.UpdateDrawLine3D();
        }

        class DrawDebugLine3D : ImmediateGeometry
        {
            static DrawDebugLine3D instance;

            public static void UpdateDrawLine3D()
            {
                if (Line3D.Count > 0)
                {
                    if (!Node.IsInstanceValid(instance))
                    {
                        Scene.Current.AddChild(instance = new DrawDebugLine3D());
                        instance.Owner = Scene.Current;
                        var mat = new SpatialMaterial();
                        instance.MaterialOverride = mat;
                        mat.VertexColorUseAsAlbedo = true;
                        mat.FlagsUnshaded = true;
                        mat.FlagsDoNotReceiveShadows = true;

                    }
                    instance.Clear();
                    instance.Begin(Mesh.PrimitiveType.Lines);
                    foreach (var line in Line3D)
                    {
                        instance.SetColor(line.color);
                        instance.AddVertex(line.origin);
                        instance.AddVertex(line.end);
                    }
                    instance.End();
                    Line3D.Clear();
                }
                else if (Node.IsInstanceValid(instance))
                    instance.QueueFree();
            }
        }

        class DrawDebugLine2D : Line2D
        {
            public override void _Draw()
            {
                foreach(var line in Line2D)
                {
                    DrawLine(line.origin, line.end, line.color, line.thickness);
                }
            }

            static DrawDebugLine2D instance;
            public static void UpdateDrawLine2D()
            {
                if (Line2D.Count > 0)
                {
                    if (!Node.IsInstanceValid(instance))
                    {
                        Scene.Current.AddChild(instance = new DrawDebugLine2D());
                        instance.Owner = Scene.Current;
                    }
                    instance.ZIndex = 4096;
                    instance.Update();
                    Line2D.Clear();
                }
                else if (Node.IsInstanceValid(instance))
                    instance.QueueFree();
            }
        }

        class DrawDebugLabels : Label
        {
            static DrawDebugLabels instance = null;
            static Godot.VScrollBar vScroll = null;

            public override void _Ready()
            {
                var flat = new StyleBoxFlat();
                flat.BgColor = new Color(0, 0, 0, .5f);
                AddStyleboxOverride("normal", flat);
                GrowHorizontal = Control.GrowDirection.Begin;
                AnchorLeft = 1;
                AnchorRight = 1f;
                MarginRight = -20;
                Align = Godot.Label.AlignEnum.Right;

                vScroll.AnchorLeft = 1;
                vScroll.AnchorRight = 1;
                vScroll.AnchorBottom = 1;
                vScroll.MarginLeft = -16;
            }

            public static void UpdateLabel()
            {
                if (Labels.Count > 0)
                {
                    if (!Node.IsInstanceValid(instance))
                    {
                        var layer = new CanvasLayer();
                        layer.AddChild(instance = new DrawDebugLabels());
                        layer.AddChild(vScroll = new VScrollBar());
                        Scene.Current.AddChild(layer);
                        layer.Owner = Scene.Current;
                        instance.Owner = layer;
                        vScroll.Owner = layer;
                    }
                    builder.Clear();
                    vScroll.MaxValue = (Labels.Count-1).min(1);
                    int scroll = (int)vScroll.Value;
                    int count = 0;
                    while (count <50)
                    {
                        var current = scroll + count;
                        if (current >= Labels.Count)
                            break;
                        builder.Append(Labels[current]);
                        count++;
                    }
                    instance.Text = builder.ToString();
                    Labels.Clear();
                }
                else if (Node.IsInstanceValid(instance))
                {
                    instance.GetParent().QueueFree();
                }
            }
        }
    }
}

namespace ConsoleCommands
{
    class ToggleDebug : ICommand
    {
        public void OnCommand(ConsoleArgs value)
        {
            Debug.Enable = !Debug.Enable;
        }
    }
}