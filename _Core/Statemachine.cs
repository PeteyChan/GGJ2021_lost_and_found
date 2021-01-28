using System.Collections.Generic;

public abstract class State
{
    public virtual void Enter (StateMachine stateMachine){}
    public virtual void Update (StateMachine stateMachine, float delta, float state_time){}
    public virtual void Exit (StateMachine stateMachine){}
}

public class StateMachine
{
    public StateMachine(params State[] states)
    {
        next = states[0];
        foreach(var state in states)
        {
            _states[TypeID.Get(state.GetType())] = state;
        }
    }

    Dictionary<int, State> _states = new Dictionary<int, State>();

    public State current {get; private set;}
    public State previous {get; private set;}
    public State next {get; private set;}

    float state_time = 0;
    public void Update(float delta)
    {
        state_time += delta;
        if (next != null)
        {
            current?.Exit(this);
            state_time = 0;
            previous = current;
            current = next;
            next = null;
            current.Enter(this);
        }
        current.Update(this, delta, state_time);
    }

    /// <summary>
    /// returns true if sucessfully changed state
    /// </summary>
    public bool Change<T>() where T: State
    {
        if (!_states.TryGetValue(TypeID.Get<T>(), out var next_state))
        {
            Debug.Log($"Statemachine failed to transition to {TypeID.Get<T>()}:{typeof(T)}");
            return false;
        }
        next = next_state;
        return true;
    }

    public override string ToString()
    {
        return $"{GetType()} : {current}";
    }
}