using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Restless.Vigil;

/// <summary>
/// Menu: Restless > Setup Vigilia Scene
/// Rebuilds the Vigil scene hierarchy from scratch. Safe to re-run —
/// destroys any existing root objects named "_Managers", "Room", "Characters",
/// "Lighting" and "UI" before recreating them.
/// </summary>
public static class VigiliaSceneSetup
{
    private const string DataPath   = "Assets/_Project/Data/Allies";
    private const string SpritePath = "Assets/_Project/Art/Sprites/Placeholder/Vigilia";

    [MenuItem("Restless/Setup Vigilia Scene")]
    public static void Run()
    {
        EnsureFolders();
        var allies = EnsureAllyDataAssets();
        CleanExistingRoots();
        BuildHierarchy(allies);
        Debug.Log("[VigiliaSceneSetup] Done. Open the Vigil scene and press Play.");
    }

    // ── Folders ────────────────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/_Project/Data");
        EnsureFolder(DataPath);
        EnsureFolder("Assets/_Project/Art/Sprites/Placeholder/Vigilia");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    // ── AllyData assets ────────────────────────────────────────────────────

    private static AllyData[] EnsureAllyDataAssets()
    {
        var defs = new[]
        {
            new AllyDef("sage",     AllyArchetype.Sage,      "El Sabio",        new Color(0.9f, 0.85f, 0.55f), 0.9f, 3.5f, -0.30f, 0f,  "Presencia calmante. Reduce la Inquietud un 30%."),
            new AllyDef("hero",     AllyArchetype.Hero,      "El Héroe",        new Color(1.0f, 0.70f, 0.30f), 0.8f, 3.0f, 0.10f,  30f, "Impulso valiente. Aumenta la duración del sueño."),
            new AllyDef("shadow",   AllyArchetype.Shadow,    "La Sombra",       new Color(0.4f, 0.25f, 0.6f),  0.7f, 2.8f, 0f,     0f,  "Naturaleza dual. Sin efecto pasivo propio."),
            new AllyDef("caregiver",AllyArchetype.Caregiver, "El Cuidador",     new Color(0.4f, 0.75f, 0.5f),  0.8f, 3.2f, -0.15f, 20f, "Atención constante. Reduce levemente la Inquietud."),
            new AllyDef("anima",    AllyArchetype.Anima,     "El Ánima",        new Color(0.7f, 0.5f, 0.9f),   0.6f, 2.5f, 0f,     0f,  "Guía interior. Sin efecto pasivo propio."),
        };

        var result = new AllyData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            string assetPath = $"{DataPath}/AllyData_{d.Id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<AllyData>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<AllyData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }
            data.id                       = d.Id;
            data.displayName              = d.Name;
            data.archetype                = d.Archetype;
            data.lightColor               = d.LightColor;
            data.lightIntensity           = d.LightIntensity;
            data.lightRadius              = d.LightRadius;
            data.restlessnessRateModifier  = d.RestlessMod;
            data.dreamDurationBonus       = d.DreamBonus;
            data.passiveDescription       = d.PassiveDesc;
            data.roomSprite               = EnsurePlaceholderSprite(d.Id, d.SpriteColor);
            data.iconSprite               = EnsurePlaceholderSprite(d.Id + "_icon", d.LightColor, 16, 16);
            EditorUtility.SetDirty(data);
            result[i] = data;
        }
        AssetDatabase.SaveAssets();
        return result;
    }

    private static Sprite EnsurePlaceholderSprite(string name, Color color, int w = 32, int h = 48)
    {
        string path = $"{SpritePath}/{name}.png";
        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), path)))
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }

        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        File.WriteAllBytes(Path.Combine(Application.dataPath, path.Substring("Assets/".Length)), tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType         = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ── Scene cleanup ──────────────────────────────────────────────────────

    private static void CleanExistingRoots()
    {
        foreach (var name in new[] { "_Managers", "Room", "Characters", "Lighting", "UI" })
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    // ── Hierarchy ──────────────────────────────────────────────────────────

    private static void BuildHierarchy(AllyData[] allies)
    {
        // ── _Managers ───────────────────────────────────────────────────────
        var managers = new GameObject("_Managers");

        var roomCtrl = managers.AddComponent<VigiliaRoomController>();
        managers.AddComponent<VigiliaTransitionFX>();

        // ── Lighting ────────────────────────────────────────────────────────
        var lightingRoot = new GameObject("Lighting");

        var globalLight = MakeChild(lightingRoot, "GlobalLight").AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.intensity = 0.45f;
        globalLight.color     = new Color(0.55f, 0.60f, 0.75f);

        var bedLightGO = MakeChild(lightingRoot, "BedLight");
        bedLightGO.transform.position = new Vector3(0f, 0.3f, 0f);
        var bedLight = bedLightGO.AddComponent<Light2D>();
        bedLight.lightType             = Light2D.LightType.Point;
        bedLight.intensity             = 1.1f;
        bedLight.color                 = new Color(1f, 0.95f, 0.85f);
        bedLight.pointLightOuterRadius = 3.5f;

        // ── Room ────────────────────────────────────────────────────────────
        var roomRoot = new GameObject("Room");

        var floor = MakeChild(roomRoot, "Floor");
        floor.transform.localScale = new Vector3(10f, 8f, 1f);
        var floorSR = floor.AddComponent<SpriteRenderer>();
        floorSR.sprite       = EnsurePlaceholderSprite("room_floor", new Color(0.15f, 0.13f, 0.12f), 8, 8);
        floorSR.sortingOrder = -10;

        // ── Characters ──────────────────────────────────────────────────────
        var charRoot = new GameObject("Characters");

        var bedGO = MakeChild(charRoot, "ProtagonistBed");
        bedGO.transform.position = Vector3.zero;
        var bedSR = bedGO.AddComponent<SpriteRenderer>();
        bedSR.sprite       = EnsurePlaceholderSprite("protagonist_bed", new Color(0.7f, 0.7f, 0.7f), 24, 40);
        bedSR.sortingOrder = 0;
        var bedCollider = bedGO.AddComponent<BoxCollider2D>();
        bedCollider.size = new Vector2(1.2f, 2f);
        var bedComp = bedGO.AddComponent<ProtagonistBed>();

        var sleepIconGO = MakeChild(bedGO, "SleepIcon");
        sleepIconGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        var sleepIconSR = sleepIconGO.AddComponent<SpriteRenderer>();
        sleepIconSR.sprite       = EnsurePlaceholderSprite("sleep_icon", new Color(0.8f, 0.9f, 1f), 12, 12);
        sleepIconSR.sortingOrder = 5;
        sleepIconSR.enabled      = false;
        SetPrivateField(bedComp, "_sleepIcon", sleepIconSR);

        var allyPositions = new[]
        {
            new Vector3(-3.0f,  0.5f, 0f),  // sage       — left
            new Vector3(-2.5f, -2.0f, 0f),  // hero       — bottom-left
            new Vector3( 2.8f,  1.8f, 0f),  // shadow     — top-right
            new Vector3( 2.5f, -1.8f, 0f),  // caregiver  — bottom-right
            new Vector3( 0.6f,  0.2f, 0f),  // anima      — near bed
        };

        var slotList = new AllySlot[allies.Length];
        for (int i = 0; i < allies.Length; i++)
        {
            var ally   = allies[i];
            var allyGO = MakeChild(charRoot, $"Ally_{ally.archetype}");
            allyGO.transform.position = allyPositions[i];

            var sr = allyGO.AddComponent<SpriteRenderer>();
            sr.sprite       = ally.roomSprite;
            sr.sortingOrder = 1;

            var col = allyGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.5f);

            // RoomAllyPresence — hides sprite if not yet unlocked
            var presence = allyGO.AddComponent<RoomAllyPresence>();
            SetPrivateField(presence, "_allyData",       ally);
            SetPrivateField(presence, "_spriteRenderer", sr);

            var allyLight = MakeChild(allyGO, "Light").AddComponent<Light2D>();
            allyLight.lightType             = Light2D.LightType.Point;
            allyLight.color                 = ally.lightColor;
            allyLight.intensity             = ally.lightIntensity;
            allyLight.pointLightOuterRadius = ally.lightRadius;
            allyLight.enabled               = false;

            var hoverGO = MakeChild(allyGO, "HoverIndicator");
            hoverGO.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            var hoverSR = hoverGO.AddComponent<SpriteRenderer>();
            hoverSR.sprite       = ally.iconSprite;
            hoverSR.sortingOrder = 6;
            hoverSR.enabled      = false;

            var slot = allyGO.AddComponent<AllySlot>();
            SetPrivateField(slot, "_data",           ally);
            SetPrivateField(slot, "_allyLight",      allyLight);
            SetPrivateField(slot, "_hoverIndicator", hoverSR);
            slotList[i] = slot;
        }

        // ── UI ──────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("UI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // AllyInfoPanel
        var panelGO = MakeChild(canvasGO, "AllyInfoPanel");
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(1f, 0.5f);
        panelRT.anchorMax        = new Vector2(1f, 0.5f);
        panelRT.pivot            = new Vector2(1f, 0.5f);
        panelRT.anchoredPosition = new Vector2(-20f, 0f);
        panelRT.sizeDelta        = new Vector2(200f, 220f);
        var panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);

        var iconGO = MakeChild(panelGO, "Icon");
        var iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.15f);
        iconRT.anchorMax = new Vector2(0.9f, 0.85f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.preserveAspect = true;

        var infoPanel = panelGO.AddComponent<AllyInfoPanel>();
        SetPrivateField(infoPanel, "_panel",        panelRT);
        SetPrivateField(infoPanel, "_icon",         iconImg);
        SetPrivateField(infoPanel, "_hiddenOffset", new Vector2(280f, 0f));

        // ── Wire VigiliaRoomController ──────────────────────────────────────
        SetPrivateField(roomCtrl, "_allySlots",      slotList);
        SetPrivateField(roomCtrl, "_protagonistBed", bedComp);
        SetPrivateField(roomCtrl, "_allyInfoPanel",  infoPanel);
        SetPrivateField(roomCtrl, "_globalLight",    globalLight);
        SetPrivateField(roomCtrl, "_bedLight",       bedLight);

        // ── Finalize ────────────────────────────────────────────────────────
        EditorUtility.SetDirty(managers);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[VigiliaSceneSetup] Hierarchy created. Assign ProtagonistState in VigiliaRoomController.");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static GameObject MakeChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    // ── Data class ─────────────────────────────────────────────────────────

    private class AllyDef
    {
        public readonly string        Id;
        public readonly AllyArchetype Archetype;
        public readonly string        Name;
        public readonly Color         LightColor;
        public readonly float         LightIntensity;
        public readonly float         LightRadius;
        public readonly float         RestlessMod;
        public readonly float         DreamBonus;
        public readonly string        PassiveDesc;
        public readonly Color         SpriteColor;

        public AllyDef(string id, AllyArchetype archetype, string name,
                       Color lightColor, float intensity, float radius,
                       float restlessMod, float dreamBonus, string passiveDesc)
        {
            Id             = id;
            Archetype      = archetype;
            Name           = name;
            LightColor     = lightColor;
            LightIntensity = intensity;
            LightRadius    = radius;
            RestlessMod    = restlessMod;
            DreamBonus     = dreamBonus;
            PassiveDesc    = passiveDesc;
            SpriteColor    = new Color(lightColor.r * 0.6f, lightColor.g * 0.6f, lightColor.b * 0.6f);
        }
    }
}
