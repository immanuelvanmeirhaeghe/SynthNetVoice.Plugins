using UnityEngine;
using VAM_ScriptEngine;

namespace MacGruber
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }
        public State NextState { get; private set; }

        public void Switch(State state)
        {
            //SuperController.LogMessage("Switch: " + state.GetType().ToString());
            state.stateMachine = this;
            NextState = state;
        }

        public void SwitchRandom(State[] states)
        {
            if (states.Length == 0)
                return;
            int i = UnityEngine.Random.Range(0, states.Length);
            Switch(states[i]);
        }

        public void OnUpdate()
        {
            if (NextState != null)
            {
                if (CurrentState != null)
                    CurrentState.OnExit();
                CurrentState = NextState;
                NextState = null;
                if (CurrentState != null)
                {
                    CurrentState.Timestamp = Utils.GetTimestamp();
                    CurrentState.OnEnter();
                }
            }

            if (CurrentState != null)
            {
                CurrentState.OnUpdate();
                if (CurrentState.IsTimeout())
                    CurrentState.OnTimeout();
            }
        }

        public void OnInterrupt(string parameter)
        {
            if (CurrentState != null)
                CurrentState.OnInterrupt(parameter);
        }
    }

    public class State
    {
        public StateMachine stateMachine;
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnExit() { }
        public virtual void OnTimeout() { }
        public virtual void OnInterrupt(string parameter) { }

        // Time since this state got activated (in seconds)
        public float Clock()
        {
            return Utils.TimeSince(Timestamp);
        }

        // Should OnTimeout() be triggered?
        public bool IsTimeout()
        {
            return Duration > 0.0f && Utils.TimeSince(Timestamp) > Duration;
        }

        public float Duration { get; set; }
        public long Timestamp { get; set; }
    }
}
