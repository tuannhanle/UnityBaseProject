# Zone System Setup Guide

## 🎯 Hệ thống Zone Management với MVP Pattern hoàn chỉnh

Hệ thống này đã được thiết kế theo yêu cầu của bạn với:
- **Model từ ScriptableObject** 
- **Zone như Presenter** với priority cụ thể
- **Scene với ID số** và GameObject binding
- **Unity GUI** để test và visualize

## 📁 Cấu trúc hệ thống

```
ZoneSystem/
├── Core/                           # Lõi hệ thống
│   ├── ZoneBase.cs                # Lớp Zone cơ sở
│   ├── SceneWithID.cs             # Scene với ID số
│   └── ZoneManager.cs             # Quản lý zones
├── ScriptableObjects/             # Models (MVP)
│   ├── EcoModel.cs                # Model cho eco data
│   ├── GaugeModel.cs              # Model cho gauge data
│   └── SDDModel.cs                # Model cho SDD data
├── Scenes/                        # Scenes với ID cụ thể
│   ├── SceneEco_101.cs            # Scene ID 101
│   ├── SceneGauge_102.cs          # Scene ID 102
│   └── SceneSDD_103.cs            # Scene ID 103
├── Presenters/                    # Zone Presenters
│   ├── ZoneContent_100.cs         # Zone Content (Priority 100)
│   ├── ZoneInterrupt_200.cs       # Zone Interrupt (Priority 200)
│   └── ZoneWarning_300.cs         # Zone Warning (Priority 300)
├── GUI/                           # Unity GUI Test
│   └── ZoneSystemGUI.cs           # GUI Controller
└── README_ZoneSystem.md           # Tài liệu chi tiết
```

## 🚀 Cách Setup trong Unity

### Bước 1: Tạo ScriptableObject Models

1. **Tạo EcoModel:**
   - Right-click trong Project → Create → ZoneSystem → Models → Eco Model
   - Đặt tên: `EcoModel_Data`

2. **Tạo GaugeModel:**
   - Right-click trong Project → Create → ZoneSystem → Models → Gauge Model
   - Đặt tên: `GaugeModel_Data`

3. **Tạo SDDModel:**
   - Right-click trong Project → Create → ZoneSystem → Models → SDD Model
   - Đặt tên: `SDDModel_Data`

### Bước 2: Setup Zone Hierarchy

```
Scene
├── ZoneManager (Empty GameObject + ZoneManager.cs)
├── ZoneContent_100 (Empty GameObject + ZoneContent_100.cs)
│   ├── SceneEco_101 (GameObject + SceneEco_101.cs)
│   ├── SceneGauge_102 (GameObject + SceneGauge_102.cs)
│   └── SceneSDD_103 (GameObject + SceneSDD_103.cs)
├── ZoneInterrupt_200 (GameObject + ZoneInterrupt_200.cs)
│   └── InterruptPanel (UI Panel)
├── ZoneWarning_300 (GameObject + ZoneWarning_300.cs)
│   └── WarningPanel (UI Panel)
└── ZoneSystemGUI (GameObject + ZoneSystemGUI.cs)
```

**📝 Note:** Bây giờ ZoneManager hoạt động như **Presenter**, không phải singleton!

### Bước 3: Tạo ZoneManagerModel

4. **Tạo ZoneManagerModel:**
   - Right-click trong Project → Create → ZoneSystem → Models → Zone Manager Model
   - Đặt tên: `ZoneManagerModel_Main`

### Bước 4: Assign References

#### ZoneManager:
- **Zone Manager Model**: Kéo ZoneManagerModel_Main vào field
- **Find Model If Null**: ✓ Check

#### ZoneContent_100:
- **Models**: Kéo 4 ScriptableObject vào các field (EcoModel, GaugeModel, SDDModel, ZoneManagerModel)
- **Scene References**: Kéo 3 Scene GameObjects vào các field tương ứng
- **Auto Discover Child Scenes**: ✓ Check

#### ZoneInterrupt_200 & ZoneWarning_300:
- **Zone Manager Model**: Kéo ZoneManagerModel_Main vào field

#### Scenes (101, 102, 103):
- **Scene GameObject**: Assign GameObject chứa UI/content của scene
- **Position**: Chọn vị trí (TopRight, BottomCenter, TopCenter)

#### ZoneSystemGUI:
- **Zone References**: Kéo 3 Zone objects vào fields
- **Zone Manager Model**: Kéo ZoneManagerModel_Main vào field
- **Zone Manager**: Kéo ZoneManager GameObject vào field
- **Auto Find Zones**: ✓ Check

### Bước 5: Test System

1. **Play Scene**
2. **Nhấn F1** để show/hide GUI
3. **Sử dụng GUI** để test các zones:
   - Activate ZoneContent (100)
   - Show Interrupt (200) 
   - Show Warning (300)
   - Update models và scenes
4. **Trigger qua Model**: Tất cả operations giờ đi qua ZoneManagerModel!

## 🎮 Controls

### Keyboard Shortcuts:
- **F1**: Toggle GUI
- **F2**: Activate ZoneContent (100)
- **F3**: Activate ZoneInterrupt (200)
- **F4**: Activate ZoneWarning (300)

### GUI Features:
- **Zone Status**: Xem trạng thái tất cả zones
- **Scene Details**: Monitor từng scene với ID
- **Model Values**: Xem và test ScriptableObject data
- **Test Controls**: Buttons để test functionality

## 📊 Visualize System

### Zone Priority Logic:
- **ZoneContent_100**: Chứa SceneEco_101, SceneGauge_102, SceneSDD_103
- **ZoneInterrupt_200**: Tắt priority 100 khi active
- **ZoneWarning_300**: Tắt priority 100, 200 khi active

### Scene ID Mapping:
- **101**: SceneEco (TopRight) - Eco information
- **102**: SceneGauge (BottomCenter) - Gauge dashboard  
- **103**: SceneSDD (TopCenter) - Speed/Direction/Distance

### Model Updates:
- Models được update qua ScriptableObject
- Scenes tự động reflect changes từ models
- GUI shows real-time model values

## 🔧 Customization

### Thêm Scene mới:
1. Inherit từ `SceneWithID`
2. Set `sceneID` và `sceneName` trong Awake()
3. Implement các abstract methods
4. Add vào ZoneContent_100

### Thêm Zone mới:
1. Inherit từ `ZoneBase` 
2. Set `priority` và `zoneName`
3. Implement load/unload resources
4. Register với ZoneManager

### Tạo Model mới:
1. Inherit từ `ScriptableObject`
2. Add `[CreateAssetMenu]` attribute
3. Implement data structure và update methods

## 🐛 Debug Tips

1. **Check Console**: Tất cả actions được log
2. **GUI Status**: Real-time zone và scene status
3. **Context Menus**: Test methods trên components
4. **Scene View**: Verify GameObject setup

## ⚡ Performance Notes

- Models chỉ update khi cần thiết
- Scenes chỉ active khi Zone active
- GUI có thể toggle off để save performance
- Auto-discover chỉ chạy một lần lúc start

## 🎉 Features Implemented

✅ **ScriptableObject Models** cho MVP
✅ **Zone như Presenter** với priority system  
✅ **Scene với ID số** (101, 102, 103)
✅ **GameObject binding** trong editor
✅ **Unity GUI** để test và visualize
✅ **Priority logic** (cao tắt thấp)
✅ **Real-time updates** và monitoring

Hệ thống đã sẵn sàng để sử dụng! 🚀
