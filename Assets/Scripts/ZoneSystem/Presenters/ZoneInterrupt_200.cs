using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.ScriptableObjects;
using System.Collections.Generic;

namespace ZoneSystem.Presenters
{
    public class ZoneInterrupt_200 : ZoneBase
    {
        [Header("Zone Interrupt Settings")]
        [SerializeField] private GameObject interruptPanel;
        [SerializeField] private List<GameObject> interruptElements = new List<GameObject>();
        [SerializeField] private float autoHideDelay = 5f;
        [SerializeField] private bool autoHideEnabled = true;

        [Header("Interrupt Content")]
        [SerializeField] private string interruptTitle = "System Interrupt";
        [SerializeField] private string interruptMessage = "";
        [SerializeField] private InterruptType currentInterruptType = InterruptType.Information;
        
        [Header("Zone Manager Model")]
        [SerializeField] private ZoneManagerModel zoneManagerModel;

        private float activationTime;
        private bool isTimerActive = false;

        public enum InterruptType
        {
            Information,
            Warning,
            Error,
            Success
        }

        protected override void Awake()
        {
            priority = 200;
            zoneName = "ZoneInterrupt";
            zoneType = ZoneType.Normal;
            base.Awake();
        }

        protected virtual void Update()
        {
            if (isTimerActive && autoHideEnabled && Time.time - activationTime >= autoHideDelay)
            {
                AutoDeactivate();
            }
        }

        protected override void OnLoadResourcesInternal()
        {
            Debug.Log("ZoneInterrupt_200: Loading interrupt resources...");
            
            // Setup interrupt panel
            if (interruptPanel != null)
            {
                interruptPanel.SetActive(false); // Start hidden
            }

            // Setup interrupt elements
            foreach (var element in interruptElements)
            {
                if (element != null)
                {
                    element.SetActive(false); // Start hidden
                }
            }

            Debug.Log("ZoneInterrupt_200: Interrupt resources loaded successfully");
        }

        protected override void OnUnloadResourcesInternal()
        {
            Debug.Log("ZoneInterrupt_200: Unloading interrupt resources...");
            
            // Hide all interrupt elements
            if (interruptPanel != null)
            {
                interruptPanel.SetActive(false);
            }

            foreach (var element in interruptElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }

            isTimerActive = false;
            Debug.Log("ZoneInterrupt_200: Interrupt resources unloaded successfully");
        }

        public override void Activate()
        {
            base.Activate();
            
            // Show interrupt panel
            if (interruptPanel != null)
            {
                interruptPanel.SetActive(true);
            }

            // Show interrupt elements
            foreach (var element in interruptElements)
            {
                if (element != null)
                {
                    element.SetActive(true);
                }
            }

            // Start auto-hide timer
            if (autoHideEnabled)
            {
                activationTime = Time.time;
                isTimerActive = true;
            }

            // Display interrupt content
            DisplayInterruptContent();
            
            Debug.Log($"ZoneInterrupt_200: Activated with message: '{interruptMessage}'");
        }

        public override void Deactivate()
        {
            // Hide all interrupt elements first
            if (interruptPanel != null)
            {
                interruptPanel.SetActive(false);
            }

            foreach (var element in interruptElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }

            isTimerActive = false;
            base.Deactivate();
            
            Debug.Log("ZoneInterrupt_200: Deactivated interrupt zone");
        }

        private void AutoDeactivate()
        {
            Debug.Log("ZoneInterrupt_200: Auto-deactivating after delay");
            isTimerActive = false;
            
            // Use ZoneManagerModel to deactivate this zone
            if (zoneManagerModel != null)
            {
                zoneManagerModel.DeactivateZone(priority);
            }
            else
            {
                Deactivate();
            }
        }

        private void DisplayInterruptContent()
        {
            // Log interrupt content (in real implementation, this would update UI elements)
            Debug.Log($"=== ZONE INTERRUPT ===");
            Debug.Log($"Type: {currentInterruptType}");
            Debug.Log($"Title: {interruptTitle}");
            Debug.Log($"Message: {interruptMessage}");
            Debug.Log($"Auto-hide: {autoHideEnabled} ({autoHideDelay}s)");
            Debug.Log($"====================");

            // Here you would update actual UI components
            // UpdateInterruptTitle(interruptTitle);
            // UpdateInterruptMessage(interruptMessage);
            // UpdateInterruptType(currentInterruptType);
        }

        // Public methods để show different types of interrupts
        public void ShowInformation(string title, string message, float hideDelay = 5f)
        {
            ShowInterrupt(InterruptType.Information, title, message, hideDelay);
        }

        public void ShowWarning(string title, string message, float hideDelay = 8f)
        {
            ShowInterrupt(InterruptType.Warning, title, message, hideDelay);
        }

        public void ShowError(string title, string message, float hideDelay = 10f)
        {
            ShowInterrupt(InterruptType.Error, title, message, hideDelay);
        }

        public void ShowSuccess(string title, string message, float hideDelay = 3f)
        {
            ShowInterrupt(InterruptType.Success, title, message, hideDelay);
        }

        public void ShowInterrupt(InterruptType type, string title, string message, float hideDelay = 5f)
        {
            currentInterruptType = type;
            interruptTitle = title;
            interruptMessage = message;
            autoHideDelay = hideDelay;

            // Activate this zone
            if (zoneManagerModel != null)
            {
                zoneManagerModel.ActivateZone(priority);
            }
            else
            {
                Activate();
            }
        }

        // Context menu methods for testing
        [ContextMenu("Test Information Interrupt")]
        private void TestInformationInterrupt()
        {
            ShowInformation("Information", "This is a test information message");
        }

        [ContextMenu("Test Warning Interrupt")]
        private void TestWarningInterrupt()
        {
            ShowWarning("Warning", "This is a test warning message");
        }

        [ContextMenu("Test Error Interrupt")]
        private void TestErrorInterrupt()
        {
            ShowError("Error", "This is a test error message");
        }

        [ContextMenu("Test Success Interrupt")]
        private void TestSuccessInterrupt()
        {
            ShowSuccess("Success", "Operation completed successfully");
        }

        [ContextMenu("Force Hide")]
        private void ForceHide()
        {
            if (zoneManagerModel != null)
            {
                zoneManagerModel.DeactivateZone(priority);
            }
            else
            {
                Deactivate();
            }
        }

        // Getters and Setters
        public InterruptType GetCurrentInterruptType() => currentInterruptType;
        public string GetInterruptTitle() => interruptTitle;
        public string GetInterruptMessage() => interruptMessage;
        public float GetAutoHideDelay() => autoHideDelay;
        public bool IsAutoHideEnabled() => autoHideEnabled;

        public void SetAutoHideEnabled(bool enabled)
        {
            autoHideEnabled = enabled;
            if (!enabled)
            {
                isTimerActive = false;
            }
        }

        public void SetAutoHideDelay(float delay)
        {
            autoHideDelay = Mathf.Max(0f, delay);
        }
    }
}
