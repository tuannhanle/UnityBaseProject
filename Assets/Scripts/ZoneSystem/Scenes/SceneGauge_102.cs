using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.ScriptableObjects;

namespace ZoneSystem.Scenes
{
    public class SceneGauge_102 : SceneWithID
    {
        [Header("Gauge Scene Settings")]
        [SerializeField] private GaugeModel gaugeModel;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool autoUpdateInterval = false;
        [SerializeField] private float updateInterval = 1f;

        private float lastUpdateTime;

        protected override void Awake()
        {
            sceneID = 102;
            sceneName = "SceneGauge";
            position = ScenePosition.BottomCenter;
            base.Awake();
        }

        protected virtual void Update()
        {
            if (autoUpdateInterval && IsActive && Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateFromModel();
                lastUpdateTime = Time.time;
            }
        }

        protected override void OnInitializeInternal()
        {
            if (gaugeModel == null)
            {
                Debug.LogWarning($"SceneGauge_102: GaugeModel chưa được assign!");
            }
            
            Debug.Log($"SceneGauge_102 initialized with ID: {sceneID}");
        }

        protected override void OnActivateInternal()
        {
            if (showDebugInfo)
                Debug.Log($"SceneGauge_102 activated - showing gauge information");
                
            lastUpdateTime = Time.time;
        }

        protected override void OnDeactivateInternal()
        {
            if (showDebugInfo)
                Debug.Log($"SceneGauge_102 deactivated");
        }

        protected override void OnUpdateViewInternal<T>(T data)
        {
            // Handle different data types
            if (data is GaugeModel gaugeData)
            {
                UpdateGaugeView(gaugeData);
            }
            else if (gaugeModel != null)
            {
                // Use assigned model if no specific data provided
                UpdateGaugeView(gaugeModel);
            }
            else
            {
                Debug.LogWarning($"SceneGauge_102: Không thể update view - không có data hoặc model");
            }
        }

        private void UpdateGaugeView(GaugeModel model)
        {
            if (model == null) return;

            // Update UI elements based on model data
            // Since this is for GUI testing, we'll log the values
            Debug.Log($"=== SceneGauge_102 Update ===");
            Debug.Log($"Speed: {model.currentSpeed:F1} km/h");
            Debug.Log($"RPM: {model.rpmPercent:F1}% ({model.currentRPM:F0} RPM)");
            Debug.Log($"Fuel: {model.fuelPercent:F1}% ({model.fuelLiters:F1}L)");
            Debug.Log($"Engine Temp: {model.engineTemp:F1}°C");
            Debug.Log($"Gear: {model.gearPosition}");
            Debug.Log($"Warnings: {(model.warningLights ? "ON" : "OFF")}");
            Debug.Log($"Engine: {(model.engineRunning ? "Running" : "Stopped")}");
            Debug.Log($"=============================");

            // Here you would update actual UI components
            // UpdateSpeedometer(model.currentSpeed);
            // UpdateRPMGauge(model.rpmPercent);
            // UpdateFuelGauge(model.fuelPercent);
            // UpdateTempGauge(model.engineTemp);
            // UpdateGearDisplay(model.gearPosition);
            // UpdateWarningLights(model.warningLights);
        }

        // Method để update từ external source
        public void UpdateFromModel()
        {
            if (gaugeModel != null)
            {
                UpdateView(gaugeModel);
            }
        }

        // Method để test update
        [ContextMenu("Test Update View")]
        public void TestUpdateView()
        {
            if (gaugeModel != null)
            {
                // Simulate some data changes
                gaugeModel.UpdateSpeed(Random.Range(0f, 120f));
                gaugeModel.UpdateFuel(Random.Range(10f, 100f));
                gaugeModel.UpdateEngineTemp(Random.Range(75f, 105f));
                
                string[] gears = {"P", "R", "N", "D", "1", "2", "3"};
                gaugeModel.SetGear(gears[Random.Range(0, gears.Length)]);
                
                UpdateFromModel();
            }
            else
            {
                Debug.LogWarning("GaugeModel not assigned for testing!");
            }
        }

        // Public method để set model từ bên ngoài
        public void SetGaugeModel(GaugeModel model)
        {
            gaugeModel = model;
            if (IsActive)
            {
                UpdateFromModel();
            }
        }

        // Get current model
        public GaugeModel GetGaugeModel()
        {
            return gaugeModel;
        }

        // Utility methods cho testing
        public void SimulateSpeedChange(float newSpeed)
        {
            if (gaugeModel != null)
            {
                gaugeModel.UpdateSpeed(newSpeed);
                UpdateFromModel();
            }
        }

        public void SimulateGearChange(string newGear)
        {
            if (gaugeModel != null)
            {
                gaugeModel.SetGear(newGear);
                UpdateFromModel();
            }
        }
    }
}
