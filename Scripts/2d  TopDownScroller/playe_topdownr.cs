using Godot;
using System;

public class playe_topdownr : RigidBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    class player_input
    {
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);
        InputAction up = new InputAction(KeyList.W);
        InputAction down = new InputAction(KeyList.S);
        public InputAction dodge = new InputAction(KeyList.Space);
        public Vector2 move => new Vector2(
            right - left, down-up

        );
    }

    StateMachine stateMachine;
    player_input input = new player_input();
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        stateMachine =  new StateMachine(
            new movement(this, input),
            new dash(this));
    }

    public override void _Process(float delta)
    {
        stateMachine.Update(delta);
        Debug.Label(stateMachine);
    }

    
    class movement : State
    {
        public movement(RigidBody2D rigidBody2D, player_input input)
        {
            this.rigidBody = rigidBody2D;
            this.input = input;
        }
        
        player_input input;
        RigidBody2D rigidBody;

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            rigidBody.LinearVelocity = rigidBody.LinearVelocity.lerp(input.move * 256f, delta* 6f);

            if (input.dodge)
                stateMachine.Change<dash>();
        }
    }

    class dash : State
    {
        public dash(RigidBody2D rigidBody)
        {
            this.rigidBody = rigidBody;
        }
        RigidBody2D rigidBody;

        Vector2 enter_velocity;
        Vector2 dodge_velocity;

        public override void Enter(StateMachine stateMachine)
        {
            enter_velocity = rigidBody.LinearVelocity;
            if (enter_velocity == Vector2.Zero)
                dodge_velocity = Vector2.Up;
            else dodge_velocity = enter_velocity.Normalized();
            dodge_velocity *= 512f;
        }

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            rigidBody.LinearVelocity = (enter_velocity + dodge_velocity).lerp(Vector2.Zero, state_time);
            if (state_time > .25f || Physics.TryCircleCast2D(rigidBody.GlobalPosition, dodge_velocity.Normalized() * 32f, 16f, out var result, debug: true))
                stateMachine.Change<movement>();
        }
    }
    
}
