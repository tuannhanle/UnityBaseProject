using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ZoneManagerModel", menuName = "ZoneSystem/Models/Zone Manager Model")]
    public class ZoneManagerModel : ScriptableObject
    {
        [Header("Zone Management Settings")]
        [SerializeField] private bool autoDiscoverZones = true;
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool logZoneOperations = true;

        [Header("Zone Commands")]
        [SerializeField] private int targetZonePriority = 100;
        [SerializeField] private string targetZoneName = "";
        [SerializeField] private ZoneCommand currentCommand = ZoneCommand.None;

        [Header("Zone State Data")]
        [SerializeField] private List<int> activeZonePriorities = new List<int>();
        [SerializeField] private List<string> registeredZoneNames = new List<string>();
        [SerializeField] private int lastExecutedCommand = 0;

        [Header("Debug Info")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private string lastOperationMessage = "";
        [SerializeField] private float lastOperationTime = 0f;

        // Events để notify Presenter
        public event Action<int> OnActivateZoneRequest;
        public event Action<int> OnDeactivateZoneRequest;
        public event Action<string> OnActivateZoneByNameRequest;
        public event Action<string> OnDeactivateZoneByNameRequest;
        public event Action OnDeactivateAllZonesRequest;
        public event Action OnInitializeRequest;
        public event Action<string, object> OnUpdateSceneRequest;

        // Events để update UI
        public event Action<List<int>> OnActiveZonesChanged;
        public event Action<List<string>> OnRegisteredZonesChanged;
        public event Action<string> OnOperationMessageChanged;

        public enum ZoneCommand
        {
            None,
            ActivateZone,
            DeactivateZone,
            ActivateZoneByName,
            DeactivateZoneByName,
            DeactivateAllZones,
            Initialize,
            UpdateScene
        }

        // Properties
        public bool AutoDiscoverZones => autoDiscoverZones;
        public bool InitializeOnStart => initializeOnStart;
        public bool LogZoneOperations => logZoneOperations;
        public bool ShowDebugInfo => showDebugInfo;
        public int TargetZonePriority => targetZonePriority;
        public string TargetZoneName => targetZoneName;
        public ZoneCommand CurrentCommand => currentCommand;
        public List<int> ActiveZonePriorities => new List<int>(activeZonePriorities);
        public List<string> RegisteredZoneNames => new List<string>(registeredZoneNames);
        public string LastOperationMessage => lastOperationMessage;

        private void OnEnable()
        {
            // Reset command when model is loaded
            currentCommand = ZoneCommand.None;
            lastOperationTime = 0f;
        }

        // Public methods để trigger operations từ bên ngoài
        public void ActivateZone(int priority)
        {
            targetZonePriority = priority;
            currentCommand = ZoneCommand.ActivateZone;
            ExecuteCommand();
        }

        public void DeactivateZone(int priority)
        {
            targetZonePriority = priority;
            currentCommand = ZoneCommand.DeactivateZone;
            ExecuteCommand();
        }

        public void ActivateZoneByName(string zoneName)
        {
            targetZoneName = zoneName;
            currentCommand = ZoneCommand.ActivateZoneByName;
            ExecuteCommand();
        }

        public void DeactivateZoneByName(string zoneName)
        {
            targetZoneName = zoneName;
            currentCommand = ZoneCommand.DeactivateZoneByName;
            ExecuteCommand();
        }

        public void DeactivateAllZones()
        {
            currentCommand = ZoneCommand.DeactivateAllZones;
            ExecuteCommand();
        }

        public void InitializeZoneSystem()
        {
            currentCommand = ZoneCommand.Initialize;
            ExecuteCommand();
        }

        public void UpdateSceneData<T>(string sceneName, T data)
        {
            targetZoneName = sceneName; // Reuse for scene name
            currentCommand = ZoneCommand.UpdateScene;
            OnUpdateSceneRequest?.Invoke(sceneName, data);
            LogOperation($"Scene update requested: {sceneName}");
        }

        private void ExecuteCommand()
        {
            lastExecutedCommand++;
            lastOperationTime = Time.time;

            switch (currentCommand)
            {
                case ZoneCommand.ActivateZone:
                    OnActivateZoneRequest?.Invoke(targetZonePriority);
                    LogOperation($"Activate Zone Priority: {targetZonePriority}");
                    break;

                case ZoneCommand.DeactivateZone:
                    OnDeactivateZoneRequest?.Invoke(targetZonePriority);
                    LogOperation($"Deactivate Zone Priority: {targetZonePriority}");
                    break;

                case ZoneCommand.ActivateZoneByName:
                    OnActivateZoneByNameRequest?.Invoke(targetZoneName);
                    LogOperation($"Activate Zone Name: {targetZoneName}");
                    break;

                case ZoneCommand.DeactivateZoneByName:
                    OnDeactivateZoneByNameRequest?.Invoke(targetZoneName);
                    LogOperation($"Deactivate Zone Name: {targetZoneName}");
                    break;

                case ZoneCommand.DeactivateAllZones:
                    OnDeactivateAllZonesRequest?.Invoke();
                    LogOperation("Deactivate All Zones");
                    break;

                case ZoneCommand.Initialize:
                    OnInitializeRequest?.Invoke();
                    LogOperation("Initialize Zone System");
                    break;
            }

            // Reset command after execution
            currentCommand = ZoneCommand.None;
        }

        // Methods để update state từ ZoneManager
        public void UpdateActiveZones(List<int> activePriorities)
        {
            activeZonePriorities.Clear();
            activeZonePriorities.AddRange(activePriorities);
            OnActiveZonesChanged?.Invoke(new List<int>(activeZonePriorities));
        }

        public void UpdateRegisteredZones(List<string> zoneNames)
        {
            registeredZoneNames.Clear();
            registeredZoneNames.AddRange(zoneNames);
            OnRegisteredZonesChanged?.Invoke(new List<string>(registeredZoneNames));
        }

        public void LogOperation(string message)
        {
            lastOperationMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            
            if (logZoneOperations)
            {
                Debug.Log($"ZoneManagerModel: {lastOperationMessage}");
            }

            OnOperationMessageChanged?.Invoke(lastOperationMessage);
        }

        // Context menu methods for testing
        [ContextMenu("Test Activate Zone 100")]
        private void TestActivateZone100()
        {
            ActivateZone(100);
        }

        [ContextMenu("Test Activate Zone 200")]
        private void TestActivateZone200()
        {
            ActivateZone(200);
        }

        [ContextMenu("Test Activate Zone 300")]
        private void TestActivateZone300()
        {
            ActivateZone(300);
        }

        [ContextMenu("Test Deactivate All")]
        private void TestDeactivateAll()
        {
            DeactivateAllZones();
        }

        [ContextMenu("Test Initialize")]
        private void TestInitialize()
        {
            InitializeZoneSystem();
        }

        // Utility methods
        public bool IsZoneActive(int priority)
        {
            return activeZonePriorities.Contains(priority);
        }

        public bool IsZoneRegistered(string zoneName)
        {
            return registeredZoneNames.Contains(zoneName);
        }

        public int GetActiveZoneCount()
        {
            return activeZonePriorities.Count;
        }

        public int GetRegisteredZoneCount()
        {
            return registeredZoneNames.Count;
        }

        // Settings methods
        public void SetAutoDiscoverZones(bool enabled)
        {
            autoDiscoverZones = enabled;
        }

        public void SetLogZoneOperations(bool enabled)
        {
            logZoneOperations = enabled;
        }

        public void SetShowDebugInfo(bool enabled)
        {
            showDebugInfo = enabled;
        }

        // Debug info
        public string GetDebugInfo()
        {
            return $"Active Zones: {activeZonePriorities.Count}, " +
                   $"Registered Zones: {registeredZoneNames.Count}, " +
                   $"Last Command: {lastExecutedCommand}, " +
                   $"Last Operation: {lastOperationMessage}";
        }
    }
}
