using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Koneski.StateMachine {
    public class StateMachine  {
        const string name = "[StateMachine]";

        public Dictionary<string, State> States { get; private set; } = new Dictionary<string, State>();
        public List<StateTransition> StateTransitions { get; private set; } = new List<StateTransition>();

        public State StartState { get; private set; }
        public State CurrentState { get; private set; }
        public State NextState { get; private set; }

        public StateTransition CurrentTransition { get; private set; }

        public bool DebuggingEnabled { get; private set; }

        public bool Trigger(string triggerName) {
            string currentStateTag = this.GetStateTag(this.CurrentState);
            
            if (this.CurrentTransition != null) {
                this.CurrentTransition.Reset();
                this.CurrentTransition = null;
            }

            if (currentStateTag != null) {
                StateTransition triggerTransition = this.StateTransitions.Find(transition => transition.EntryState.StateTag == currentStateTag && transition.TriggerName == triggerName);
                if (triggerTransition != null) {
                    this.CurrentTransition = triggerTransition;
                    return true;
                } else {
                    if (this.DebuggingEnabled) {
                        Debug.Log($"{StateMachine.name} Trigger '{triggerName}' not found for current active state '{currentStateTag}'");
                    }
                    
                    return false;
                }
            }

            return false;
        }

        public void DoUpdateStateMachine() {
            this.CurrentState?.OnStateUpdate();
            this.CurrentTransition?.Update(Time.deltaTime);
        }

        public void DoFixedUpdateStateMachine() {
            if (this.CurrentState != null && this.CurrentState is IStateFixedUpdate) {
                ((IStateFixedUpdate) this.CurrentState).OnStateFixedUpdate();
            }
        }

        public void DoLateUpdateStateMachine() {
            if (this.CurrentState != null && this.CurrentState is IStateLateUpdate) {
                ((IStateLateUpdate) this.CurrentState).OnStateLateUpdate();
            }
        }

        public void SetStartState(string tag) {
            if (!this.HasState(tag)) {
                throw new UnityException($"{StateMachine.name} State instance {name} not added. Cannot set this state as a start state, add it first with AddState<T>().");
            }

            State state = GetState(tag);
            this.StartState = state;
            this.CurrentState = state;
        }

        public void SetState(State state) => this.CurrentState = state;

        public void AddTransition(State entryState, State exitState, string triggerName, float transitionDuration = 0) {
            if (entryState == default(State)) {
                throw new UnityException($"{StateMachine.name} Error adding state transition. Entry state is null.");
            }
            if (exitState == default(State)) {
                throw new UnityException($"{StateMachine.name} Error adding exit state transition. Exit state is null.");
            }

            if (this.StateTransitions.Count(transition => transition.TriggerName == triggerName) >= 1) {
                throw new UnityException($"{StateMachine.name} Error adding state transition {triggerName}. State transition with this trigger name already exist in StateMachine.");
            }

            this.StateTransitions.Add(new StateTransition(this, entryState, exitState, triggerName, transitionDuration));
        }

        public void AddTransition(string entryStateTag, string exitStateTag, string triggerName, float transitionDuration = 0) {
            State entryState = this.GetState(entryStateTag);
            State exitState = this.GetState(exitStateTag);
            if (entryState == default(State)) {
                throw new UnityException($"{StateMachine.name} Trying to create transition '{triggerName}' but state with tag '{entryStateTag}' not found. Did you forget to add it with AddState()?");
            }
            if (exitState == default(State)) {
                throw new UnityException($"{StateMachine.name} Trying to create transition '{triggerName}' but state with tag '{exitStateTag}' not found. Did you forget to add it with AddState()?");
            }

            this.AddTransition(entryState, exitState, triggerName, transitionDuration);
        } 

        public void AddState<T>(T stateInstance) where T : State {
            string name = stateInstance.StateTag ?? stateInstance.GetType().Name;
            if (this.States.ContainsKey(name)) {
                throw new UnityException($"{StateMachine.name} State {name} already exists in the StateMachine.");
            } else {
                if (this.DebuggingEnabled) {
                    Debug.Log($"{StateMachine.name} Adding state {name} to the StateMachine.");
                }
                stateInstance.SetStateMachineContext(this);
                this.States.Add(name, stateInstance);
            }
        }

        public State GetState(string name) {
            if (this.States.ContainsKey(name)) {
                return this.States[name];
            }

            return default(State);
        }

        public bool GetState(string tag, out State stateInstance) {
            stateInstance = default(State);

            if (this.States.ContainsKey(tag)) {
                stateInstance = this.States[tag];
                return true;
            }

            return false;
        }

        public T GetState<T>() where T : State {
            string name = typeof(T).Name;
            if (this.States.ContainsKey(name)) {
                return (T) this.States[name];
            }

            throw new UnityException($"{StateMachine.name} State instance of type {name} not found in the StateMachine.");
        }

        public bool GetState<T>(out T stateInstance) where T : State {
            stateInstance = default(T);

            string name = typeof(T).Name;
            if (this.States.ContainsKey(name)) {
                stateInstance = (T) this.States[name];
                return true;
            }

            return false;
        }

        public string GetStateTag<T>() where T : State => this.States.FirstOrDefault(pair => pair.Key == typeof(T).Name).Key;

        public string GetStateTag(State stateInstance) => this.States.FirstOrDefault(pair => pair.Value == stateInstance).Key;

        public bool HasState<T>(T stateInstance) where T : State => this.HasState(stateInstance.GetType().Name); 

        public bool HasState(string key) => this.States.ContainsKey(key);
        
        public void ClearCurrentTransition() => this.CurrentTransition = null;

        public void SetDebugging(bool debuggingEnabled) => this.DebuggingEnabled = debuggingEnabled;
    }
}
