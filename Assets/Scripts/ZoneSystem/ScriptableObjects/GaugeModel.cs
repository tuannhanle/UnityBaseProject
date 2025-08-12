using UnityEngine;

namespace ZoneSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GaugeModel", menuName = "ZoneSystem/Models/Gauge Model")]
    public class GaugeModel : ScriptableObject
    {
        [Header("Speed Information")]
        public float currentSpeed = 0f; // km/h
        public float maxSpeed = 200f;
        
        [Header("Engine Information")]
        public float rpmPercent = 0f; // 0-100%
        public float maxRPM = 7000f;
        public float currentRPM = 0f;
        
        [Header("Fuel Information")]
        public float fuelPercent = 100f;
        public float fuelLiters = 60f;
        
        [Header("Temperature")]
        public float engineTemp = 85f; // Celsius
        public float normalTemp = 90f;
        public float warningTemp = 100f;
        
        [Header("Vehicle Status")]
        public string gearPosition = "P";
        public bool warningLights = false;
        public bool engineRunning = false;
        
        [Header("Warning Thresholds")]
        public float lowFuelThreshold = 15f;
        public float highTempThreshold = 95f;
        public float highRPMThreshold = 85f;

        public void UpdateSpeed(float newSpeed)
        {
            currentSpeed = Mathf.Clamp(newSpeed, 0f, maxSpeed);
            UpdateRPM();
            CheckWarnings();
        }

        public void UpdateRPM()
        {
            // Calculate RPM based on speed and gear
            if (engineRunning && gearPosition != "P" && gearPosition != "N")
            {
                currentRPM = (currentSpeed / maxSpeed) * maxRPM * 0.7f; // Approximate calculation
                rpmPercent = (currentRPM / maxRPM) * 100f;
            }
            else
            {
                currentRPM = engineRunning ? Random.Range(800f, 1200f) : 0f; // Idle RPM
                rpmPercent = (currentRPM / maxRPM) * 100f;
            }
        }

        public void UpdateFuel(float newFuelPercent)
        {
            fuelPercent = Mathf.Clamp(newFuelPercent, 0f, 100f);
            fuelLiters = (fuelPercent / 100f) * 60f; // Assuming 60L tank
            CheckWarnings();
        }

        public void UpdateEngineTemp(float newTemp)
        {
            engineTemp = newTemp;
            CheckWarnings();
        }

        public void SetGear(string gear)
        {
            gearPosition = gear;
            engineRunning = gear != "P";
            UpdateRPM();
        }

        private void CheckWarnings()
        {
            warningLights = fuelPercent < lowFuelThreshold || 
                           engineTemp > highTempThreshold || 
                           rpmPercent > highRPMThreshold;
        }

        [ContextMenu("Test Update")]
        public void TestUpdate()
        {
            UpdateSpeed(Random.Range(0f, 120f));
            UpdateFuel(Random.Range(10f, 100f));
            UpdateEngineTemp(Random.Range(75f, 105f));
            
            string[] gears = {"P", "R", "N", "D", "1", "2", "3"};
            SetGear(gears[Random.Range(0, gears.Length)]);
        }
    }
}
