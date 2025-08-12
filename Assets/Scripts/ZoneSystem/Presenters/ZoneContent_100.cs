using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.Scenes;
using ZoneSystem.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace ZoneSystem.Presenters
{
    public class ZoneContent_100 : ZoneBase
    {
        [Header("Zone Content Settings")]
        [SerializeField] private List<SceneWithID> managedScenes = new List<SceneWithID>();
        [SerializeField] private bool autoDiscoverChildScenes = true;
        
        [Header("Scene References")]
        [SerializeField] private SceneEco_101 sceneEco;
        [SerializeField] private SceneGauge_102 sceneGauge;
        [SerializeField] private SceneSDD_103 sceneSDD;

        [Header("Models")]
        [SerializeField] private EcoModel ecoModel;
        [SerializeField] private GaugeModel gaugeModel;
        [SerializeField] private SDDModel sddModel;
        [SerializeField] private ZoneManagerModel zoneManagerModel;

        protected override void Awake()
        {
            priority = 100;
            zoneName = "ZoneContent";
            zoneType = ZoneType.Normal;
            base.Awake();
        }

        protected void Start()
        {
            SetupScenes();
        }

        private void SetupScenes()
        {
            if (autoDiscoverChildScenes)
            {
                AutoDiscoverScenes();
            }

            // Manually find and setup specific scenes
            if (sceneEco == null)
                sceneEco = FindObjectOfType<SceneEco_101>();
            
            if (sceneGauge == null)
                sceneGauge = FindObjectOfType<SceneGauge_102>();
                
            if (sceneSDD == null)
                sceneSDD = FindObjectOfType<SceneSDD_103>();

            // Add to managed scenes list
            if (sceneEco != null && !managedScenes.Contains(sceneEco))
            {
                managedScenes.Add(sceneEco);
                AddScene(sceneEco);
            }
            
            if (sceneGauge != null && !managedScenes.Contains(sceneGauge))
            {
                managedScenes.Add(sceneGauge);
                AddScene(sceneGauge);
            }
            
            if (sceneSDD != null && !managedScenes.Contains(sceneSDD))
            {
                managedScenes.Add(sceneSDD);
                AddScene(sceneSDD);
            }

            // Assign models to scenes
            AssignModelsToScenes();
            
            Debug.Log($"ZoneContent_100 setup completed with {managedScenes.Count} scenes");
        }

        private void AssignModelsToScenes()
        {
            if (sceneEco != null && ecoModel != null)
            {
                sceneEco.SetEcoModel(ecoModel);
                Debug.Log("Assigned EcoModel to SceneEco_101");
            }

            if (sceneGauge != null && gaugeModel != null)
            {
                sceneGauge.SetGaugeModel(gaugeModel);
                Debug.Log("Assigned GaugeModel to SceneGauge_102");
            }

            if (sceneSDD != null && sddModel != null)
            {
                sceneSDD.SetSDDModel(sddModel);
                Debug.Log("Assigned SDDModel to SceneSDD_103");
            }
        }

        protected override void OnLoadResourcesInternal()
        {
            Debug.Log("ZoneContent_100: Loading content resources...");
            
            // Load resources for all managed scenes
            foreach (var scene in managedScenes)
            {
                if (scene != null)
                {
                    scene.Initialize();
                }
            }

            Debug.Log("ZoneContent_100: Content resources loaded successfully");
        }

        protected override void OnUnloadResourcesInternal()
        {
            Debug.Log("ZoneContent_100: Unloading content resources...");
            
            // Deactivate all managed scenes
            foreach (var scene in managedScenes)
            {
                if (scene != null && scene.IsActive)
                {
                    scene.Deactivate();
                }
            }

            Debug.Log("ZoneContent_100: Content resources unloaded successfully");
        }

        public override void Activate()
        {
            base.Activate();
            
            // Activate all managed scenes
            foreach (var scene in managedScenes)
            {
                if (scene != null)
                {
                    scene.Activate();
                }
            }

            Debug.Log("ZoneContent_100: All content scenes activated");
        }

        public override void Deactivate()
        {
            // Deactivate all managed scenes first
            foreach (var scene in managedScenes)
            {
                if (scene != null)
                {
                    scene.Deactivate();
                }
            }

            base.Deactivate();
            Debug.Log("ZoneContent_100: All content scenes deactivated");
        }

        // Public methods để update specific scenes
        public void UpdateEcoScene()
        {
            if (sceneEco != null && ecoModel != null)
            {
                sceneEco.UpdateFromModel();
            }
        }

        public void UpdateGaugeScene()
        {
            if (sceneGauge != null && gaugeModel != null)
            {
                sceneGauge.UpdateFromModel();
            }
        }

        public void UpdateSDDScene()
        {
            if (sceneSDD != null && sddModel != null)
            {
                sceneSDD.UpdateFromModel();
            }
        }

        public void UpdateAllScenes()
        {
            UpdateEcoScene();
            UpdateGaugeScene();
            UpdateSDDScene();
            Debug.Log("ZoneContent_100: Updated all scenes");
        }

        // Context menu methods for testing
        [ContextMenu("Test Update All Scenes")]
        private void TestUpdateAllScenes()
        {
            // Test update all models first
            if (ecoModel != null)
            {
                ecoModel.TestUpdate();
            }
            
            if (gaugeModel != null)
            {
                gaugeModel.TestUpdate();
            }
            
            if (sddModel != null)
            {
                sddModel.TestUpdate();
            }

            // Then update scenes
            UpdateAllScenes();
        }

        [ContextMenu("List Managed Scenes")]
        private void ListManagedScenes()
        {
            Debug.Log($"=== ZoneContent_100 Managed Scenes ===");
            for (int i = 0; i < managedScenes.Count; i++)
            {
                var scene = managedScenes[i];
                if (scene != null)
                {
                    Debug.Log($"{i}: {scene.SceneName} (ID: {scene.SceneID}) - Active: {scene.IsActive}");
                }
                else
                {
                    Debug.Log($"{i}: NULL SCENE");
                }
            }
            Debug.Log($"=====================================");
        }

        // Getter methods
        public SceneEco_101 GetEcoScene() => sceneEco;
        public SceneGauge_102 GetGaugeScene() => sceneGauge;
        public SceneSDD_103 GetSDDScene() => sceneSDD;
        
        public EcoModel GetEcoModel() => ecoModel;
        public GaugeModel GetGaugeModel() => gaugeModel;
        public SDDModel GetSDDModel() => sddModel;

        // Setter methods for models (useful for runtime assignment)
        public void SetEcoModel(EcoModel model)
        {
            ecoModel = model;
            if (sceneEco != null)
            {
                sceneEco.SetEcoModel(model);
            }
        }

        public void SetGaugeModel(GaugeModel model)
        {
            gaugeModel = model;
            if (sceneGauge != null)
            {
                sceneGauge.SetGaugeModel(model);
            }
        }

        public void SetSDDModel(SDDModel model)
        {
            sddModel = model;
            if (sceneSDD != null)
            {
                sceneSDD.SetSDDModel(model);
            }
        }
    }
}
