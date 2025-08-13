using System;
using System.Collections.Generic;

namespace StateMachineSystem.Interfaces
{
    public interface IStateMachineModel
    {
        string CurrentStateID { get; }
        string PreviousStateID { get; }
        List<string> StateHistory { get; }
        Dictionary<string, object> StateData { get; }
        bool IsRunning { get; }
        
        event Action<string, string> OnStateChangeRequested; // from, to
        event Action<string> OnStartRequested;
        event Action OnStopRequested;
        event Action<string, object> OnStateDataUpdated;
        event Action OnModelUpdated;

        void RequestStateChange(string fromStateID, string toStateID);
        void RequestStart(string initialStateID = null);
        void RequestStop();
        void UpdateStateData(string key, object value);
        T GetStateData<T>(string key);
        void SetCurrentState(string stateID);
        void AddToHistory(string stateID);
        void ClearHistory();
    }

    public interface IResourceLoader
    {
        event Action<string, object> OnResourceLoaded;
        event Action<string> OnResourceUnloaded;
        event Action<string, float> OnResourceLoadProgress;

        void LoadResource(string resourceID, string resourcePath);
        void UnloadResource(string resourceID);
        T GetResource<T>(string resourceID) where T : class;
        bool IsResourceLoaded(string resourceID);
        void LoadResourcesAsync(Dictionary<string, string> resources, Action onComplete = null);
        void UnloadAllResources();
    }
}
