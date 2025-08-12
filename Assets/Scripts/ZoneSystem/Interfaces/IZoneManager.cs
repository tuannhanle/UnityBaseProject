using System;
using System.Collections.Generic;

namespace ZoneSystem.Interfaces
{
    public interface IZoneManager
    {
        event Action<IZone> OnZoneChanged;
        event Action<IZone, IZone> OnZoneSwitched; // oldZone, newZone

        void RegisterZone(IZone zone);
        void UnregisterZone(IZone zone);
        void ActivateZone(int priority);
        void DeactivateZone(int priority);
        void ActivateZoneByName(string zoneName);
        void DeactivateZoneByName(string zoneName);
        
        IZone GetZone(int priority);
        IZone GetZoneByName(string zoneName);
        IEnumerable<IZone> GetActiveZones();
        IEnumerable<IZone> GetAllZones();
        
        void UpdateSceneInActiveZone<T>(string sceneName, T data);
        void InitializeAlwaysVisibleZones();
    }
}
