namespace Koneski.StateMachine {
    public abstract class State {
        public abstract void OnStateEnter();
        public abstract void OnStateUpdate();
        public abstract void OnStateExit();

        public string StateTag { get; private set; }
        public StateMachine StateMachine { get; private set; }
        
        public State() : base() => this.StateTag = GetType().Name;
        public State(string stateTag) => this.StateTag = stateTag;

        public void SetStateMachineContext(StateMachine stateMachine) => this.StateMachine = stateMachine;
    }

    public abstract class TransitableState : State {
        public abstract void OnStateEnterTransition(float t);
        public abstract void OnStateExitTransition(float t);

        public TransitableState() : base() { }
        public TransitableState(string stateTag) : base(stateTag) { }
    }

    public interface IStateLateUpdate {
        void OnStateLateUpdate();
    }

    public interface IStateFixedUpdate {
        void OnStateFixedUpdate();
    }
}
