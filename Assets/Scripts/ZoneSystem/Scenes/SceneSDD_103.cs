using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.ScriptableObjects;

namespace ZoneSystem.Scenes
{
    public class SceneSDD_103 : SceneWithID
    {
        [Header("SDD Scene Settings")]
        [SerializeField] private SDDModel sddModel;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool autoUpdateNavigation = false;
        [SerializeField] private float navigationUpdateInterval = 5f;

        private float lastNavigationUpdate;

        protected override void Awake()
        {
            sceneID = 103;
            sceneName = "SceneSDD";
            position = ScenePosition.TopCenter;
            base.Awake();
        }

        protected virtual void Update()
        {
            if (autoUpdateNavigation && IsActive && Time.time - lastNavigationUpdate >= navigationUpdateInterval)
            {
                if (sddModel != null && sddModel.isNavigationActive)
                {
                    // Simulate navigation updates
                    SimulateNavigationProgress();
                }
                lastNavigationUpdate = Time.time;
            }
        }

        protected override void OnInitializeInternal()
        {
            if (sddModel == null)
            {
                Debug.LogWarning($"SceneSDD_103: SDDModel chưa được assign!");
            }
            
            Debug.Log($"SceneSDD_103 initialized with ID: {sceneID}");
        }

        protected override void OnActivateInternal()
        {
            if (showDebugInfo)
                Debug.Log($"SceneSDD_103 activated - showing speed/direction/distance information");
                
            lastNavigationUpdate = Time.time;
        }

        protected override void OnDeactivateInternal()
        {
            if (showDebugInfo)
                Debug.Log($"SceneSDD_103 deactivated");
        }

        protected override void OnUpdateViewInternal<T>(T data)
        {
            // Handle different data types
            if (data is SDDModel sddData)
            {
                UpdateSDDView(sddData);
            }
            else if (sddModel != null)
            {
                // Use assigned model if no specific data provided
                UpdateSDDView(sddModel);
            }
            else
            {
                Debug.LogWarning($"SceneSDD_103: Không thể update view - không có data hoặc model");
            }
        }

        private void UpdateSDDView(SDDModel model)
        {
            if (model == null) return;

            // Update UI elements based on model data
            // Since this is for GUI testing, we'll log the values
            Debug.Log($"=== SceneSDD_103 Update ===");
            Debug.Log($"Current Speed: {model.currentSpeed:F1} km/h");
            Debug.Log($"Speed Limit: {model.speedLimit:F0} km/h {(model.speedWarning ? "[WARNING]" : "")}");
            Debug.Log($"Direction: {model.currentDirection} ({model.compassHeading:F0}°)");
            
            if (model.isNavigationActive)
            {
                Debug.Log($"Destination: {model.destinationName}");
                Debug.Log($"Distance: {model.distanceToDestination:F1} km");
                Debug.Log($"ETA: {model.estimatedTimeArrival:F0} min");
                
                if (!string.IsNullOrEmpty(model.nextTurnDirection))
                {
                    Debug.Log($"Next Turn: {model.nextTurnDirection} in {model.distanceToNextTurn:F0}m {(model.navigationAlert ? "[ALERT]" : "")}");
                }
            }
            else
            {
                Debug.Log("Navigation: Inactive");
            }
            
            Debug.Log($"Display Options: Speed Limit: {model.showSpeedLimit}, Compass: {model.showCompass}, Navigation: {model.showNavigation}");
            Debug.Log($"==========================");

            // Here you would update actual UI components
            // UpdateSpeedDisplay(model.currentSpeed);
            // UpdateSpeedLimitDisplay(model.speedLimit, model.speedWarning);
            // UpdateCompassDisplay(model.currentDirection, model.compassHeading);
            // UpdateNavigationDisplay(model);
        }

        // Method để update từ external source
        public void UpdateFromModel()
        {
            if (sddModel != null)
            {
                UpdateView(sddModel);
            }
        }

        // Method để test update
        [ContextMenu("Test Update View")]
        public void TestUpdateView()
        {
            if (sddModel != null)
            {
                // Simulate some data changes
                sddModel.UpdateSpeed(Random.Range(30f, 80f));
                sddModel.UpdateSpeedLimit(Random.Range(40f, 60f));
                sddModel.UpdateCompass(Random.Range(0f, 360f));
                
                string[] destinations = {"Home", "Office", "Mall", "Airport"};
                sddModel.UpdateNavigation(
                    destinations[Random.Range(0, destinations.Length)], 
                    Random.Range(0.5f, 25f), 
                    Random.Range(5f, 45f)
                );
                
                string[] turnDirections = {"Left", "Right", "Straight", "Exit"};
                sddModel.UpdateNextTurn(
                    turnDirections[Random.Range(0, turnDirections.Length)], 
                    Random.Range(50f, 500f)
                );
                
                UpdateFromModel();
            }
            else
            {
                Debug.LogWarning("SDDModel not assigned for testing!");
            }
        }

        // Simulate navigation progress
        private void SimulateNavigationProgress()
        {
            if (sddModel != null && sddModel.isNavigationActive)
            {
                // Reduce distance to destination
                sddModel.distanceToDestination = Mathf.Max(0f, sddModel.distanceToDestination - 0.1f);
                
                // Reduce distance to next turn
                if (sddModel.distanceToNextTurn > 0)
                {
                    sddModel.distanceToNextTurn = Mathf.Max(0f, sddModel.distanceToNextTurn - 50f);
                    
                    // Reset turn when reached
                    if (sddModel.distanceToNextTurn <= 0)
                    {
                        string[] turnDirections = {"Left", "Right", "Straight"};
                        sddModel.UpdateNextTurn(
                            turnDirections[Random.Range(0, turnDirections.Length)], 
                            Random.Range(200f, 800f)
                        );
                    }
                }
                
                UpdateFromModel();
            }
        }

        // Public method để set model từ bên ngoài
        public void SetSDDModel(SDDModel model)
        {
            sddModel = model;
            if (IsActive)
            {
                UpdateFromModel();
            }
        }

        // Get current model
        public SDDModel GetSDDModel()
        {
            return sddModel;
        }

        // Utility methods cho testing
        public void StartNavigation(string destination, float distance)
        {
            if (sddModel != null)
            {
                sddModel.UpdateNavigation(destination, distance, distance * 2f); // Estimate 2 min per km
                UpdateFromModel();
            }
        }

        public void StopNavigation()
        {
            if (sddModel != null)
            {
                sddModel.UpdateNavigation("", 0f, 0f);
                UpdateFromModel();
            }
        }
    }
}
