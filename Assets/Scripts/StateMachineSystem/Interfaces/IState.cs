using System;

namespace StateMachineSystem.Interfaces
{
    public interface IState
    {
        string StateName { get; }
        string StateID { get; }
        IStateMachine ParentStateMachine { get; set; }
        bool IsActive { get; }
        bool CanTransitionTo(string targetStateID);
        
        event Action<IState> OnStateEntered;
        event Action<IState> OnStateExited;
        event Action<IState> OnStateUpdated;
        event Action<IState, string> OnResourceLoaded;
        event Action<IState, string> OnResourceUnloaded;

        void Enter();
        void Exit();
        void Update();
        void LoadResources();
        void UnloadResources();
        void AddTransition(string targetStateID, ITransition transition);
        void RemoveTransition(string targetStateID);
        bool HasTransition(string targetStateID);
    }

    public interface ITransition
    {
        string TransitionName { get; }
        string FromStateID { get; }
        string ToStateID { get; }
        bool CanTransition();
        void OnTransition();
        
        event Action<ITransition> OnTransitionExecuted;
    }

    public interface IStateMachine
    {
        string StateMachineName { get; }
        string StateMachineID { get; }
        IState CurrentState { get; }
        IStateMachine ParentStateMachine { get; set; }
        bool IsRunning { get; }
        
        event Action<IStateMachine, IState, IState> OnStateChanged;
        event Action<IStateMachine> OnStateMachineStarted;
        event Action<IStateMachine> OnStateMachineStopped;

        void AddState(IState state);
        void RemoveState(string stateID);
        IState GetState(string stateID);
        bool HasState(string stateID);
        
        void Start(string initialStateID = null);
        void Stop();
        void Update();
        void TransitionTo(string stateID);
        bool CanTransitionTo(string stateID);
        
        void AddChildStateMachine(string stateID, IStateMachine childStateMachine);
        void RemoveChildStateMachine(string stateID);
        IStateMachine GetChildStateMachine(string stateID);
    }
}
