using System;
using UnityEngine;

namespace ZoneSystem.Interfaces
{
    public interface IScene
    {
        string SceneName { get; }
        ScenePosition Position { get; }
        IZone ParentZone { get; set; }
        bool IsActive { get; }
        GameObject SceneRoot { get; }

        event Action<IScene> OnSceneActivated;
        event Action<IScene> OnSceneDeactivated;

        void Initialize();
        void Activate();
        void Deactivate();
        void UpdateView<T>(T data);
        void SetPosition(Vector2 position, Vector2 size);
    }
}
