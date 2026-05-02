using System;
using System.Collections.Generic;

namespace Restless.Core
{
    [Serializable]
    public class SaveData
    {
        public float mentalHealth = 100f;
        public float physicalHealth = 100f;
        public int inventoryGridCells = 20;
        public int totalAbruptWakeUps = 0;

        public List<string> unlockedAllyIds = new();
        public List<string> collectedFragmentIds = new();

        public int totalRuns = 0;
        public string lastSaveDate = "";
    }
}
