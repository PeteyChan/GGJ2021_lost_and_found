using ConsoleCommands;
using Events;
using Godot;
using System.Collections.Generic;

class PlayerDebug : ICommand
{
    public static bool value = false;

    public void OnCommand(ConsoleArgs args)
    {
        value = !value;
    }
}

public class player_2d_platformer : RigidBody2D
{
    class player_input
    {
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);
        
        public InputAction jump= new InputAction(KeyList.Space);
        public float move => right.value - left.value;
    }

    player_input input = new player_input();
    StateMachine player_stateMachine;
        
    public override void _EnterTree()
    {
        player_stateMachine = new StateMachine(
            new move_state(this, input),
            new jump_state(this),
            new fall_state(this),
            new jump_state(this)
        );
    }

    public override void _Process(float delta)
    {
        player_stateMachine.Update(delta);
        Debug.Label(player_stateMachine);
    }

    class move_state : State
    {
        public move_state( RigidBody2D rigidBody, player_input input)
        {
            this.rigidBody = rigidBody;
            this.input = input;
        }

        player_input input;
        RigidBody2D rigidBody;

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            if (!Physics.TryCircleCast2D(rigidBody.GlobalPosition, Vector2.Down * 16, 24f, out var result, debug: PlayerDebug.value) 
                && stateMachine.Change<fall_state>())
                return;

            if (input.jump && stateMachine.Change<jump_state>())
                return;

            rigidBody.LinearVelocity = rigidBody.LinearVelocity.lerp(new Vector2(input.move * 400, 16), delta * 12f);
        }
    }

    class jump_state : State
    {
        public jump_state(RigidBody2D rigidBody)
        {
            this.rigidbody = rigidBody;
        }
        RigidBody2D rigidbody;

        Vector2 enter_vel;
        public override void Enter(StateMachine stateMachine)
        {
            enter_vel = rigidbody.LinearVelocity;
            enter_vel.y = -1000;
        }

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            var time = state_time + .5f;
            var targetvelocity = time * time * 1400f * Vector2.Down; 
            rigidbody.LinearVelocity = targetvelocity + enter_vel;
            if (rigidbody.LinearVelocity.y > 0)
                stateMachine.Change<fall_state>();
            if (rigidbody.LinearVelocity.y == 0f || Physics.TryCircleCast2D(rigidbody.GlobalPosition, Vector2.Up * 16, 24f, out var result, debug: PlayerDebug.value))
                stateMachine.Change<fall_state>();
        }
    }

    class fall_state : State
    {
        public fall_state(RigidBody2D rigidBody)
        {
            this.rigidbody = rigidBody;
        }
        RigidBody2D rigidbody;

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            if (Physics.TryCircleCast2D(rigidbody.GlobalTransform.origin, Vector2.Down * 16f, 24f, out var result, debug: PlayerDebug.value))
            {
                stateMachine.Change<move_state>();
            }

            var vel = rigidbody.LinearVelocity;
            var time = state_time + .25f;
            vel.y = time * time * 3200f;
            rigidbody.LinearVelocity = vel;
        }
    }
}
