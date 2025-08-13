# State Machine System với MVP Pattern & LazyLoad

Hệ thống State Machine hiện đại cho Unity với MVP pattern, hỗ trợ LazyLoad và nested State Machines.

## 📋 Mục lục

- [Tổng quan](#tổng-quan)
- [Cài đặt](#cài-đặt)
- [Kiến trúc MVP](#kiến-trúc-mvp)
- [Sử dụng cơ bản](#sử-dụng-cơ-bản)
- [LazyLoad System](#lazyload-system)
- [Nested State Machines](#nested-state-machines)
- [Case Study 1: LazyLoad Resources](#case-study-1-lazyload-resources)
- [Case Study 2: Parent-Child State Machines](#case-study-2-parent-child-state-machines)
- [API Reference](#api-reference)
- [Best Practices](#best-practices)

## 🎯 Tổng quan

State Machine System này được thiết kế để:

- ✅ **MVP Pattern**: Tách biệt hoàn toàn Model-View-Presenter
- ✅ **LazyLoad**: Chỉ load states/resources khi cần thiết
- ✅ **Nested Support**: State Machine trong State Machine
- ✅ **No Coupling**: Hoàn toàn độc lập với các hệ thống khác
- ✅ **Resource Management**: Tự động load/unload resources
- ✅ **Production Ready**: Tối ưu cho dự án thực tế

## 🛠️ Cài đặt

### Bước 1: Setup ScriptableObject Models

Tạo các ScriptableObject instances:

```csharp
// Menu: Assets > Create > StateMachine > Models > State Machine Model
StateMachineModel mainModel = CreateInstance<StateMachineModel>();

// Menu: Assets > Create > StateMachine > Models > Resource Loader Model  
ResourceLoaderModel resourceModel = CreateInstance<ResourceLoaderModel>();
```

### Bước 2: Setup GameObject Hierarchy

```
GameManager (GameObject)
├── StateMachinePresenter (MonoBehaviour)
│   ├── StateMachineModel: mainModel
│   └── ResourceLoaderModel: resourceModel
├── LazyLoadingSystem (MonoBehaviour)
└── StateMachine (MonoBehaviour)
    ├── StateMachineModel: mainModel
    └── ResourceLoaderModel: resourceModel
```

### Bước 3: Tạo States

```csharp
public class MenuState : StateBase
{
    public override string StateID => "MenuState";
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entering Menu State");
        LoadResource("MenuUI", "UI/MenuCanvas");
    }
    
    public override void Exit()
    {
        UnloadResource("MenuUI");
        base.Exit();
    }
}
```

## 🏗️ Kiến trúc MVP

### Model (ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "StateMachine/Models/Game State Model")]
public class GameStateModel : ScriptableObject
{
    [Header("Game Data")]
    public int currentLevel = 1;
    public float playerHealth = 100f;
    
    // Events cho Presenter
    public event System.Action<int> OnLevelChanged;
    public event System.Action<float> OnHealthChanged;
    
    public void UpdateLevel(int newLevel)
    {
        currentLevel = newLevel;
        OnLevelChanged?.Invoke(currentLevel);
    }
}
```

### View (MonoBehaviour)
```csharp
public class GameplayView : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Slider healthBar;
    
    public void UpdateLevelDisplay(int level)
    {
        levelText.text = $"Level: {level}";
    }
    
    public void UpdateHealthDisplay(float health)
    {
        healthBar.value = health / 100f;
    }
}
```

### Presenter (MonoBehaviour)
```csharp
public class GameStatePresenter : StateMachinePresenter
{
    [SerializeField] private GameStateModel gameModel;
    [SerializeField] private GameplayView gameView;
    
    protected override void Start()
    {
        base.Start();
        
        // Subscribe to Model events
        gameModel.OnLevelChanged += gameView.UpdateLevelDisplay;
        gameModel.OnHealthChanged += gameView.UpdateHealthDisplay;
    }
}
```

## 🚀 Sử dụng cơ bản

### 1. Khởi tạo State Machine

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private StateMachinePresenter presenter;
    
    void Start()
    {
        // Initialize presenter
        presenter.Initialize();
        
        // Add states
        var menuState = new MenuState();
        var gameplayState = new GameplayState();
        var pauseState = new PauseState();
        
        presenter.GetStateMachine().AddState(menuState);
        presenter.GetStateMachine().AddState(gameplayState);
        presenter.GetStateMachine().AddState(pauseState);
        
        // Setup transitions
        menuState.AddTransition("GameplayState", new ButtonClickTransition());
        gameplayState.AddTransition("PauseState", new KeyPressTransition(KeyCode.Escape));
        
        // Start with menu
        presenter.StartStateMachine("MenuState");
    }
}
```

### 2. Transitions

```csharp
public class ButtonClickTransition : TransitionBase
{
    public override string TransitionID => "ButtonClick";
    
    public override bool CanTransition()
    {
        return Input.GetMouseButtonDown(0);
    }
    
    public override void OnTransition()
    {
        Debug.Log("Transitioning via button click");
    }
}
```

### 3. Resource Loading trong States

```csharp
public class GameplayState : StateBase
{
    public override string StateID => "GameplayState";
    
    public override void LoadResources()
    {
        LoadResource("PlayerModel", "Models/Player");
        LoadResource("Environment", "Scenes/Level1");
        LoadResource("GameplayMusic", "Audio/GameplayBGM");
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Sử dụng resources đã load
        var playerModel = GetResource<GameObject>("PlayerModel");
        if (playerModel != null)
        {
            Instantiate(playerModel);
        }
    }
}
```

## 🔄 LazyLoad System

### Setup LazyLoading

```csharp
public class LazyLoadManager : MonoBehaviour
{
    [SerializeField] private LazyLoadingSystem lazySystem;
    
    void Start()
    {
        // Register lazy states
        lazySystem.RegisterLazyState("HeavyGameplayState", 
            () => new HeavyGameplayState(), 
            new List<string> { "PlayerData", "LevelData" });
            
        // Register lazy resources
        lazySystem.RegisterLazyResource("PlayerModel", 
            "Models/DetailedPlayer", 
            (path) => Resources.Load<GameObject>(path));
            
        lazySystem.RegisterLazyResource("HeavyTexture", 
            "Textures/4K_Environment", 
            (path) => Resources.Load<Texture2D>(path));
    }
    
    void LoadGameplayWhenNeeded()
    {
        // Load state khi cần
        lazySystem.LoadStateAsync("HeavyGameplayState", (state) => {
            stateMachine.AddState(state);
            stateMachine.TransitionTo("HeavyGameplayState");
        });
    }
}
```

### Smart Preloading

```csharp
public class SmartLoadingState : StateBase
{
    public override void Enter()
    {
        base.Enter();
        
        // Preload adjacent states
        var lazySystem = FindObjectOfType<LazyLoadingSystem>();
        lazySystem.PreloadAdjacentStates(StateID, ParentStateMachine);
    }
}
```

## 🌳 Nested State Machines

### Parent State Machine

```csharp
public class MainGameStateMachine : StateMachine
{
    [SerializeField] private StateMachine combatSubMachine;
    [SerializeField] private StateMachine inventorySubMachine;
    
    protected override void Start()
    {
        base.Start();
        
        // Add child state machines
        AddChildStateMachine("CombatState", combatSubMachine);
        AddChildStateMachine("InventoryState", inventorySubMachine);
    }
}
```

### Child State Machine

```csharp
public class CombatStateMachine : StateMachine
{
    protected override void Start()
    {
        base.Start();
        
        // Add combat-specific states
        AddState(new AttackState());
        AddState(new DefendState());
        AddState(new DodgeState());
        
        // Setup combat transitions
        var attackState = GetState("AttackState");
        attackState.AddTransition("DefendState", new EnemyAttackTransition());
    }
}
```

---

## 📚 Case Study 1: LazyLoad Resources

### Scenario: Game với nhiều levels có assets nặng

```csharp
public class LevelManager : MonoBehaviour
{
    [SerializeField] private LazyLoadingSystem lazySystem;
    [SerializeField] private StateMachinePresenter gamePresenter;
    
    private Dictionary<int, string> levelPaths = new Dictionary<int, string>
    {
        { 1, "Levels/Forest" },
        { 2, "Levels/Desert" },
        { 3, "Levels/Ocean" },
        { 4, "Levels/Mountain" }
    };
    
    void Start()
    {
        SetupLazyLevels();
    }
    
    /// <summary>
    /// Setup lazy loading cho tất cả levels
    /// </summary>
    void SetupLazyLevels()
    {
        foreach (var level in levelPaths)
        {
            // Register lazy state cho mỗi level
            lazySystem.RegisterLazyState($"Level{level.Key}State", 
                () => CreateLevelState(level.Key), 
                new List<string> { $"Level{level.Key}Environment", $"Level{level.Key}Audio" });
            
            // Register lazy resources
            lazySystem.RegisterLazyResource($"Level{level.Key}Environment", 
                level.Value + "/Environment", 
                LoadEnvironmentAsset);
                
            lazySystem.RegisterLazyResource($"Level{level.Key}Audio", 
                level.Value + "/Audio/BGM", 
                (path) => Resources.Load<AudioClip>(path));
            
            // Register enemy data
            lazySystem.RegisterLazyResource($"Level{level.Key}Enemies", 
                level.Value + "/EnemyData", 
                (path) => Resources.Load<ScriptableObject>(path));
        }
    }
    
    /// <summary>
    /// Load level với progress tracking
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        string stateID = $"Level{levelNumber}State";
        
        // Show loading screen
        ShowLoadingScreen(true);
        
        // Unload current level để giải phóng memory
        UnloadCurrentLevel();
        
        // Load new level với callback
        lazySystem.LoadStateAsync(stateID, (state) => {
            OnLevelLoaded(state, levelNumber);
        });
        
        // Preload next level trong background
        if (levelNumber < levelPaths.Count)
        {
            string nextStateID = $"Level{levelNumber + 1}State";
            lazySystem.LoadStateAsync(nextStateID); // Preload không callback
        }
    }
    
    /// <summary>
    /// Custom loader cho environment assets
    /// </summary>
    private object LoadEnvironmentAsset(string path)
    {
        // Load main environment prefab
        var environmentPrefab = Resources.Load<GameObject>(path);
        
        // Load related textures
        var textures = Resources.LoadAll<Texture2D>(path + "/Textures");
        
        // Load materials
        var materials = Resources.LoadAll<Material>(path + "/Materials");
        
        // Return compound object
        return new LevelEnvironmentData
        {
            EnvironmentPrefab = environmentPrefab,
            Textures = textures,
            Materials = materials
        };
    }
    
    /// <summary>
    /// Callback khi level đã load xong
    /// </summary>
    private void OnLevelLoaded(IState levelState, int levelNumber)
    {
        // Add state vào main state machine
        gamePresenter.GetStateMachine().AddState(levelState);
        
        // Transition đến level mới
        gamePresenter.TransitionToState($"Level{levelNumber}State");
        
        // Hide loading screen
        ShowLoadingScreen(false);
        
        Debug.Log($"Level {levelNumber} loaded successfully!");
    }
    
    /// <summary>
    /// Unload level hiện tại để tiết kiệm memory
    /// </summary>
    private void UnloadCurrentLevel()
    {
        var currentState = gamePresenter.GetStateMachine().CurrentState;
        if (currentState != null && currentState.StateID.Contains("Level"))
        {
            // Extract level number từ state ID
            string levelNum = currentState.StateID.Replace("Level", "").Replace("State", "");
            
            // Unload resources
            lazySystem.UnloadResource($"Level{levelNum}Environment");
            lazySystem.UnloadResource($"Level{levelNum}Audio");
            lazySystem.UnloadResource($"Level{levelNum}Enemies");
            
            // Unload state
            lazySystem.UnloadState(currentState.StateID);
            
            // Force garbage collection
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
    
    /// <summary>
    /// Factory method để tạo level state
    /// </summary>
    private IState CreateLevelState(int levelNumber)
    {
        return new LevelState(levelNumber);
    }
    
    /// <summary>
    /// Show/hide loading screen
    /// </summary>
    private void ShowLoadingScreen(bool show)
    {
        // Implementation cho loading UI
        var loadingCanvas = GameObject.Find("LoadingCanvas");
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(show);
        }
    }
}

/// <summary>
/// Data structure cho environment assets
/// </summary>
[System.Serializable]
public class LevelEnvironmentData
{
    public GameObject EnvironmentPrefab;
    public Texture2D[] Textures;
    public Material[] Materials;
}

/// <summary>
/// Level state implementation
/// </summary>
public class LevelState : StateBase
{
    private int levelNumber;
    
    public override string StateID => $"Level{levelNumber}State";
    
    public LevelState(int level)
    {
        levelNumber = level;
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Lấy environment data đã được lazy load
        var envData = GetResource<LevelEnvironmentData>($"Level{levelNumber}Environment");
        if (envData?.EnvironmentPrefab != null)
        {
            Instantiate(envData.EnvironmentPrefab);
        }
        
        // Setup audio
        var bgm = GetResource<AudioClip>($"Level{levelNumber}Audio");
        if (bgm != null)
        {
            AudioSource.PlayClipAtPoint(bgm, Vector3.zero);
        }
        
        // Load enemy data
        var enemyData = GetResource<ScriptableObject>($"Level{levelNumber}Enemies");
        if (enemyData != null)
        {
            SpawnEnemies(enemyData);
        }
    }
    
    public override void Exit()
    {
        // Cleanup level-specific objects
        var levelObjects = GameObject.FindGameObjectsWithTag("LevelObject");
        foreach (var obj in levelObjects)
        {
            Destroy(obj);
        }
        
        base.Exit();
    }
    
    private void SpawnEnemies(ScriptableObject enemyData)
    {
        // Implementation để spawn enemies từ data
    }
}
```

### Kết quả của LazyLoad Case Study:

- **Memory Efficiency**: Chỉ load level hiện tại, unload level cũ
- **Fast Loading**: Preload level tiếp theo trong background
- **Smooth Transitions**: Không bị lag khi chuyển level
- **Resource Management**: Tự động cleanup unused assets

---

## 🌲 Case Study 2: Parent-Child State Machines

### Scenario: RPG Game với Combat System phức tạp

```csharp
/// <summary>
/// Main Game State Machine - Quản lý high-level game states
/// </summary>
public class RPGGameManager : MonoBehaviour
{
    [Header("State Machine Components")]
    [SerializeField] private StateMachinePresenter mainPresenter;
    [SerializeField] private LazyLoadingSystem lazySystem;
    
    [Header("Sub State Machines")]
    [SerializeField] private CombatStateMachine combatSM;
    [SerializeField] private InventoryStateMachine inventorySM;
    [SerializeField] private DialogueStateMachine dialogueSM;
    [SerializeField] private ShopStateMachine shopSM;
    
    [Header("Models")]
    [SerializeField] private RPGGameModel gameModel;
    
    void Start()
    {
        SetupMainStateMachine();
        SetupSubStateMachines();
        SetupLazyLoading();
        
        // Start với world exploration
        mainPresenter.StartStateMachine("WorldExplorationState");
    }
    
    /// <summary>
    /// Setup main state machine với high-level states
    /// </summary>
    void SetupMainStateMachine()
    {
        mainPresenter.Initialize();
        
        // Add main game states
        mainPresenter.GetStateMachine().AddState(new WorldExplorationState());
        mainPresenter.GetStateMachine().AddState(new CombatState());
        mainPresenter.GetStateMachine().AddState(new InventoryState());
        mainPresenter.GetStateMachine().AddState(new DialogueState());
        mainPresenter.GetStateMachine().AddState(new ShopState());
        mainPresenter.GetStateMachine().AddState(new GameOverState());
        
        // Setup transitions giữa main states
        SetupMainTransitions();
    }
    
    /// <summary>
    /// Setup sub state machines cho từng main state
    /// </summary>
    void SetupSubStateMachines()
    {
        var mainSM = mainPresenter.GetStateMachine();
        
        // Attach sub state machines
        mainSM.AddChildStateMachine("CombatState", combatSM);
        mainSM.AddChildStateMachine("InventoryState", inventorySM);
        mainSM.AddChildStateMachine("DialogueState", dialogueSM);
        mainSM.AddChildStateMachine("ShopState", shopSM);
        
        // Initialize sub state machines
        combatSM.Initialize();
        inventorySM.Initialize();
        dialogueSM.Initialize();
        shopSM.Initialize();
    }
    
    /// <summary>
    /// Setup lazy loading cho sub state machines
    /// </summary>
    void SetupLazyLoading()
    {
        // Register lazy sub-states
        lazySystem.RegisterLazyState("BossState", 
            () => new BossState(), 
            new List<string> { "BossModel", "BossAudio", "BossArena" });
            
        lazySystem.RegisterLazyState("CraftingState", 
            () => new CraftingState(), 
            new List<string> { "CraftingRecipes", "CraftingUI" });
        
        // Register heavy resources
        lazySystem.RegisterLazyResource("BossModel", 
            "Bosses/DragonBoss/Model", 
            (path) => Resources.Load<GameObject>(path));
            
        lazySystem.RegisterLazyResource("BossArena", 
            "Arenas/DragonArena", 
            (path) => Resources.Load<GameObject>(path));
    }
    
    /// <summary>
    /// Setup transitions cho main states
    /// </summary>
    void SetupMainTransitions()
    {
        var worldState = mainPresenter.GetStateMachine().GetState("WorldExplorationState");
        var combatState = mainPresenter.GetStateMachine().GetState("CombatState");
        var inventoryState = mainPresenter.GetStateMachine().GetState("InventoryState");
        
        // World -> Combat khi gặp enemy
        worldState.AddTransition("CombatState", new EnemyEncounterTransition());
        
        // Combat -> World khi combat kết thúc
        combatState.AddTransition("WorldExplorationState", new CombatEndTransition());
        
        // Any State -> Inventory khi nhấn I
        worldState.AddTransition("InventoryState", new KeyPressTransition(KeyCode.I));
        combatState.AddTransition("InventoryState", new KeyPressTransition(KeyCode.I));
        
        // Inventory -> Previous State khi đóng
        inventoryState.AddTransition("WorldExplorationState", new InventoryCloseTransition());
    }
}

/// <summary>
/// Combat State Machine - Quản lý chi tiết combat flow
/// </summary>
public class CombatStateMachine : StateMachine
{
    [Header("Combat Configuration")]
    [SerializeField] private CombatModel combatModel;
    [SerializeField] private CombatView combatView;
    
    public override void Initialize()
    {
        base.Initialize();
        SetupCombatStates();
        SetupCombatTransitions();
    }
    
    /// <summary>
    /// Setup các states chi tiết cho combat
    /// </summary>
    void SetupCombatStates()
    {
        // Basic combat states
        AddState(new CombatInitState());
        AddState(new PlayerTurnState());
        AddState(new EnemyTurnState());
        AddState(new SkillSelectionState());
        AddState(new TargetSelectionState());
        AddState(new AnimationPlaybackState());
        AddState(new CombatResultState());
        
        // Special combat states (lazy loaded)
        RegisterLazyState("BossCombatState");
        RegisterLazyState("PvPCombatState");
    }
    
    /// <summary>
    /// Setup transitions cho combat flow
    /// </summary>
    void SetupCombatTransitions()
    {
        var initState = GetState("CombatInitState");
        var playerTurn = GetState("PlayerTurnState");
        var enemyTurn = GetState("EnemyTurnState");
        var skillSelection = GetState("SkillSelectionState");
        var targetSelection = GetState("TargetSelectionState");
        var animation = GetState("AnimationPlaybackState");
        var result = GetState("CombatResultState");
        
        // Combat flow transitions
        initState.AddTransition("PlayerTurnState", new CombatReadyTransition());
        
        playerTurn.AddTransition("SkillSelectionState", new PlayerActionTransition());
        playerTurn.AddTransition("EnemyTurnState", new TurnSkipTransition());
        
        skillSelection.AddTransition("TargetSelectionState", new SkillSelectedTransition());
        skillSelection.AddTransition("PlayerTurnState", new CancelActionTransition());
        
        targetSelection.AddTransition("AnimationPlaybackState", new TargetSelectedTransition());
        targetSelection.AddTransition("SkillSelectionState", new CancelTargetTransition());
        
        animation.AddTransition("EnemyTurnState", new AnimationCompleteTransition());
        animation.AddTransition("CombatResultState", new BattleEndTransition());
        
        enemyTurn.AddTransition("PlayerTurnState", new EnemyTurnCompleteTransition());
        enemyTurn.AddTransition("CombatResultState", new BattleEndTransition());
    }
    
    /// <summary>
    /// Register lazy state với dependencies
    /// </summary>
    void RegisterLazyState(string stateID)
    {
        var lazySystem = FindObjectOfType<LazyLoadingSystem>();
        
        switch (stateID)
        {
            case "BossCombatState":
                lazySystem.RegisterLazyState(stateID, 
                    () => new BossCombatState(), 
                    new List<string> { "BossModel", "BossAudio", "BossArena" });
                break;
                
            case "PvPCombatState":
                lazySystem.RegisterLazyState(stateID, 
                    () => new PvPCombatState(), 
                    new List<string> { "PvPArena", "NetworkManager" });
                break;
        }
    }
    
    /// <summary>
    /// Load boss combat khi cần
    /// </summary>
    public void LoadBossCombat(string bossID)
    {
        var lazySystem = FindObjectOfType<LazyLoadingSystem>();
        
        lazySystem.LoadStateAsync("BossCombatState", (state) => {
            AddState(state);
            TransitionTo("BossCombatState");
            
            // Setup boss-specific data
            var bossState = state as BossCombatState;
            bossState?.SetBossData(bossID);
        });
    }
}

/// <summary>
/// Inventory State Machine - Quản lý inventory UI và logic
/// </summary>
public class InventoryStateMachine : StateMachine
{
    [Header("Inventory Configuration")]
    [SerializeField] private InventoryModel inventoryModel;
    [SerializeField] private InventoryView inventoryView;
    
    public override void Initialize()
    {
        base.Initialize();
        SetupInventoryStates();
    }
    
    void SetupInventoryStates()
    {
        // Inventory states
        AddState(new InventoryBrowseState());
        AddState(new ItemDetailState());
        AddState(new ItemUseState());
        AddState(new ItemSortState());
        AddState(new InventorySearchState());
        
        // Setup inventory transitions
        var browseState = GetState("InventoryBrowseState");
        var detailState = GetState("ItemDetailState");
        var useState = GetState("ItemUseState");
        
        browseState.AddTransition("ItemDetailState", new ItemSelectedTransition());
        detailState.AddTransition("ItemUseState", new UseItemTransition());
        detailState.AddTransition("InventoryBrowseState", new BackToInventoryTransition());
        useState.AddTransition("InventoryBrowseState", new ItemUsedTransition());
    }
}

/// <summary>
/// Communication giữa Parent và Child State Machines
/// </summary>
public class StateMachineCommunicator : MonoBehaviour
{
    [SerializeField] private RPGGameManager gameManager;
    
    void Start()
    {
        // Subscribe to child state machine events
        SubscribeToCombatEvents();
        SubscribeToInventoryEvents();
    }
    
    /// <summary>
    /// Listen combat state machine events
    /// </summary>
    void SubscribeToCombatEvents()
    {
        var combatSM = gameManager.GetComponent<CombatStateMachine>();
        
        // Listen khi combat kết thúc
        combatSM.OnStateChanged += (oldState, newState) => {
            if (newState.StateID == "CombatResultState")
            {
                // Notify main state machine
                var mainSM = gameManager.GetComponent<StateMachinePresenter>();
                mainSM.UpdateStateData("CombatResult", "Victory");
            }
        };
    }
    
    /// <summary>
    /// Listen inventory events
    /// </summary>
    void SubscribeToInventoryEvents()
    {
        var inventorySM = gameManager.GetComponent<InventoryStateMachine>();
        
        inventorySM.OnStateChanged += (oldState, newState) => {
            if (newState.StateID == "ItemUseState")
            {
                // Item được sử dụng - có thể affect combat hoặc world state
                HandleItemUsed();
            }
        };
    }
    
    void HandleItemUsed()
    {
        // Cross-communication logic
        var mainSM = gameManager.GetComponent<StateMachinePresenter>();
        
        if (mainSM.GetStateMachine().CurrentState.StateID == "CombatState")
        {
            // Nếu đang trong combat, apply item effect ngay lập tức
            var combatSM = gameManager.GetComponent<CombatStateMachine>();
            combatSM.TransitionTo("PlayerTurnState"); // Resume combat
        }
    }
}

/// <summary>
/// Boss Combat State - Special combat state cho boss fights
/// </summary>
public class BossCombatState : StateBase
{
    public override string StateID => "BossCombatState";
    
    private string bossID;
    private GameObject bossModel;
    private GameObject arena;
    
    public void SetBossData(string id)
    {
        bossID = id;
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Load boss-specific resources
        bossModel = GetResource<GameObject>("BossModel");
        arena = GetResource<GameObject>("BossArena");
        
        if (bossModel != null)
        {
            var boss = Instantiate(bossModel);
            SetupBossAI(boss);
        }
        
        if (arena != null)
        {
            Instantiate(arena);
        }
        
        // Start boss battle music
        var bossAudio = GetResource<AudioClip>("BossAudio");
        if (bossAudio != null)
        {
            AudioSource.PlayClipAtPoint(bossAudio, Vector3.zero);
        }
    }
    
    private void SetupBossAI(GameObject boss)
    {
        // Setup boss-specific AI behavior
        var bossAI = boss.GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.SetBossType(bossID);
            bossAI.OnBossDefeated += () => {
                // Transition back to main game
                RequestTransition("CombatResultState");
            };
        }
    }
    
    public override void Exit()
    {
        // Cleanup boss-specific objects
        var bossObjects = GameObject.FindGameObjectsWithTag("Boss");
        foreach (var obj in bossObjects)
        {
            Destroy(obj);
        }
        
        base.Exit();
    }
}
```

### Hierarchy của State Machines:

```
MainGameStateMachine (Level 0)
├── WorldExplorationState
├── CombatState (Parent)
│   └── CombatStateMachine (Level 1)
│       ├── CombatInitState
│       ├── PlayerTurnState
│       ├── EnemyTurnState
│       ├── SkillSelectionState
│       ├── TargetSelectionState
│       ├── AnimationPlaybackState
│       ├── BossCombatState (LazyLoaded)
│       └── CombatResultState
├── InventoryState (Parent)
│   └── InventoryStateMachine (Level 1)
│       ├── InventoryBrowseState
│       ├── ItemDetailState
│       ├── ItemUseState
│       └── InventorySearchState
├── DialogueState (Parent)
│   └── DialogueStateMachine (Level 1)
│       ├── NPCDialogueState
│       ├── ChoiceSelectionState
│       └── QuestDialogueState
└── ShopState (Parent)
    └── ShopStateMachine (Level 1)
        ├── BrowseItemsState
        ├── PurchaseState
        └── SellItemsState
```

### Kết quả của Nested SM Case Study:

- **Modular Design**: Mỗi domain có state machine riêng
- **Clear Separation**: Logic combat tách biệt với inventory
- **Reusable Components**: Sub state machines có thể reuse
- **Easy Debugging**: Dễ debug từng phần riêng biệt
- **Scalable**: Dễ thêm sub state machines mới

---

## 📖 API Reference

### Core Classes

#### StateMachine
```csharp
public class StateMachine : MonoBehaviour, IStateMachine
{
    // State Management
    void AddState(IState state)
    void RemoveState(string stateID)
    IState GetState(string stateID)
    bool HasState(string stateID)
    
    // Execution Control
    void Start(string initialStateID = null)
    void Stop()
    
    // Transitions
    bool CanTransitionTo(string stateID)
    void TransitionTo(string stateID)
    
    // Nested State Machines
    void AddChildStateMachine(string stateID, IStateMachine childStateMachine)
    void RemoveChildStateMachine(string stateID)
    IStateMachine GetChildStateMachine(string stateID)
    
    // Properties
    IState CurrentState { get; }
    bool IsRunning { get; }
    string StateMachineName { get; }
}
```

#### StateBase
```csharp
public abstract class StateBase : IState
{
    // Core Lifecycle
    virtual void Enter()
    virtual void Exit()
    virtual void Update()
    
    // Resource Management
    virtual void LoadResources()
    virtual void UnloadResources()
    protected void LoadResource(string resourceID, string resourcePath)
    protected void UnloadResource(string resourceID)
    protected T GetResource<T>(string resourceID) where T : class
    
    // Transitions
    virtual bool CanTransitionTo(string targetStateID)
    void AddTransition(string targetStateID, ITransition transition)
    void RemoveTransition(string targetStateID)
    protected void RequestTransition(string targetStateID)
    
    // Data Management
    protected void SetStateData(string key, object value)
    protected T GetStateData<T>(string key)
    
    // Properties
    abstract string StateID { get; }
    bool IsActive { get; }
    IStateMachine ParentStateMachine { get; set; }
}
```

#### LazyLoadingSystem
```csharp
public class LazyLoadingSystem : MonoBehaviour
{
    // Registration
    void RegisterLazyState(string stateID, Func<IState> stateFactory, List<string> dependencies = null)
    void RegisterLazyResource(string resourceID, string resourcePath, Func<string, object> resourceLoader)
    
    // Loading
    void LoadStateAsync(string stateID, Action<IState> onComplete = null)
    void LoadResourceAsync(string resourceID, Action<object> onComplete = null)
    void PreloadAdjacentStates(string currentStateID, IStateMachine stateMachine)
    
    // Unloading
    void UnloadState(string stateID)
    void UnloadResource(string resourceID)
    void UnloadAll()
    
    // Status Check
    bool IsStateLoaded(string stateID)
    bool IsResourceLoaded(string resourceID)
    IState GetLoadedState(string stateID)
    object GetLoadedResource(string resourceID)
    
    // Configuration
    void SetEnableLazyLoading(bool enable)
    void SetMaxConcurrentLoads(int maxLoads)
    void SetLoadTimeout(float timeoutSeconds)
    void SetPreloadAdjacentStates(bool enable)
    void SetAdjacentStateDepth(int depth)
}
```

### Models (ScriptableObjects)

#### StateMachineModel
```csharp
[CreateAssetMenu(menuName = "StateMachine/Models/State Machine Model")]
public class StateMachineModel : ScriptableObject
{
    // Events
    event Action<string> OnStartRequested;
    event Action OnStopRequested;
    event Action<string> OnTransitionRequested;
    event Action<string, object> OnStateDataChanged;
    
    // Methods
    void RequestStart(string initialStateID)
    void RequestStop()
    void RequestTransition(string stateID)
    void UpdateStateData(string key, object value)
    T GetStateData<T>(string key)
}
```

#### ResourceLoaderModel
```csharp
[CreateAssetMenu(menuName = "StateMachine/Models/Resource Loader Model")]
public class ResourceLoaderModel : ScriptableObject
{
    // Events
    event Action<string, string> OnLoadResourceRequested;
    event Action<string> OnUnloadResourceRequested;
    event Action<string, object> OnResourceLoaded;
    
    // Methods
    void LoadResource(string resourceID, string path)
    void UnloadResource(string resourceID)
    T GetResource<T>(string resourceID) where T : class
    bool IsResourceLoaded(string resourceID)
}
```

## 🏆 Best Practices

### 1. State Design
- **Single Responsibility**: Mỗi state chỉ handle một nhiệm vụ cụ thể
- **Clear Entry/Exit**: Luôn cleanup resources trong Exit()
- **Avoid Long Updates**: Tránh logic phức tạp trong Update()

```csharp
// ✅ Good
public class MenuState : StateBase
{
    public override void Enter()
    {
        LoadResource("MenuUI", "UI/MenuCanvas");
        ShowMenuUI();
    }
    
    public override void Exit()
    {
        HideMenuUI();
        UnloadResource("MenuUI");
    }
}

// ❌ Bad
public class MenuState : StateBase
{
    public override void Update()
    {
        // Quá nhiều logic trong Update
        HandleInput();
        UpdateAnimations();
        CheckNetworkStatus();
        ValidateUserData();
        ProcessAudio();
    }
}
```

### 2. Resource Management
- **Load on Enter**: Load resources khi vào state
- **Unload on Exit**: Cleanup khi thoát state
- **Use LazyLoad**: Cho heavy resources

```csharp
// ✅ Good - LazyLoad pattern
public override void Enter()
{
    var lazySystem = FindObjectOfType<LazyLoadingSystem>();
    lazySystem.LoadResourceAsync("HeavyModel", (resource) => {
        SetupHeavyModel(resource);
    });
}

// ❌ Bad - Sync loading
public override void Enter()
{
    var heavyModel = Resources.Load<GameObject>("VeryHeavyModel"); // Blocking
    Instantiate(heavyModel);
}
```

### 3. Nested State Machines
- **Clear Hierarchy**: Parent state chỉ handle high-level logic
- **Communication**: Sử dụng events để communicate giữa levels
- **Avoid Deep Nesting**: Tối đa 2-3 levels

```csharp
// ✅ Good - Clear separation
// Main SM: WorldState, CombatState, MenuState
// Combat SM: PlayerTurn, EnemyTurn, SkillSelection

// ❌ Bad - Too deep nesting
// Level 1: GameState
//   Level 2: CombatState
//     Level 3: PlayerTurnState
//       Level 4: SkillSelectionState
//         Level 5: TargetSelectionState (Too deep!)
```

### 4. Transitions
- **Clear Conditions**: Transition conditions phải rõ ràng
- **Validation**: Kiểm tra CanTransition trước khi transition
- **Cleanup**: Đảm bảo current state cleanup trước khi transition

```csharp
// ✅ Good
public class PlayerDeathTransition : TransitionBase
{
    public override bool CanTransition()
    {
        return PlayerHealth.Current <= 0 && !PlayerHealth.IsInvulnerable;
    }
}

// ❌ Bad
public class VagueTransition : TransitionBase
{
    public override bool CanTransition()
    {
        return someRandomCondition || maybe || sometimes; // Unclear!
    }
}
```

### 5. MVP Pattern
- **Model**: Chỉ chứa data và business logic
- **View**: Chỉ handle UI display
- **Presenter**: Orchestrate Model và View

```csharp
// ✅ Good separation
// Model: PlayerStatsModel (data + events)
// View: PlayerStatsView (UI display)
// Presenter: PlayerStatsPresenter (coordination)

// ❌ Bad mixing
public class PlayerStatsView : MonoBehaviour
{
    void Update()
    {
        // Không nên có business logic trong View
        CalculateExperience();
        SavePlayerData();
        ValidateAchievements();
    }
}
```

---

## ⚡ Performance Tips

### 1. LazyLoad Configuration
```csharp
// Optimal settings cho mobile
lazySystem.SetMaxConcurrentLoads(2);
lazySystem.SetLoadTimeout(10f);
lazySystem.SetPreloadAdjacentStates(true);
lazySystem.SetAdjacentStateDepth(1);

// Optimal settings cho PC
lazySystem.SetMaxConcurrentLoads(4);
lazySystem.SetLoadTimeout(30f);
lazySystem.SetAdjacentStateDepth(2);
```

### 2. Memory Management
```csharp
// Cleanup unused resources periodically
public class MemoryManager : MonoBehaviour
{
    void Update()
    {
        if (Time.time % 30f < Time.deltaTime) // Every 30 seconds
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}
```

### 3. State Machine Optimization
```csharp
// Cache frequently used states
private Dictionary<string, IState> stateCache = new Dictionary<string, IState>();

// Pool transitions
private Dictionary<Type, ITransition> transitionPool = new Dictionary<Type, ITransition>();
```

---

## 🐛 Troubleshooting

### Common Issues

#### 1. LazyLoad không hoạt động
```csharp
// Check: LazyLoadingSystem có được enable không?
if (!lazySystem.enabled)
{
    Debug.LogError("LazyLoadingSystem is disabled!");
}

// Check: State có được register không?
if (!lazySystem.IsStateLoaded("MyState"))
{
    Debug.LogError("State not registered or failed to load");
}
```

#### 2. Nested State Machine không transition
```csharp
// Check: Child SM có được attach đúng không?
var childSM = parentSM.GetChildStateMachine("ParentStateID");
if (childSM == null)
{
    Debug.LogError("Child SM not attached to parent state");
}
```

#### 3. Memory Leaks
```csharp
// Always unload resources
public override void Exit()
{
    UnloadAllResources();
    base.Exit();
}

// Check for null references
private void OnDestroy()
{
    if (stateMachineModel != null)
    {
        stateMachineModel.OnTransitionRequested -= HandleTransition;
    }
}
```

---

## 📝 License

State Machine System được phát triển cho Unity projects. Free to use và modify.

## 🤝 Contributing

Để contribute:
1. Fork repository
2. Tạo feature branch
3. Submit pull request với detailed description

---

**Happy State Machine Development! 🚀**
