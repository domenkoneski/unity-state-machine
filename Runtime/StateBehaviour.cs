using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koneski.StateMachine {
    public abstract class StateBehaviour : MonoBehaviour {

        public interface IStateBehaviourLateUpdate {
            void OnLateUpdate();
        }

        public interface IStateBehaviourFixedUpdate {
            void OnFixedUpdate();
        }

        public abstract void OnStart();
        public abstract void OnUpdate();

        public StateMachine StateMachine { get; private set; } = new StateMachine();

        private void Start() {
            this.OnStart();
        }

        private void Update() {
            this.StateMachine.DoUpdateStateMachine();
            this.OnUpdate();
        }

        private void FixedUpdate() {
            this.StateMachine.DoFixedUpdateStateMachine();
            if (this is IStateBehaviourFixedUpdate) {
                ((IStateBehaviourFixedUpdate) this).OnFixedUpdate();
            }
        }

        private void LateUpdate() {
            this.StateMachine.DoLateUpdateStateMachine();
            if (this is IStateBehaviourLateUpdate) {
                ((IStateBehaviourLateUpdate) this).OnLateUpdate();
            }
        }
    }
}

