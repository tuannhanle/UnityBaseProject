using System;

namespace ZoneSystem
{
    [Flags]
    public enum ZoneType
    {
        Normal = 0,
        Left = 1,
        Right = 2,
        AlwaysVisible = 4
    }

    public enum ZoneState
    {
        Inactive,
        Loading,
        Active,
        Unloading
    }

    public enum ScenePosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Custom
    }
}
