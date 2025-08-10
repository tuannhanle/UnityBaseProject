# InspectorAttributes DLL

This is the compiled DLL version of InspectorAttributes - a comprehensive Unity attribute system for enhancing the Unity Inspector.

## Files

- **Runtime/InspectorAttributes.Core.dll** - Core attributes and interfaces
- **Editor/InspectorAttributes.Editor.dll** - Editor scripts and property drawers

## Usage

Simply add `using InspectorAttributes;` to your scripts and use the attributes:

```csharp
using InspectorAttributes;

public class ExampleComponent : MonoBehaviour
{
    [BoxGroup("Settings")]
    [MinMaxSlider(0f, 1f)]
    public Vector2 range;
    
    [ShowIf("range", ConditionOperator.Greater, 0.5f)]
    [Required]
    public string requiredField;
}
```

## Complete Attribute List

### 🎨 Drawer Attributes
Custom property drawers for various Unity types:

#### Basic Drawers
- `[AllowNesting]` - Allows nesting of custom property drawers
- `[AnimatorParam]` - Dropdown for Animator parameters
- `[CurveRange]` - Curve field with min/max range
- `[Dropdown]` - Custom dropdown with predefined values
- `[EnumFlags]` - Enum flags field
- `[Expandable]` - Makes objects expandable in inspector
- `[HorizontalLine]` - Draws a horizontal line
- `[InfoBox]` - Displays an info box with message
- `[InputAxis]` - Dropdown for Input Manager axes
- `[Layer]` - Dropdown for Unity layers
- `[MinMaxSlider]` - Min-max range slider
- `[ProgressBar]` - Visual progress bar
- `[ResizableTextArea]` - Resizable text area
- `[Scene]` - Dropdown for scenes in build settings
- `[ShowAssetPreview]` - Shows asset preview
- `[SortingLayer]` - Dropdown for sorting layers
- `[Tag]` - Dropdown for Unity tags

#### Special Case Drawers
- `[Button]` - Creates a button in inspector
- `[ReorderableList]` - Makes lists reorderable
- `[ShowNativeProperty]` - Shows native properties
- `[ShowNonSerializedField]` - Shows non-serialized fields

### 🔧 Meta Attributes
Conditional display and grouping attributes:

#### Conditional Display
- `[ShowIf]` - Show field based on condition
- `[HideIf]` - Hide field based on condition
- `[EnableIf]` - Enable field based on condition
- `[DisableIf]` - Disable field based on condition

#### Grouping
- `[BoxGroup]` - Group fields in a box
- `[Foldout]` - Create foldout groups
- `[Label]` - Custom label for fields

#### Other
- `[OnValueChanged]` - Call method when value changes
- `[ReadOnly]` - Make field read-only

### ✅ Validator Attributes
Input validation attributes:

- `[Required]` - Mark field as required (cannot be null/empty)
- `[MinValue]` - Set minimum value for numeric fields
- `[MaxValue]` - Set maximum value for numeric fields
- `[ValidateInput]` - Custom validation with custom method

## Detailed Usage Examples

### Conditional Display
```csharp
public class ConditionalExample : MonoBehaviour
{
    public bool showAdvanced;
    
    [ShowIf("showAdvanced")]
    public string advancedSetting;
    
    [EnableIf("showAdvanced")]
    public float advancedValue;
    
    [HideIf("showAdvanced")]
    public string basicSetting;
}
```

### Grouping
```csharp
public class GroupingExample : MonoBehaviour
{
    [BoxGroup("Player Settings")]
    public string playerName;
    
    [BoxGroup("Player Settings")]
    [MinValue(0)]
    public int playerHealth;
    
    [Foldout("Advanced Settings")]
    public bool enableDebug;
    
    [Foldout("Advanced Settings")]
    public Color debugColor;
}
```

### Validation
```csharp
public class ValidationExample : MonoBehaviour
{
    [Required]
    public string playerName;
    
    [MinValue(0)]
    [MaxValue(100)]
    public int playerHealth;
    
    [ValidateInput("ValidateEmail")]
    public string email;
    
    private bool ValidateEmail(string email)
    {
        return email.Contains("@");
    }
}
```

### Custom Drawers
```csharp
public class DrawerExample : MonoBehaviour
{
    [Dropdown("GetOptions")]
    public string selectedOption;
    
    [MinMaxSlider(0f, 100f)]
    public Vector2 healthRange;
    
    [CurveRange(0f, 1f, 0f, 1f)]
    public AnimationCurve curve;
    
    [ProgressBar("Loading", 100f)]
    public float progress;
    
    private string[] GetOptions() => new[] { "Option 1", "Option 2", "Option 3" };
}
```

## Features

- **Drawer Attributes**: Custom property drawers for various Unity types
- **Meta Attributes**: Conditional display attributes (show/hide/enable/disable)
- **Validator Attributes**: Input validation attributes
- **Editor Integration**: Custom inspector and property drawers
- **Performance Optimized**: Compiled DLL for faster compilation
- **Easy to Use**: Simple attribute-based API

## Installation

1. Copy the `InspectorAttributes` folder to your Unity project's `Assets/Plugins` folder
2. Unity will automatically recognize the DLLs
3. Add `using InspectorAttributes;` to your scripts
4. Start using the attributes!

## Version

This DLL was compiled from the source code in InspectorAttributes v1.0
