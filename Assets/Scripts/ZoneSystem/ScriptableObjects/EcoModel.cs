using UnityEngine;

namespace ZoneSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EcoModel", menuName = "ZoneSystem/Models/Eco Model")]
    public class EcoModel : ScriptableObject
    {
        [Header("Fuel Information")]
        public float fuelLevel = 100f;
        public float fuelCapacity = 60f; // Liters
        
        [Header("Efficiency Data")]
        public float currentEfficiency = 6.5f; // L/100km
        public float bestEfficiency = 5.2f;
        public float averageEfficiency = 7.8f;
        
        [Header("Trip Information")]
        public float totalDistance = 0f;
        public float tripDistance = 0f;
        public float averageSpeed = 0f;
        
        [Header("Eco Rating")]
        public string ecoRating = "C";
        public bool isEcoModeActive = false;
        
        [Header("Thresholds")]
        public float excellentThreshold = 6f;
        public float goodThreshold = 8f;
        public float averageThreshold = 10f;
        public float poorThreshold = 12f;

        public void UpdateEfficiency(float newEfficiency)
        {
            currentEfficiency = newEfficiency;
            UpdateEcoRating();
        }

        public void UpdateFuelLevel(float newLevel)
        {
            fuelLevel = Mathf.Clamp(newLevel, 0f, 100f);
        }

        private void UpdateEcoRating()
        {
            if (currentEfficiency <= excellentThreshold) ecoRating = "A";
            else if (currentEfficiency <= goodThreshold) ecoRating = "B";
            else if (currentEfficiency <= averageThreshold) ecoRating = "C";
            else if (currentEfficiency <= poorThreshold) ecoRating = "D";
            else ecoRating = "F";
            
            isEcoModeActive = currentEfficiency <= goodThreshold;
        }

        [ContextMenu("Test Update")]
        public void TestUpdate()
        {
            UpdateEfficiency(Random.Range(4f, 15f));
            UpdateFuelLevel(Random.Range(20f, 100f));
        }
    }
}
