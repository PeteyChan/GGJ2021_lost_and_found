using Godot;

namespace Events
{
    public class Bootstrap : Node
    {
        public override void _EnterTree()
        {
            PauseMode = PauseModeEnum.Process;
            DispatchManager.Dispatch(this);
        }

        public override void _Process(float delta)
        {
            Profiler.Start("Update");
            DispatchManager.Dispatch(new Events.FrameUpdate(){deltaTime = delta});
            Profiler.Stop("Update");
        }

        public override void _PhysicsProcess(float delta)
        {
            Profiler.Start("Physics");
            DispatchManager.Dispatch(new Events.FixedUpdate(){deltaTime = delta});
            Profiler.Stop("Physics");
        }

        public override void _Input(InputEvent @event)
        {
            DispatchManager.Dispatch(@event);
        }
    }

    public struct FrameUpdate
    {
        public bool paused => Time.paused;
        public float deltaTime;
    }

    public struct FixedUpdate
    {
        public bool paused => Time.paused;
        public float deltaTime;
    }
}

