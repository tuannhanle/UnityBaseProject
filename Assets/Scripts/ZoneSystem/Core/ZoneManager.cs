using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZoneSystem.Interfaces;
using ZoneSystem.ScriptableObjects;

namespace ZoneSystem.Core
{
    public class ZoneManager : MonoBehaviour, IZoneManager
    {
        [Header("Zone Manager Presenter")]
        [SerializeField] private ZoneManagerModel zoneManagerModel;
        [SerializeField] private bool findModelIfNull = true;

        private Dictionary<int, IZone> zonesByPriority = new Dictionary<int, IZone>();
        private Dictionary<string, IZone> zonesByName = new Dictionary<string, IZone>();
        private HashSet<IZone> activeZones = new HashSet<IZone>();

        public event Action<IZone> OnZoneChanged;
        public event Action<IZone, IZone> OnZoneSwitched;

        private void Awake()
        {
            // Find model if not assigned
            if (zoneManagerModel == null && findModelIfNull)
            {
                zoneManagerModel = Resources.FindObjectsOfTypeAll<ZoneManagerModel>().FirstOrDefault();
                if (zoneManagerModel == null)
                {
                    Debug.LogWarning("ZoneManager: ZoneManagerModel not found! Please assign one.");
                }
            }

            // Subscribe to model events
            SubscribeToModelEvents();
        }

        private void Start()
        {
            if (zoneManagerModel != null && zoneManagerModel.InitializeOnStart)
            {
                InitializeManager();
            }
        }

        private void SubscribeToModelEvents()
        {
            if (zoneManagerModel == null) return;

            zoneManagerModel.OnActivateZoneRequest += ActivateZone;
            zoneManagerModel.OnDeactivateZoneRequest += DeactivateZone;
            zoneManagerModel.OnActivateZoneByNameRequest += ActivateZoneByName;
            zoneManagerModel.OnDeactivateZoneByNameRequest += DeactivateZoneByName;
            zoneManagerModel.OnDeactivateAllZonesRequest += () => DeactivateAllZones(false);
            zoneManagerModel.OnInitializeRequest += InitializeManager;
            zoneManagerModel.OnUpdateSceneRequest += UpdateSceneInActiveZone;
        }

        private void UnsubscribeFromModelEvents()
        {
            if (zoneManagerModel == null) return;

            zoneManagerModel.OnActivateZoneRequest -= ActivateZone;
            zoneManagerModel.OnDeactivateZoneRequest -= DeactivateZone;
            zoneManagerModel.OnActivateZoneByNameRequest -= ActivateZoneByName;
            zoneManagerModel.OnDeactivateZoneByNameRequest -= DeactivateZoneByName;
            zoneManagerModel.OnDeactivateAllZonesRequest -= () => DeactivateAllZones(false);
            zoneManagerModel.OnInitializeRequest -= InitializeManager;
            zoneManagerModel.OnUpdateSceneRequest -= UpdateSceneInActiveZone;
        }

        private void InitializeManager()
        {
            if (zoneManagerModel != null && zoneManagerModel.AutoDiscoverZones)
            {
                AutoDiscoverZones();
            }
            
            InitializeAlwaysVisibleZones();
            UpdateModelState();
        }

        private void AutoDiscoverZones()
        {
            var allZones = FindObjectsOfType<MonoBehaviour>().OfType<IZone>();
            foreach (var zone in allZones)
            {
                RegisterZone(zone);
            }
        }

        public void RegisterZone(IZone zone)
        {
            if (zone == null) return;

            // Register by priority
            if (zonesByPriority.ContainsKey(zone.Priority))
            {
                Debug.LogWarning($"Zone với priority {zone.Priority} đã tồn tại. Sẽ thay thế zone cũ.");
            }
            zonesByPriority[zone.Priority] = zone;

            // Register by name
            if (zonesByName.ContainsKey(zone.ZoneName))
            {
                Debug.LogWarning($"Zone với tên {zone.ZoneName} đã tồn tại. Sẽ thay thế zone cũ.");
            }
            zonesByName[zone.ZoneName] = zone;

            // Subscribe to events
            zone.OnZoneActivated += OnZoneActivatedHandler;
            zone.OnZoneDeactivated += OnZoneDeactivatedHandler;

            Debug.Log($"Đã đăng ký Zone: {zone.ZoneName} (Priority: {zone.Priority})");
        }

        public void UnregisterZone(IZone zone)
        {
            if (zone == null) return;

            // Unsubscribe from events
            zone.OnZoneActivated -= OnZoneActivatedHandler;
            zone.OnZoneDeactivated -= OnZoneDeactivatedHandler;

            // Remove from collections
            zonesByPriority.Remove(zone.Priority);
            zonesByName.Remove(zone.ZoneName);
            activeZones.Remove(zone);

            Debug.Log($"Đã hủy đăng ký Zone: {zone.ZoneName}");
        }

        public void ActivateZone(int priority)
        {
            var zone = GetZone(priority);
            if (zone == null)
            {
                Debug.LogWarning($"Không tìm thấy Zone với priority {priority}");
                return;
            }

            ActivateZoneInternal(zone);
        }

        public void ActivateZoneByName(string zoneName)
        {
            var zone = GetZoneByName(zoneName);
            if (zone == null)
            {
                Debug.LogWarning($"Không tìm thấy Zone với tên {zoneName}");
                return;
            }

            ActivateZoneInternal(zone);
        }

        private void ActivateZoneInternal(IZone zone)
        {
            if (zone == null) return;

            // Handle Left/Right zones logic
            if (zone.Type.HasFlag(ZoneType.Left) || zone.Type.HasFlag(ZoneType.Right))
            {
                HandleLeftRightZones(zone);
            }

            // Handle priority-based deactivation
            if (!zone.IsAlwaysVisible)
            {
                DeactivateLowerPriorityZones(zone.Priority);
            }

            // Activate the zone
            if (!activeZones.Contains(zone))
            {
                zone.Activate();
                activeZones.Add(zone);
                OnZoneChanged?.Invoke(zone);
            }
        }

        private void HandleLeftRightZones(IZone targetZone)
        {
            var oppositeType = targetZone.Type.HasFlag(ZoneType.Left) ? ZoneType.Right : ZoneType.Left;
            
            // Deactivate zones of opposite type with same or lower priority
            var zonesToDeactivate = activeZones
                .Where(z => z.Type.HasFlag(oppositeType) && 
                           z.Priority <= targetZone.Priority && 
                           !z.IsAlwaysVisible)
                .ToList();

            foreach (var zone in zonesToDeactivate)
            {
                DeactivateZoneInternal(zone);
            }
        }

        private void DeactivateLowerPriorityZones(int priority)
        {
            var zonesToDeactivate = activeZones
                .Where(z => z.Priority < priority && !z.IsAlwaysVisible)
                .ToList();

            foreach (var zone in zonesToDeactivate)
            {
                DeactivateZoneInternal(zone);
            }
        }

        public void DeactivateZone(int priority)
        {
            var zone = GetZone(priority);
            if (zone != null)
            {
                DeactivateZoneInternal(zone);
            }
        }

        public void DeactivateZoneByName(string zoneName)
        {
            var zone = GetZoneByName(zoneName);
            if (zone != null)
            {
                DeactivateZoneInternal(zone);
            }
        }

        private void DeactivateZoneInternal(IZone zone)
        {
            if (zone == null || !activeZones.Contains(zone)) return;

            if (zone.IsAlwaysVisible)
            {
                Debug.LogWarning($"Không thể tắt Zone AlwaysVisible: {zone.ZoneName}");
                return;
            }

            zone.Deactivate();
            activeZones.Remove(zone);
            OnZoneChanged?.Invoke(zone);
        }

        public IZone GetZone(int priority)
        {
            return zonesByPriority.ContainsKey(priority) ? zonesByPriority[priority] : null;
        }

        public IZone GetZoneByName(string zoneName)
        {
            return zonesByName.ContainsKey(zoneName) ? zonesByName[zoneName] : null;
        }

        public IEnumerable<IZone> GetActiveZones()
        {
            return activeZones.ToList();
        }

        public IEnumerable<IZone> GetAllZones()
        {
            return zonesByPriority.Values.ToList();
        }

        public void UpdateSceneInActiveZone<T>(string sceneName, T data)
        {
            foreach (var zone in activeZones)
            {
                var allScenes = zone.GetAllScenes();
                var targetScene = allScenes.FirstOrDefault(s => s.SceneName == sceneName);
                
                if (targetScene != null)
                {
                    targetScene.UpdateView(data);
                    zoneManagerModel?.LogOperation($"Updated Scene {sceneName} in Zone {zone.ZoneName}");
                    return;
                }
            }
            
            zoneManagerModel?.LogOperation($"Scene {sceneName} not found in active zones");
        }

        // Overload để handle generic object từ model
        private void UpdateSceneInActiveZone(string sceneName, object data)
        {
            foreach (var zone in activeZones)
            {
                var allScenes = zone.GetAllScenes();
                var targetScene = allScenes.FirstOrDefault(s => s.SceneName == sceneName);
                
                if (targetScene != null)
                {
                    targetScene.UpdateView(data);
                    zoneManagerModel?.LogOperation($"Updated Scene {sceneName} in Zone {zone.ZoneName}");
                    return;
                }
            }
            
            zoneManagerModel?.LogOperation($"Scene {sceneName} not found in active zones");
        }

        public void InitializeAlwaysVisibleZones()
        {
            var alwaysVisibleZones = zonesByPriority.Values
                .Where(z => z.IsAlwaysVisible)
                .OrderBy(z => z.Priority);

            foreach (var zone in alwaysVisibleZones)
            {
                if (!activeZones.Contains(zone))
                {
                    zone.Activate();
                    activeZones.Add(zone);
                    Debug.Log($"Đã khởi tạo AlwaysVisible Zone: {zone.ZoneName}");
                }
            }
        }

        private void UpdateModelState()
        {
            if (zoneManagerModel == null) return;

            var activePriorities = activeZones.Select(z => z.Priority).ToList();
            var registeredNames = zonesByName.Keys.ToList();

            zoneManagerModel.UpdateActiveZones(activePriorities);
            zoneManagerModel.UpdateRegisteredZones(registeredNames);
        }

        private void OnZoneActivatedHandler(IZone zone)
        {
            zoneManagerModel?.LogOperation($"Zone Activated: {zone.ZoneName} (Priority: {zone.Priority})");
            UpdateModelState();
        }

        private void OnZoneDeactivatedHandler(IZone zone)
        {
            zoneManagerModel?.LogOperation($"Zone Deactivated: {zone.ZoneName} (Priority: {zone.Priority})");
            UpdateModelState();
        }

        // Utility methods
        public void DeactivateAllZones(bool includeAlwaysVisible = false)
        {
            var zonesToDeactivate = includeAlwaysVisible ? 
                activeZones.ToList() : 
                activeZones.Where(z => !z.IsAlwaysVisible).ToList();

            foreach (var zone in zonesToDeactivate)
            {
                DeactivateZoneInternal(zone);
            }
        }

        public void ActivateZoneExclusive(int priority)
        {
            DeactivateAllZones(false);
            ActivateZone(priority);
        }

        private void OnDestroy()
        {
            UnsubscribeFromModelEvents();
        }

        // Public methods để access từ bên ngoài (nếu cần)
        public ZoneManagerModel GetModel()
        {
            return zoneManagerModel;
        }

        public void SetModel(ZoneManagerModel model)
        {
            UnsubscribeFromModelEvents();
            zoneManagerModel = model;
            SubscribeToModelEvents();
            UpdateModelState();
        }
    }
}

