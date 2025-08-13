using System;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.Core
{
    public abstract class TransitionBase : ITransition
    {
        protected string transitionName;
        protected string fromStateID;
        protected string toStateID;

        public string TransitionName => transitionName;
        public string FromStateID => fromStateID;
        public string ToStateID => toStateID;

        public event Action<ITransition> OnTransitionExecuted;

        protected TransitionBase(string fromStateID, string toStateID, string transitionName = null)
        {
            this.fromStateID = fromStateID;
            this.toStateID = toStateID;
            this.transitionName = transitionName ?? $"{fromStateID} -> {toStateID}";
        }

        public abstract bool CanTransition();

        public virtual void OnTransition()
        {
            OnTransitionInternal();
            OnTransitionExecuted?.Invoke(this);
        }

        protected virtual void OnTransitionInternal() { }

        public override string ToString()
        {
            return $"Transition '{transitionName}': {fromStateID} → {toStateID}";
        }
    }

    // Concrete transition implementations
    public class ImmediateTransition : TransitionBase
    {
        public ImmediateTransition(string fromStateID, string toStateID, string transitionName = null) 
            : base(fromStateID, toStateID, transitionName)
        {
        }

        public override bool CanTransition()
        {
            return true;
        }
    }

    public class ConditionalTransition : TransitionBase
    {
        private readonly Func<bool> condition;

        public ConditionalTransition(string fromStateID, string toStateID, Func<bool> condition, string transitionName = null) 
            : base(fromStateID, toStateID, transitionName)
        {
            this.condition = condition ?? (() => true);
        }

        public override bool CanTransition()
        {
            try
            {
                return condition.Invoke();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Transition condition error in '{transitionName}': {e.Message}");
                return false;
            }
        }
    }

    public class DelayedTransition : TransitionBase
    {
        private readonly float delay;
        private float startTime;
        private bool isStarted = false;

        public DelayedTransition(string fromStateID, string toStateID, float delay, string transitionName = null) 
            : base(fromStateID, toStateID, transitionName)
        {
            this.delay = delay;
        }

        public void StartTransition()
        {
            startTime = UnityEngine.Time.time;
            isStarted = true;
        }

        public void ResetTransition()
        {
            isStarted = false;
        }

        public override bool CanTransition()
        {
            if (!isStarted) return false;
            return UnityEngine.Time.time - startTime >= delay;
        }

        protected override void OnTransitionInternal()
        {
            ResetTransition();
        }
    }

    public class TimerTransition : TransitionBase
    {
        private readonly float duration;
        private float timer = 0f;
        private bool isActive = false;

        public TimerTransition(string fromStateID, string toStateID, float duration, string transitionName = null) 
            : base(fromStateID, toStateID, transitionName)
        {
            this.duration = duration;
        }

        public void StartTimer()
        {
            timer = 0f;
            isActive = true;
        }

        public void StopTimer()
        {
            isActive = false;
            timer = 0f;
        }

        public void UpdateTimer()
        {
            if (isActive)
            {
                timer += UnityEngine.Time.deltaTime;
            }
        }

        public override bool CanTransition()
        {
            return isActive && timer >= duration;
        }

        protected override void OnTransitionInternal()
        {
            StopTimer();
        }
    }
}
