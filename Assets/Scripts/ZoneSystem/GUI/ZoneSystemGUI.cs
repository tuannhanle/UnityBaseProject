using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.Presenters;
using ZoneSystem.Scenes;
using ZoneSystem.ScriptableObjects;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZoneSystem.GUI
{
    public class ZoneSystemGUI : MonoBehaviour
    {
        [Header("GUI Settings")]
        [SerializeField] private bool showGUI = true;
        [SerializeField] private float guiWidth = 400f;
        [SerializeField] private float guiHeight = 600f;
        [SerializeField] private Vector2 guiPosition = new Vector2(10, 10);

        [Header("Zone References")]
        [SerializeField] private ZoneContent_100 zoneContent;
        [SerializeField] private ZoneInterrupt_200 zoneInterrupt;
        [SerializeField] private ZoneWarning_300 zoneWarning;
        
        [Header("Zone Manager Model")]
        [SerializeField] private ZoneManagerModel zoneManagerModel;
        [SerializeField] private ZoneManager zoneManager;

        [Header("Auto Find")]
        [SerializeField] private bool autoFindZones = true;

        private Vector2 scrollPosition;
        private bool showZoneDetails = true;
        private bool showSceneDetails = true;
        private bool showModelDetails = true;
        private bool showTestControls = true;

        private void Start()
        {
            if (autoFindZones)
            {
                FindZones();
            }
        }

        private void FindZones()
        {
            if (zoneContent == null)
                zoneContent = FindObjectOfType<ZoneContent_100>();
            
            if (zoneInterrupt == null)
                zoneInterrupt = FindObjectOfType<ZoneInterrupt_200>();
                
            if (zoneWarning == null)
                zoneWarning = FindObjectOfType<ZoneWarning_300>();

            if (zoneManagerModel == null)
                zoneManagerModel = Resources.FindObjectsOfTypeAll<ZoneManagerModel>().FirstOrDefault();
            
            if (zoneManager == null)
                zoneManager = FindObjectOfType<ZoneManager>();

            Debug.Log($"ZoneSystemGUI: Found {(zoneContent != null ? 1 : 0) + (zoneInterrupt != null ? 1 : 0) + (zoneWarning != null ? 1 : 0)} zones, Model: {zoneManagerModel != null}, Manager: {zoneManager != null}");
        }

        private void OnGUI()
        {
            if (!showGUI) return;

            Rect windowRect = new Rect(guiPosition.x, guiPosition.y, guiWidth, guiHeight);
            windowRect = UnityEngine.GUI.Window(0, windowRect, DrawZoneSystemWindow, "Zone System Controller");
            
            // Update position if window was moved
            guiPosition = new Vector2(windowRect.x, windowRect.y);
        }

        private void DrawZoneSystemWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Header controls
            DrawHeaderControls();
            
            // Scroll view for main content
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(guiHeight - 100));

            // Zone Manager status
            DrawZoneManagerStatus();

            if (showZoneDetails)
            {
                DrawZoneDetails();
            }

            if (showSceneDetails)
            {
                DrawSceneDetails();
            }

            if (showModelDetails)
            {
                DrawModelDetails();
            }

            if (showTestControls)
            {
                DrawTestControls();
            }

            GUILayout.EndScrollView();

            // Footer
            DrawFooter();

            GUILayout.EndVertical();

            // Make window draggable
            UnityEngine.GUI.DragWindow();
        }

        private void DrawHeaderControls()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Zone System Visualizer", GetLabelStyle());
            
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                FindZones();
            }
            
            showGUI = GUILayout.Toggle(showGUI, "Show");
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Section toggles
            GUILayout.BeginHorizontal();
            showZoneDetails = GUILayout.Toggle(showZoneDetails, "Zones", "Button", GUILayout.Width(60));
            showSceneDetails = GUILayout.Toggle(showSceneDetails, "Scenes", "Button", GUILayout.Width(60));
            showModelDetails = GUILayout.Toggle(showModelDetails, "Models", "Button", GUILayout.Width(60));
            showTestControls = GUILayout.Toggle(showTestControls, "Tests", "Button", GUILayout.Width(60));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private void DrawZoneManagerStatus()
        {
            GUILayout.Label("=== Zone Manager Status ===", GetLabelStyle());
            
            if (zoneManager != null)
            {
                var activeZones = zoneManager.GetActiveZones().ToList();
                var allZones = zoneManager.GetAllZones().ToList();

                GUILayout.Label($"Active Zones: {activeZones.Count} / {allZones.Count}");
                
                foreach (var zone in activeZones)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"• {zone.ZoneName} (Priority: {zone.Priority})", GUILayout.Width(200));
                    GUILayout.Label($"State: {zone.State}");
                    if (GUILayout.Button("Deactivate", GUILayout.Width(80)))
                    {
                        if (zoneManagerModel != null)
                        {
                            zoneManagerModel.DeactivateZone(zone.Priority);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Zone Manager not found!");
            }

            // Show model status
            if (zoneManagerModel != null)
            {
                GUILayout.Space(5);
                GUILayout.Label("=== Model Status ===", GetLabelStyle());
                GUILayout.Label($"Active Priorities: {string.Join(", ", zoneManagerModel.ActiveZonePriorities)}");
                GUILayout.Label($"Last Operation: {zoneManagerModel.LastOperationMessage}");
            }
            else
            {
                GUILayout.Label("Zone Manager Model not found!");
            }

            GUILayout.Space(10);
        }

        private void DrawZoneDetails()
        {
            GUILayout.Label("=== Zone Details ===", GetLabelStyle());

            // ZoneContent_100
            if (zoneContent != null)
            {
                DrawZoneContentDetails();
            }
            else
            {
                GUILayout.Label("ZoneContent_100: Not Found");
            }

            // ZoneInterrupt_200
            if (zoneInterrupt != null)
            {
                DrawZoneInterruptDetails();
            }
            else
            {
                GUILayout.Label("ZoneInterrupt_200: Not Found");
            }

            // ZoneWarning_300
            if (zoneWarning != null)
            {
                DrawZoneWarningDetails();
            }
            else
            {
                GUILayout.Label("ZoneWarning_300: Not Found");
            }

            GUILayout.Space(10);
        }

        private void DrawZoneContentDetails()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"ZoneContent_100 - State: {zoneContent.State}", GetLabelStyle());
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Activate", GUILayout.Width(80)))
            {
                zoneManagerModel?.ActivateZone(100);
            }
            if (GUILayout.Button("Deactivate", GUILayout.Width(80)))
            {
                zoneManagerModel?.DeactivateZone(100);
            }
            if (GUILayout.Button("Update All", GUILayout.Width(80)))
            {
                zoneContent.UpdateAllScenes();
            }
            GUILayout.EndHorizontal();

            // Scene count
            var allScenes = zoneContent.GetAllScenes().ToList();
            GUILayout.Label($"Scenes: {allScenes.Count}");
            
            GUILayout.EndVertical();
        }

        private void DrawZoneInterruptDetails()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"ZoneInterrupt_200 - State: {zoneInterrupt.State}", GetLabelStyle());
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Info", GUILayout.Width(60)))
            {
                zoneInterrupt.ShowInformation("Test Info", "This is a test information message");
            }
            if (GUILayout.Button("Warning", GUILayout.Width(60)))
            {
                zoneInterrupt.ShowWarning("Test Warning", "This is a test warning message");
            }
            if (GUILayout.Button("Error", GUILayout.Width(60)))
            {
                zoneInterrupt.ShowError("Test Error", "This is a test error message");
            }
            if (GUILayout.Button("Hide", GUILayout.Width(60)))
            {
                zoneManagerModel?.DeactivateZone(200);
            }
            GUILayout.EndHorizontal();
            
            if (zoneInterrupt.State == ZoneState.Active)
            {
                GUILayout.Label($"Type: {zoneInterrupt.GetCurrentInterruptType()}");
                GUILayout.Label($"Message: {zoneInterrupt.GetInterruptMessage()}");
            }
            
            GUILayout.EndVertical();
        }

        private void DrawZoneWarningDetails()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"ZoneWarning_300 - State: {zoneWarning.State}", GetLabelStyle());
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Low", GUILayout.Width(50)))
            {
                zoneWarning.ShowLowWarning("Low Warning", "Low priority warning test");
            }
            if (GUILayout.Button("Med", GUILayout.Width(50)))
            {
                zoneWarning.ShowMediumWarning("Medium Warning", "Medium priority warning test");
            }
            if (GUILayout.Button("High", GUILayout.Width(50)))
            {
                zoneWarning.ShowHighWarning("High Warning", "High priority warning test");
            }
            if (GUILayout.Button("Critical", GUILayout.Width(60)))
            {
                zoneWarning.ShowCriticalWarning("CRITICAL", "Critical emergency warning!");
            }
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                zoneWarning.ClearAllWarnings();
            }
            GUILayout.EndHorizontal();
            
            if (zoneWarning.State == ZoneState.Active)
            {
                GUILayout.Label($"Level: {zoneWarning.GetCurrentWarningLevel()}");
                GUILayout.Label($"Warnings: {zoneWarning.GetWarningCount()}");
            }
            
            GUILayout.EndVertical();
        }

        private void DrawSceneDetails()
        {
            GUILayout.Label("=== Scene Details ===", GetLabelStyle());

            if (zoneContent != null)
            {
                // SceneEco_101
                var sceneEco = zoneContent.GetEcoScene();
                if (sceneEco != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"SceneEco_101 (ID: {sceneEco.SceneID}) - Active: {sceneEco.IsActive}", GUILayout.Width(250));
                    if (GUILayout.Button("Update", GUILayout.Width(60)))
                    {
                        sceneEco.TestUpdateView();
                    }
                    GUILayout.EndHorizontal();
                }

                // SceneGauge_102
                var sceneGauge = zoneContent.GetGaugeScene();
                if (sceneGauge != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"SceneGauge_102 (ID: {sceneGauge.SceneID}) - Active: {sceneGauge.IsActive}", GUILayout.Width(250));
                    if (GUILayout.Button("Update", GUILayout.Width(60)))
                    {
                        sceneGauge.TestUpdateView();
                    }
                    GUILayout.EndHorizontal();
                }

                // SceneSDD_103
                var sceneSDD = zoneContent.GetSDDScene();
                if (sceneSDD != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"SceneSDD_103 (ID: {sceneSDD.SceneID}) - Active: {sceneSDD.IsActive}", GUILayout.Width(250));
                    if (GUILayout.Button("Update", GUILayout.Width(60)))
                    {
                        sceneSDD.TestUpdateView();
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10);
        }

        private void DrawModelDetails()
        {
            GUILayout.Label("=== Model Details ===", GetLabelStyle());

            if (zoneContent != null)
            {
                // EcoModel
                var ecoModel = zoneContent.GetEcoModel();
                if (ecoModel != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"EcoModel - Fuel: {ecoModel.fuelLevel:F1}%, Efficiency: {ecoModel.currentEfficiency:F1}, Rating: {ecoModel.ecoRating}", GUILayout.Width(300));
                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        ecoModel.TestUpdate();
                        zoneContent.UpdateEcoScene();
                    }
                    GUILayout.EndHorizontal();
                }

                // GaugeModel
                var gaugeModel = zoneContent.GetGaugeModel();
                if (gaugeModel != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"GaugeModel - Speed: {gaugeModel.currentSpeed:F1}km/h, RPM: {gaugeModel.rpmPercent:F1}%, Gear: {gaugeModel.gearPosition}", GUILayout.Width(300));
                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        gaugeModel.TestUpdate();
                        zoneContent.UpdateGaugeScene();
                    }
                    GUILayout.EndHorizontal();
                }

                // SDDModel
                var sddModel = zoneContent.GetSDDModel();
                if (sddModel != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"SDDModel - Speed: {sddModel.currentSpeed:F1}km/h, Direction: {sddModel.currentDirection}, Nav: {sddModel.isNavigationActive}", GUILayout.Width(300));
                    if (GUILayout.Button("Test", GUILayout.Width(50)))
                    {
                        sddModel.TestUpdate();
                        zoneContent.UpdateSDDScene();
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10);
        }

        private void DrawTestControls()
        {
            GUILayout.Label("=== Test Controls ===", GetLabelStyle());
            

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Activate ZoneContent (100)", GUILayout.Width(150)))
            {
                zoneManagerModel?.ActivateZone(100);
            }
            if (GUILayout.Button("Show Interrupt (200)", GUILayout.Width(120)))
            {
                zoneManagerModel?.ActivateZone(200);
                zoneInterrupt?.ShowInformation("Test", "Zone activated via GUI");
            }
            if (GUILayout.Button("Show Warning (300)", GUILayout.Width(120)))
            {
                zoneManagerModel?.ActivateZone(300);
                zoneWarning?.ShowMediumWarning("Test", "Zone activated via GUI");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Deactivate All Zones", GUILayout.Width(150)))
            {
                zoneManagerModel?.DeactivateAllZones();
            }
            if (GUILayout.Button("Update All Models", GUILayout.Width(120)))
            {
                if (zoneContent != null)
                {
                    zoneContent.GetEcoModel()?.TestUpdate();
                    zoneContent.GetGaugeModel()?.TestUpdate();
                    zoneContent.GetSDDModel()?.TestUpdate();
                    zoneContent.UpdateAllScenes();
                }
            }
            if (GUILayout.Button("Show Active Zones", GUILayout.Width(120)))
            {
                zoneManager?.GetActiveZones().ToList().ForEach(z => 
                    Debug.Log($"Active Zone: {z.ZoneName} (Priority: {z.Priority})"));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            
            // Priority test buttons
            GUILayout.Label("Priority Test:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("100", GUILayout.Width(40)))
            {
                zoneManagerModel?.ActivateZone(100);
            }
            if (GUILayout.Button("200", GUILayout.Width(40)))
            {
                zoneManagerModel?.ActivateZone(200);
            }
            if (GUILayout.Button("300", GUILayout.Width(40)))
            {
                zoneManagerModel?.ActivateZone(300);
            }
            if (GUILayout.Button("900", GUILayout.Width(40)))
            {
                zoneManagerModel?.ActivateZone(900); // High priority test
            }
            GUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"GUI Position: ({guiPosition.x:F0}, {guiPosition.y:F0})", GUILayout.Width(150));
            if (GUILayout.Button("Reset Position", GUILayout.Width(100)))
            {
                guiPosition = new Vector2(10, 10);
            }
            GUILayout.EndHorizontal();
        }

        private GUIStyle GetLabelStyle()
        {
#if UNITY_EDITOR
            return EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label;
#else
            return UnityEngine.GUI.skin.label;
#endif
        }

        // Public methods for external control
        public void SetGUIVisible(bool visible)
        {
            showGUI = visible;
        }

        public void ToggleGUI()
        {
            showGUI = !showGUI;
        }

        // Keyboard shortcuts
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleGUI();
            }
            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                zoneManagerModel?.ActivateZone(100);
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                zoneManagerModel?.ActivateZone(200);
            }
            
            if (Input.GetKeyDown(KeyCode.F4))
            {
                zoneManagerModel?.ActivateZone(300);
            }
        }
    }
}
