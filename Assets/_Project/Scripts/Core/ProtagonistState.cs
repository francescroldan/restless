using UnityEngine;

namespace Restless.Core
{
    [CreateAssetMenu(menuName = "Restless/Protagonist State")]
    public class ProtagonistState : ScriptableObject
    {
        [Range(0f, 100f)] public float mentalHealth = 100f;
        [Range(0f, 100f)] public float physicalHealth = 100f;
        public int inventoryGridWidth = 4;
        public int inventoryGridHeight = 5;
        public int totalAbruptWakeUps = 0;

        [Range(0f, 180f)] public float dreamDurationOverride = 0f;

        // Base dream duration in seconds, reduced by abrupt wake-ups (override if set)
        public float BaseDreamDuration => dreamDurationOverride > 0f
            ? dreamDurationOverride
            : Mathf.Max(40f, 75f - totalAbruptWakeUps * 3f);

        public void ApplyNormalRun(float mentalCost, float physicalCost)
        {
            mentalHealth   = Mathf.Max(0f, mentalHealth   - mentalCost);
            physicalHealth = Mathf.Max(0f, physicalHealth - physicalCost);
        }

        public void ApplyAbruptWakeUp(float mentalDamage, float physicalDamage)
        {
            mentalHealth   = Mathf.Max(0f, mentalHealth   - mentalDamage);
            physicalHealth = Mathf.Max(0f, physicalHealth - physicalDamage);
            totalAbruptWakeUps++;

            // Each abrupt wake-up shrinks the inventory by 1 cell (min 12)
            int totalCells = inventoryGridWidth * inventoryGridHeight;
            if (totalCells > 12)
            {
                if (inventoryGridWidth > inventoryGridHeight)
                    inventoryGridWidth = Mathf.Max(3, inventoryGridWidth - 1);
                else
                    inventoryGridHeight = Mathf.Max(3, inventoryGridHeight - 1);
            }
        }

        public void ResetForNewGame()
        {
            mentalHealth = 100f;
            physicalHealth = 100f;
            inventoryGridWidth = 4;
            inventoryGridHeight = 5;
            totalAbruptWakeUps = 0;
        }
    }
}
