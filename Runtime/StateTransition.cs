using UnityEngine;

namespace Koneski.StateMachine {
    public class StateTransition {
        public StateMachine StateMachine { get; private set; }
        public string TriggerName { get; private set; }
        public float TransitionDuration { get; private set; }
        public State EntryState { get; private set; }
        public State ExitState { get; private set; }

        private TransitableState _entryTransitableState;
        private TransitableState _exitTransitableState;
        private float _currentDuration;
        private bool _transitionComplete;

        public StateTransition(StateMachine stateMachine, State entryState, State exitState, string triggerName, float transitionDuration = 0) {
            this.EntryState = entryState;
            this.ExitState = exitState;
            this.TriggerName = triggerName;
            this.TransitionDuration = Mathf.Abs(transitionDuration);

            this.StateMachine = stateMachine;
            if (this.EntryState is TransitableState) {
                _entryTransitableState = (TransitableState) this.EntryState;
            }
            if (this.ExitState is TransitableState) {
                _exitTransitableState = (TransitableState) this.ExitState;
            }
        }

        public void Update(float deltaTime) {
            if (_currentDuration + deltaTime >= this.TransitionDuration) {
                _currentDuration = this.TransitionDuration;
            }

            float t = TransitionDuration > 0f ? _currentDuration / TransitionDuration : 1f;
            if (_entryTransitableState != default && this.TransitionDuration > 0) {
                _entryTransitableState.OnStateExitTransition(t);
            }
            if (_exitTransitableState != default && this.TransitionDuration > 0) {
                _exitTransitableState.OnStateEnterTransition(t);
            }

            if (_currentDuration + deltaTime >= this.TransitionDuration && !_transitionComplete) {
                this.Complete();
            }

            _currentDuration += deltaTime;
        }

        public bool IsDone() => _currentDuration >= this.TransitionDuration;

        public void Reset() {
            _currentDuration = 0;
            _transitionComplete = false;
        }

        public void Complete() {
            this.EntryState.OnStateExit();

            _currentDuration = this.TransitionDuration;
            _transitionComplete = true;

            this.StateMachine.SetState(this.ExitState);
            this.StateMachine.CurrentState.OnStateEnter();
            if (this.StateMachine.CurrentTransition == this) {
                this.StateMachine.ClearCurrentTransition();
            }

            this.Reset();

            if (this.StateMachine.DebuggingEnabled) {
                Debug.Log($"[{this.GetType().Name}] StateTransition is complete");
            }
        }
    }
}
