using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StateMachineSystem.Interfaces;

namespace StateMachineSystem.Models
{
    [CreateAssetMenu(fileName = "StateMachineModel", menuName = "StateMachine/Models/State Machine Model")]
    public class StateMachineModel : ScriptableObject, IStateMachineModel
    {
        [Header("State Machine Configuration")]
        [SerializeField] private string stateMachineName = "StateMachine";
        [SerializeField] private string currentStateID = "";
        [SerializeField] private string previousStateID = "";
        [SerializeField] private bool isRunning = false;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int maxHistoryCount = 10;

        [Header("State Data")]
        [SerializeField] private List<StateDataEntry> stateDataEntries = new List<StateDataEntry>();
        [SerializeField] private List<string> stateHistory = new List<string>();

        [Header("Commands")]
        [SerializeField] private string targetStateID = "";
        [SerializeField] private StateMachineCommand currentCommand = StateMachineCommand.None;

        private Dictionary<string, object> stateDataCache = new Dictionary<string, object>();

        public string CurrentStateID => currentStateID;
        public string PreviousStateID => previousStateID;
        public List<string> StateHistory => new List<string>(stateHistory);
        public Dictionary<string, object> StateData => new Dictionary<string, object>(stateDataCache);
        public bool IsRunning => isRunning;
        public string StateMachineName => stateMachineName;

        public event Action<string, string> OnStateChangeRequested;
        public event Action<string> OnStartRequested;
        public event Action OnStopRequested;
        public event Action<string, object> OnStateDataUpdated;
        public event Action OnModelUpdated;

        public enum StateMachineCommand
        {
            None,
            Start,
            Stop,
            TransitionTo,
            UpdateData
        }

        [Serializable]
        public class StateDataEntry
        {
            public string key;
            public string valueType;
            public string serializedValue;
        }

        private void OnEnable()
        {
            RebuildStateDataCache();
            currentCommand = StateMachineCommand.None;
        }

        private void RebuildStateDataCache()
        {
            stateDataCache.Clear();
            foreach (var entry in stateDataEntries)
            {
                try
                {
                    object value = DeserializeValue(entry.valueType, entry.serializedValue);
                    stateDataCache[entry.key] = value;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to deserialize state data '{entry.key}': {e.Message}");
                }
            }
        }

        private object DeserializeValue(string valueType, string serializedValue)
        {
            switch (valueType)
            {
                case "String": return serializedValue;
                case "Int32": return int.Parse(serializedValue);
                case "Single": return float.Parse(serializedValue);
                case "Boolean": return bool.Parse(serializedValue);
                case "Vector3": return JsonUtility.FromJson<Vector3>(serializedValue);
                default: return serializedValue;
            }
        }

        public void RequestStateChange(string fromStateID, string toStateID)
        {
            targetStateID = toStateID;
            currentCommand = StateMachineCommand.TransitionTo;
            ExecuteCommand();
        }

        public void RequestStart(string initialStateID = null)
        {
            targetStateID = initialStateID ?? "";
            currentCommand = StateMachineCommand.Start;
            ExecuteCommand();
        }

        public void RequestStop()
        {
            currentCommand = StateMachineCommand.Stop;
            ExecuteCommand();
        }

        public void UpdateStateData(string key, object value)
        {
            stateDataCache[key] = value;
            
            // Update serialized data
            var entry = stateDataEntries.FirstOrDefault(e => e.key == key);
            if (entry == null)
            {
                entry = new StateDataEntry { key = key };
                stateDataEntries.Add(entry);
            }

            entry.valueType = value?.GetType().Name ?? "String";
            entry.serializedValue = SerializeValue(value);

            OnStateDataUpdated?.Invoke(key, value);
            LogOperation($"Updated state data: {key} = {value}");
        }

        private string SerializeValue(object value)
        {
            if (value == null) return "";
            
            switch (value)
            {
                case string s: return s;
                case int i: return i.ToString();
                case float f: return f.ToString();
                case bool b: return b.ToString();
                case Vector3 v: return JsonUtility.ToJson(v);
                default: return value.ToString();
            }
        }

        public T GetStateData<T>(string key)
        {
            if (stateDataCache.ContainsKey(key))
            {
                try
                {
                    return (T)stateDataCache[key];
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to cast state data '{key}' to type {typeof(T)}: {e.Message}");
                }
            }
            return default(T);
        }

        public void SetCurrentState(string stateID)
        {
            previousStateID = currentStateID;
            currentStateID = stateID;
            AddToHistory(stateID);
            OnModelUpdated?.Invoke();
            LogOperation($"State changed: {previousStateID} → {currentStateID}");
        }

        public void AddToHistory(string stateID)
        {
            if (string.IsNullOrEmpty(stateID)) return;

            stateHistory.Add(stateID);
            
            // Maintain max history count
            while (stateHistory.Count > maxHistoryCount)
            {
                stateHistory.RemoveAt(0);
            }
        }

        public void ClearHistory()
        {
            stateHistory.Clear();
            OnModelUpdated?.Invoke();
            LogOperation("State history cleared");
        }

        private void ExecuteCommand()
        {
            switch (currentCommand)
            {
                case StateMachineCommand.Start:
                    isRunning = true;
                    OnStartRequested?.Invoke(targetStateID);
                    LogOperation($"Start requested with initial state: {targetStateID}");
                    break;

                case StateMachineCommand.Stop:
                    isRunning = false;
                    OnStopRequested?.Invoke();
                    LogOperation("Stop requested");
                    break;

                case StateMachineCommand.TransitionTo:
                    OnStateChangeRequested?.Invoke(currentStateID, targetStateID);
                    LogOperation($"Transition requested: {currentStateID} → {targetStateID}");
                    break;
            }

            currentCommand = StateMachineCommand.None;
            OnModelUpdated?.Invoke();
        }

        private void LogOperation(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[{stateMachineName}] {message}");
            }
        }

        // Context menu methods for testing
        [ContextMenu("Start State Machine")]
        private void TestStart()
        {
            RequestStart();
        }

        [ContextMenu("Stop State Machine")]
        private void TestStop()
        {
            RequestStop();
        }

        [ContextMenu("Clear History")]
        private void TestClearHistory()
        {
            ClearHistory();
        }

        [ContextMenu("Log Current State")]
        private void TestLogCurrentState()
        {
            Debug.Log($"Current State: {currentStateID}, Running: {isRunning}, History Count: {stateHistory.Count}");
        }

        // Utility methods
        public void SetRunningState(bool running)
        {
            isRunning = running;
            OnModelUpdated?.Invoke();
        }

        public void SetStateMachineName(string name)
        {
            stateMachineName = name;
        }

        public void SetEnableLogging(bool enable)
        {
            enableLogging = enable;
        }

        public void SetMaxHistoryCount(int count)
        {
            maxHistoryCount = Mathf.Max(0, count);
            
            // Trim current history if needed
            while (stateHistory.Count > maxHistoryCount)
            {
                stateHistory.RemoveAt(0);
            }
        }

        public string GetDebugInfo()
        {
            return $"StateMachine '{stateMachineName}' - Current: {currentStateID}, Running: {isRunning}, " +
                   $"History: {stateHistory.Count}/{maxHistoryCount}, Data Keys: {stateDataCache.Count}";
        }
    }
}
