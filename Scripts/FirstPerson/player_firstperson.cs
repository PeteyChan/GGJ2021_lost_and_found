using Godot;
using System;

public class player_firstperson : RigidBody
{
    class player_input
    {
        public InputAction jump = new InputAction(KeyList.Space);
        InputAction left = new InputAction(KeyList.A);
        InputAction right = new InputAction(KeyList.D);
        InputAction forward = new InputAction(KeyList.W);
        InputAction back = new InputAction(KeyList.S);

        InputAction look_left = new InputAction(MouseList.left);
        InputAction look_right = new InputAction(MouseList.right);
        InputAction look_up = new InputAction(MouseList.up);
        InputAction look_down = new InputAction(MouseList.down);

        public Vector2 move => new Vector2(right - left, back - forward);
        public Vector2 look => new Vector2(look_left - look_right, look_down - look_up);

    }

    Camera camera;
    player_input input = new player_input();
    StateMachine player_statemachine;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
        camera = FindNode("camera") as Camera;
        player_statemachine = new StateMachine(
            new movement(this, input, camera),
            new fall(this),
            new jump(this)
        ); 
    }

    float pitch, yaw;
    public override void _Process(float delta)
    {
        player_statemachine.Update(delta);
        Debug.Label(player_statemachine);
    
        pitch += input.look.y * 360f * delta;
        yaw += input.look.x * 360f * delta;

        camera.Rotation = new Vector3(Mathf.Deg2Rad(pitch), Mathf.Deg2Rad(yaw), 0);
    }

    class movement : State
    {
        public movement (RigidBody rigidBody, player_input input, Camera camera)
        {
            this.rigidBody = rigidBody;
            this.input = input;
            this.camera = camera;
        }

        RigidBody rigidBody;
        player_input input;
        Camera camera;

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            var forward = camera.GlobalTransform.basis.z;
            forward.y = 0;
            forward = forward.Normalized();

            var right = camera.GlobalTransform.basis.x;
            right.y = 0;
            right = right.Normalized();

            //var target_vel = new Vector3(input.move.x, 0, input.move.y);
            var target_vel = forward * input.move.y + right * input.move.x;
            target_vel *= 5f;
            rigidBody.LinearVelocity = rigidBody.LinearVelocity.lerp(target_vel, delta * 4f);            

            if (input.jump)
                stateMachine.Change<jump>();
            
            if (!Physics.TrySphereCast(rigidBody.GlobalTransform.origin, Vector3.Down * .5f, .2f, out var result, debug: true))
            {
                stateMachine.Change<fall>();
            }
        }
    }

    class fall : State
    {
        public fall(RigidBody rigidBody)
        {
            this.rigidBody = rigidBody;
        }
        RigidBody rigidBody;

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            if (Physics.TrySphereCast(rigidBody.GlobalTransform.origin, Vector3.Down * .5f, .2f, out var result, debug: true))
            {
                stateMachine.Change<movement>();
            }
        }
    }

    class jump : State
    {
        public jump(RigidBody rigidBody)
        {
            this.rigidBody = rigidBody;
        }
        RigidBody rigidBody;

        Vector3 entervel;
        public override void Enter(StateMachine stateMachine)
        {
            entervel  = Vector3.Up * 4f;
            entervel.x = rigidBody.LinearVelocity.x;
            entervel.z = rigidBody.LinearVelocity.z;
            rigidBody.LinearVelocity = entervel;
        }

        public override void Update(StateMachine stateMachine, float delta, float state_time)
        {
            rigidBody.LinearVelocity = entervel.lerp(Vector3.Zero, state_time);
            if (rigidBody.LinearVelocity.y >= 0)
                stateMachine.Change<fall>();
        }
    }
}
