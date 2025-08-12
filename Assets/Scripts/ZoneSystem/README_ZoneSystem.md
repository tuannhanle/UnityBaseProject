# Zone System Documentation

## Tổng quan

Hệ thống Zone System được thiết kế để quản lý các khu vực hiển thị (Zone) và các thành phần giao diện (Scene) trong Unity theo kiến trúc MVP (Model-View-Presenter).

## Các thành phần chính

### 1. Zone Types

- **Normal**: Zone bình thường
- **Left**: Zone bên trái - tắt Zone Right khi được bật
- **Right**: Zone bên phải - tắt Zone Left khi được bật  
- **AlwaysVisible**: Zone luôn hiển thị, không bị tắt

### 2. Priority System

Zones được quản lý theo priority (100, 200, 300, 400, ...):
- Zone có priority cao hơn sẽ tắt zone có priority thấp hơn
- Zone AlwaysVisible không bị ảnh hưởng bởi priority
- Left/Right zones chỉ ảnh hưởng lẫn nhau trong cùng mức priority

### 3. Scene Positions

- TopLeft, TopCenter, TopRight
- MiddleLeft, MiddleCenter, MiddleRight  
- BottomLeft, BottomCenter, BottomRight
- Custom (tự định vị trí)

## Cách sử dụng

### Bước 1: Tạo ScriptableObject Models

```csharp
// Tạo EcoModel ScriptableObject
[CreateAssetMenu(fileName = "EcoModel", menuName = "ZoneSystem/Models/Eco Model")]
public class EcoModel : ScriptableObject
{
    public float fuelLevel = 100f;
    public float currentEfficiency = 6.5f;
    // ...
}
```

### Bước 2: Tạo Scene với ID

```csharp
// Scene với ID cụ thể
public class SceneEco_101 : SceneWithID
{
    protected override void Awake()
    {
        sceneID = 101;
        sceneName = "SceneEco";
        base.Awake();
    }
}
```

### Bước 3: Tạo Zone Presenter

```csharp
// Zone như Presenter
public class ZoneContent_100 : ZoneBase
{
    [SerializeField] private EcoModel ecoModel;
    [SerializeField] private SceneEco_101 sceneEco;
    
    protected override void Awake()
    {
        priority = 100;
        zoneName = "ZoneContent";
        base.Awake();
    }
}
```

### Bước 4: Sử dụng ZoneManager

```csharp
// Kích hoạt zone
ZoneManager.Instance.ActivateZone(200);

// Cập nhật dữ liệu cho scene
var ecoData = new EcoData { fuelLevel = 85f };
ZoneManager.Instance.UpdateSceneInActiveZone("EcoScene", ecoData);

// Xem zones đang active
var activeZones = ZoneManager.Instance.GetActiveZones();
```

## Logic hoạt động

### Zone Left/Right
- Khi ZoneLeft 200 được bật → ZoneRight 300 sẽ bị tắt
- Khi ZoneRight 300 được bật → ZoneLeft 200 sẽ bị tắt

### Priority System
- Khi Zone 900 được bật → tất cả zones < 900 bị tắt (trừ AlwaysVisible)
- Zone AlwaysVisible luôn được load khi khởi động

### Resource Management
- Resources được load khi zone active
- Resources được unload khi zone deactive (trừ AlwaysVisible)
- Scene chỉ update view khi thuộc zone đang active

## Ví dụ sử dụng

### Setup trong Unity

1. Tạo GameObject với ZoneManager component
2. Tạo GameObject cho từng Zone (EcoZone, GaugeZone, NavigationZone)
3. Trong mỗi Zone, tạo child objects cho các Scene
4. Setup UI components cho từng Scene
5. Sử dụng ZoneSystemDemo để test

### Code Example

```csharp
// Trong MonoBehaviour
private void Start()
{
    // Auto-discover zones
    var zoneManager = ZoneManager.Instance;
    
    // Activate initial zones
    zoneManager.ActivateZone(100); // GaugeZone (AlwaysVisible)
    zoneManager.ActivateZone(200); // EcoZone (Right)
}

private void UpdateEcoData()
{
    var data = new EcoData { fuelLevel = 75f, efficiency = 6.2f };
    ZoneManager.Instance.UpdateSceneInActiveZone("EcoScene", data);
}
```

## Events

### Zone Events
- OnZoneActivated
- OnZoneDeactivated  
- OnResourcesLoaded
- OnResourcesUnloaded

### Scene Events
- OnSceneActivated
- OnSceneDeactivated

### ZoneManager Events
- OnZoneChanged
- OnZoneSwitched

## Best Practices

1. **Luôn sử dụng MVP pattern** cho Scene complex
2. **Set priority rõ ràng** để tránh conflict
3. **Sử dụng AlwaysVisible** cho UI quan trọng (gauges, warnings)
4. **Load resources hiệu quả** - chỉ load khi cần
5. **Test Left/Right logic** kỹ càng
6. **Sử dụng events** để decouple logic

## Debugging

- Sử dụng ZoneSystemDemo để test
- Check Console logs cho zone activation/deactivation
- Sử dụng ShowActiveZones() và ShowAllZones() để debug
- Verify resource loading/unloading trong Scene view
