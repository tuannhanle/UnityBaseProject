using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.ScriptableObjects;

namespace ZoneSystem.Scenes
{
    public class SceneEco_101 : SceneWithID
    {
        [Header("Eco Scene Settings")]
        [SerializeField] private EcoModel ecoModel;
        [SerializeField] private bool showDebugInfo = true;

        protected override void Awake()
        {
            sceneID = 101;
            sceneName = "SceneEco";
            position = ScenePosition.TopRight;
            base.Awake();
        }

        protected override void OnInitializeInternal()
        {
            if (ecoModel == null)
            {
                Debug.LogWarning($"SceneEco_101: EcoModel chưa được assign!");
            }
            
            Debug.Log($"SceneEco_101 initialized with ID: {sceneID}");
        }

        protected override void OnActivateInternal()
        {
            if (showDebugInfo)
                Debug.Log($"SceneEco_101 activated - showing eco information");
        }

        protected override void OnDeactivateInternal()
        {
            if (showDebugInfo)
                Debug.Log($"SceneEco_101 deactivated");
        }

        protected override void OnUpdateViewInternal<T>(T data)
        {
            // Handle different data types
            if (data is EcoModel ecoData)
            {
                UpdateEcoView(ecoData);
            }
            else if (ecoModel != null)
            {
                // Use assigned model if no specific data provided
                UpdateEcoView(ecoModel);
            }
            else
            {
                Debug.LogWarning($"SceneEco_101: Không thể update view - không có data hoặc model");
            }
        }

        private void UpdateEcoView(EcoModel model)
        {
            if (model == null) return;

            // Update UI elements based on model data
            // Since this is for GUI testing, we'll log the values
            Debug.Log($"=== SceneEco_101 Update ===");
            Debug.Log($"Fuel Level: {model.fuelLevel}%");
            Debug.Log($"Efficiency: {model.currentEfficiency} L/100km");
            Debug.Log($"Eco Rating: {model.ecoRating}");
            Debug.Log($"Eco Mode: {(model.isEcoModeActive ? "ON" : "OFF")}");
            Debug.Log($"Distance: {model.totalDistance} km");
            Debug.Log($"===========================");

            // Here you would update actual UI components
            // UpdateFuelLevelDisplay(model.fuelLevel);
            // UpdateEfficiencyDisplay(model.currentEfficiency);
            // UpdateEcoRatingDisplay(model.ecoRating);
        }

        // Method để update từ external source
        public void UpdateFromModel()
        {
            if (ecoModel != null)
            {
                UpdateView(ecoModel);
            }
        }

        // Method để test update
        [ContextMenu("Test Update View")]
        public void TestUpdateView()
        {
            if (ecoModel != null)
            {
                // Simulate some data changes
                ecoModel.UpdateEfficiency(Random.Range(4f, 12f));
                ecoModel.UpdateFuelLevel(Random.Range(20f, 100f));
                UpdateFromModel();
            }
            else
            {
                Debug.LogWarning("EcoModel not assigned for testing!");
            }
        }

        // Public method để set model từ bên ngoài
        public void SetEcoModel(EcoModel model)
        {
            ecoModel = model;
            if (IsActive)
            {
                UpdateFromModel();
            }
        }

        // Get current model
        public EcoModel GetEcoModel()
        {
            return ecoModel;
        }
    }
}
