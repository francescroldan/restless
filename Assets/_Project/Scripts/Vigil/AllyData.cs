using UnityEngine;

namespace Restless.Vigil
{
    public enum AllyArchetype { Doctor, Housekeeper, Occultist, Addict, Pet }

    [CreateAssetMenu(menuName = "Restless/Ally Data", fileName = "AllyData_")]
    public class AllyData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public AllyArchetype archetype;

        [Header("Visual")]
        public Sprite roomSprite;
        public Sprite iconSprite;

        [Header("Lighting")]
        public Color lightColor = new Color(0.9f, 0.8f, 0.6f);
        [Range(0f, 2f)] public float lightIntensity = 0.8f;
        [Range(0.5f, 8f)] public float lightRadius = 3f;

        [Header("Passive (reference only)")]
        [TextArea(2, 4)] public string passiveDescription;

        [Header("Gameplay modifiers")]
        public float dreamDurationBonus = 0f;
        public float restlessnessRateModifier = 0f;
    }
}
