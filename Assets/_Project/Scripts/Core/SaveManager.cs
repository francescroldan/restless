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

        [Header("Editor testing")]
        [SerializeField] private string[] _editorDefaultUnlockedAllies = { "sage", "hero", "shadow", "caregiver" };

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
            DeleteSave();
            foreach (var id in _editorDefaultUnlockedAllies)
                if (!string.IsNullOrEmpty(id)) Data.unlockedAllyIds.Add(id);
#else
            Load();
#endif
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
    }
}
