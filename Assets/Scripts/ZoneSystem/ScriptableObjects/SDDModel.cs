using UnityEngine;
using System.Collections.Generic;

namespace ZoneSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SDDModel", menuName = "ZoneSystem/Models/SDD Model")]
    public class SDDModel : ScriptableObject
    {
        [Header("Speed Display Data")]
        public float currentSpeed = 0f;
        public float speedLimit = 50f;
        public bool isSpeedLimitActive = true;
        
        [Header("Direction Information")]
        public string currentDirection = "N";
        public float compassHeading = 0f; // 0-360 degrees
        
        [Header("Distance Information")]
        public float distanceToDestination = 0f; // km
        public string destinationName = "";
        public float estimatedTimeArrival = 0f; // minutes
        
        [Header("Navigation Status")]
        public bool isNavigationActive = false;
        public string nextTurnDirection = "";
        public float distanceToNextTurn = 0f; // meters
        
        [Header("Warning States")]
        public bool speedWarning = false;
        public bool navigationAlert = false;
        
        [Header("Display Settings")]
        public bool showSpeedLimit = true;
        public bool showCompass = true;
        public bool showNavigation = true;

        public void UpdateSpeed(float newSpeed)
        {
            currentSpeed = newSpeed;
            CheckSpeedWarning();
        }

        public void UpdateSpeedLimit(float newLimit)
        {
            speedLimit = newLimit;
            CheckSpeedWarning();
        }

        public void UpdateCompass(float heading)
        {
            compassHeading = heading;
            currentDirection = GetCardinalDirection(heading);
        }

        public void UpdateNavigation(string destination, float distance, float eta)
        {
            destinationName = destination;
            distanceToDestination = distance;
            estimatedTimeArrival = eta;
            isNavigationActive = !string.IsNullOrEmpty(destination);
        }

        public void UpdateNextTurn(string direction, float distance)
        {
            nextTurnDirection = direction;
            distanceToNextTurn = distance;
            navigationAlert = distance < 100f && isNavigationActive; // Alert if turn is within 100m
        }

        private void CheckSpeedWarning()
        {
            speedWarning = isSpeedLimitActive && currentSpeed > speedLimit + 5f; // 5 km/h tolerance
        }

        private string GetCardinalDirection(float heading)
        {
            string[] directions = {"N", "NE", "E", "SE", "S", "SW", "W", "NW"};
            int index = Mathf.RoundToInt(heading / 45f) % 8;
            return directions[index];
        }

        [ContextMenu("Test Update")]
        public void TestUpdate()
        {
            UpdateSpeed(Random.Range(30f, 80f));
            UpdateSpeedLimit(Random.Range(40f, 60f));
            UpdateCompass(Random.Range(0f, 360f));
            
            string[] destinations = {"Home", "Office", "Mall", "Airport"};
            string[] turnDirections = {"Left", "Right", "Straight", "Exit"};
            
            UpdateNavigation(
                destinations[Random.Range(0, destinations.Length)], 
                Random.Range(0.5f, 25f), 
                Random.Range(5f, 45f)
            );
            
            UpdateNextTurn(
                turnDirections[Random.Range(0, turnDirections.Length)], 
                Random.Range(50f, 500f)
            );
        }
    }
}
