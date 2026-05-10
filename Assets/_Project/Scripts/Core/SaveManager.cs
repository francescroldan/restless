using System;
using System.IO;
using UnityEngine;

namespace Restless.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private static readonly string SaveFileName = "save.json";
        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public SaveData Data { get; private set; } = new SaveData();

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
            DeleteSave();
#else
            Load();
#endif
        }

        public void UnlockTestAllies()
        {
            Data.unlockedAllyIds.Add("sage");
            Data.unlockedAllyIds.Add("hero");
        }

        public void Save()
        {
            Data.lastSaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data, prettyPrint: true));
            Debug.Log($"[SaveManager] Saved to {SavePath}");
        }

        public void Load()
        {
            if (!File.Exists(SavePath)) { Data = new SaveData(); return; }
            Data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) ?? new SaveData();
            Debug.Log("[SaveManager] Save loaded.");
        }

        public void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Data = new SaveData();
            Debug.Log("[SaveManager] Save deleted.");
        }

        // ── Ally helpers ───────────────────────────────────────────────────

        public bool IsAllyUnlocked(string id) =>
            Data.unlockedAllyIds.Contains(id);

        public void UnlockAlly(string id)
        {
            if (Data.unlockedAllyIds.Contains(id)) return;
            Data.unlockedAllyIds.Add(id);
            Save();
        }

        public void LockAlly(string id)
        {
            if (!Data.unlockedAllyIds.Remove(id)) return;
            Save();
        }

        public void SetSelectedAllies(System.Collections.Generic.List<string> ids)
        {
            Data.selectedAllyIds = new System.Collections.Generic.List<string>(ids);
            Save();
        }

        // ── Fragment helpers ───────────────────────────────────────────────

        public int CollectedFragmentCount => Data.collectedFragmentIds.Count;

        /// <summary>How many fragments were gained in the last committed run. Consumed once by MemoryUrnController on Vigilia entry.</summary>
        public int RecentFragmentsGained { get; private set; }

        public void ConsumeRecentGain() => RecentFragmentsGained = 0;

        /// <summary>
        /// Persists fragments from a completed run.
        /// On abrupt wake-up, <paramref name="lossOnAbrupt"/> fragments are dropped from the end of the list.
        /// </summary>
        public void CommitRunFragments(System.Collections.Generic.List<string> ids, bool abrupt, int lossOnAbrupt = 1)
        {
            if (ids == null || ids.Count == 0) { RecentFragmentsGained = 0; return; }

            int saveCount = abrupt ? Mathf.Max(0, ids.Count - lossOnAbrupt) : ids.Count;
            for (int i = 0; i < saveCount; i++)
                Data.collectedFragmentIds.Add(ids[i]);

            RecentFragmentsGained = saveCount;
            Debug.Log($"[SaveManager] Fragments committed: {saveCount}/{ids.Count} (abrupt={abrupt}, lost={ids.Count - saveCount}). Total: {Data.collectedFragmentIds.Count}");
            Save();
        }
    }
}
