using UnityEngine;
using ZoneSystem.Core;
using ZoneSystem.ScriptableObjects;
using System.Collections.Generic;
using System.Collections;

namespace ZoneSystem.Presenters
{
    public class ZoneWarning_300 : ZoneBase
    {
        [Header("Zone Warning Settings")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private List<GameObject> warningElements = new List<GameObject>();
        [SerializeField] private bool persistentWarning = true;
        [SerializeField] private float blinkInterval = 0.5f;
        [SerializeField] private bool enableBlinking = true;

        [Header("Warning Content")]
        [SerializeField] private string warningTitle = "System Warning";
        [SerializeField] private string warningMessage = "";
        [SerializeField] private WarningLevel currentWarningLevel = WarningLevel.Medium;
        [SerializeField] private List<string> activeWarnings = new List<string>();

        [Header("Warning Colors")]
        [SerializeField] private Color lowWarningColor = Color.yellow;
        [SerializeField] private Color mediumWarningColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private Color highWarningColor = Color.red;
        [SerializeField] private Color criticalWarningColor = Color.magenta;
        
        [Header("Zone Manager Model")]
        [SerializeField] private ZoneManagerModel zoneManagerModel;

        private Coroutine blinkCoroutine;
        private bool isBlinkVisible = true;

        public enum WarningLevel
        {
            Low,      // Yellow - Minor issues
            Medium,   // Orange - Attention needed
            High,     // Red - Immediate attention
            Critical  // Magenta - Emergency
        }

        protected override void Awake()
        {
            priority = 300;
            zoneName = "ZoneWarning";
            zoneType = ZoneType.Normal;
            base.Awake();
        }

        protected override void OnLoadResourcesInternal()
        {
            Debug.Log("ZoneWarning_300: Loading warning resources...");
            
            // Setup warning panel
            if (warningPanel != null)
            {
                warningPanel.SetActive(false); // Start hidden
            }

            // Setup warning elements
            foreach (var element in warningElements)
            {
                if (element != null)
                {
                    element.SetActive(false); // Start hidden
                }
            }

            Debug.Log("ZoneWarning_300: Warning resources loaded successfully");
        }

        protected override void OnUnloadResourcesInternal()
        {
            Debug.Log("ZoneWarning_300: Unloading warning resources...");
            
            // Stop blinking
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            // Hide all warning elements
            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }

            foreach (var element in warningElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }

            Debug.Log("ZoneWarning_300: Warning resources unloaded successfully");
        }

        public override void Activate()
        {
            base.Activate();
            
            // Show warning panel
            if (warningPanel != null)
            {
                warningPanel.SetActive(true);
            }

            // Show warning elements
            foreach (var element in warningElements)
            {
                if (element != null)
                {
                    element.SetActive(true);
                }
            }

            // Start blinking if enabled
            if (enableBlinking)
            {
                StartBlinking();
            }

            // Display warning content
            DisplayWarningContent();
            
            Debug.Log($"ZoneWarning_300: Activated with warning level: {currentWarningLevel}");
        }

        public override void Deactivate()
        {
            // Stop blinking first
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            // Hide all warning elements
            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }

            foreach (var element in warningElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }

            base.Deactivate();
            
            Debug.Log("ZoneWarning_300: Deactivated warning zone");
        }

        private void StartBlinking()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            blinkCoroutine = StartCoroutine(BlinkWarning());
        }

        private IEnumerator BlinkWarning()
        {
            while (true)
            {
                isBlinkVisible = !isBlinkVisible;
                
                // Apply blinking to warning elements
                foreach (var element in warningElements)
                {
                    if (element != null)
                    {
                        element.SetActive(isBlinkVisible);
                    }
                }

                yield return new WaitForSeconds(blinkInterval);
            }
        }

        private void DisplayWarningContent()
        {
            Color warningColor = GetWarningColor(currentWarningLevel);
            
            // Log warning content (in real implementation, this would update UI elements)
            Debug.Log($"=== ZONE WARNING ===");
            Debug.Log($"Level: {currentWarningLevel} (Color: {warningColor})");
            Debug.Log($"Title: {warningTitle}");
            Debug.Log($"Message: {warningMessage}");
            Debug.Log($"Active Warnings: {activeWarnings.Count}");
            foreach (var warning in activeWarnings)
            {
                Debug.Log($"  - {warning}");
            }
            Debug.Log($"Persistent: {persistentWarning}");
            Debug.Log($"Blinking: {enableBlinking} ({blinkInterval}s)");
            Debug.Log($"===================");

            // Here you would update actual UI components
            // UpdateWarningTitle(warningTitle);
            // UpdateWarningMessage(warningMessage);
            // UpdateWarningLevel(currentWarningLevel, warningColor);
            // UpdateWarningList(activeWarnings);
        }

        private Color GetWarningColor(WarningLevel level)
        {
            switch (level)
            {
                case WarningLevel.Low: return lowWarningColor;
                case WarningLevel.Medium: return mediumWarningColor;
                case WarningLevel.High: return highWarningColor;
                case WarningLevel.Critical: return criticalWarningColor;
                default: return Color.white;
            }
        }

        // Public methods để show different warning levels
        public void ShowLowWarning(string title, string message)
        {
            ShowWarning(WarningLevel.Low, title, message);
        }

        public void ShowMediumWarning(string title, string message)
        {
            ShowWarning(WarningLevel.Medium, title, message);
        }

        public void ShowHighWarning(string title, string message)
        {
            ShowWarning(WarningLevel.High, title, message);
        }

        public void ShowCriticalWarning(string title, string message)
        {
            ShowWarning(WarningLevel.Critical, title, message);
        }

        public void ShowWarning(WarningLevel level, string title, string message)
        {
            currentWarningLevel = level;
            warningTitle = title;
            warningMessage = message;

            // Add to active warnings if not already present
            string warningEntry = $"[{level}] {title}: {message}";
            if (!activeWarnings.Contains(warningEntry))
            {
                activeWarnings.Add(warningEntry);
            }

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

        // Warning management methods
        public void AddWarning(string warning, WarningLevel level = WarningLevel.Medium)
        {
            string warningEntry = $"[{level}] {warning}";
            if (!activeWarnings.Contains(warningEntry))
            {
                activeWarnings.Add(warningEntry);
                
                // Update current warning level to highest level
                if (level > currentWarningLevel)
                {
                    currentWarningLevel = level;
                }

                // If zone is active, refresh display
                if (State == ZoneState.Active)
                {
                    DisplayWarningContent();
                }
            }
        }

        public void RemoveWarning(string warning)
        {
            activeWarnings.RemoveAll(w => w.Contains(warning));
            
            // If no warnings left and not persistent, deactivate
            if (activeWarnings.Count == 0 && !persistentWarning)
            {
                ClearAllWarnings();
            }
        }

        public void ClearAllWarnings()
        {
            activeWarnings.Clear();
            
            if (zoneManagerModel != null)
            {
                zoneManagerModel.DeactivateZone(priority);
            }
            else
            {
                Deactivate();
            }
        }

        // Context menu methods for testing
        [ContextMenu("Test Low Warning")]
        private void TestLowWarning()
        {
            ShowLowWarning("Low Priority", "This is a low priority warning");
        }

        [ContextMenu("Test Medium Warning")]
        private void TestMediumWarning()
        {
            ShowMediumWarning("Medium Priority", "This is a medium priority warning");
        }

        [ContextMenu("Test High Warning")]
        private void TestHighWarning()
        {
            ShowHighWarning("High Priority", "This is a high priority warning - immediate attention needed");
        }

        [ContextMenu("Test Critical Warning")]
        private void TestCriticalWarning()
        {
            ShowCriticalWarning("CRITICAL", "This is a critical emergency warning!");
        }

        [ContextMenu("Add Multiple Warnings")]
        private void TestMultipleWarnings()
        {
            AddWarning("Engine temperature high", WarningLevel.High);
            AddWarning("Low fuel level", WarningLevel.Medium);
            AddWarning("Check engine", WarningLevel.Low);
            
            ShowWarning(currentWarningLevel, "Multiple Issues", $"{activeWarnings.Count} warnings active");
        }

        [ContextMenu("Clear All Warnings")]
        private void TestClearWarnings()
        {
            ClearAllWarnings();
        }

        // Getters and Setters
        public WarningLevel GetCurrentWarningLevel() => currentWarningLevel;
        public string GetWarningTitle() => warningTitle;
        public string GetWarningMessage() => warningMessage;
        public List<string> GetActiveWarnings() => new List<string>(activeWarnings);
        public int GetWarningCount() => activeWarnings.Count;

        public void SetBlinkingEnabled(bool enabled)
        {
            enableBlinking = enabled;
            if (State == ZoneState.Active)
            {
                if (enabled)
                {
                    StartBlinking();
                }
                else if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    blinkCoroutine = null;
                    
                    // Ensure all elements are visible
                    foreach (var element in warningElements)
                    {
                        if (element != null)
                        {
                            element.SetActive(true);
                        }
                    }
                }
            }
        }

        public void SetBlinkInterval(float interval)
        {
            blinkInterval = Mathf.Max(0.1f, interval);
        }

        public void SetPersistentWarning(bool persistent)
        {
            persistentWarning = persistent;
        }
    }
}
