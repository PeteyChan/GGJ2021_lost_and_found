using Godot;
using System;

public class player_3rdperson : RigidBody
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

    StateMachine player_statemachine;
    float pitch, yaw;
    player_input input;
    Camera camera;
    Spatial yaw_transform, pitch_transform;


    public override void _EnterTree()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
        input = new player_input();
        camera = FindNode("camera") as Camera;
        yaw_transform = FindNode("yaw") as Spatial;
        pitch_transform = FindNode("pitch") as Spatial;
        player_statemachine = new StateMachine( new movement(input, this, camera));
    }

    public override void _Process(float delta)
    {
        player_statemachine.Update(delta);
        Debug.Label(player_statemachine);
    
        pitch += input.look.y * 360f * delta;
        yaw += input.look.x * 360f * delta;

        pitch_transform.RotationDegrees = new Vector3(pitch, 0, 0);
        yaw_transform.RotationDegrees = new Vector3(0, yaw, 0);        
    }

    class movement: State
    {
        public movement (player_input input, RigidBody rigidBody, Camera camera)
        {
            this.rigidBody = rigidBody;
            this.input = input;
            this.camera = camera;
        }

        Camera camera;
        player_input input;
        RigidBody rigidBody;

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
            return;
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

    }

    class jump : State
    {

    }
}
