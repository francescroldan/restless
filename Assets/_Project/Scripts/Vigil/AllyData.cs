using System.Collections.Generic;
using UnityEngine;

namespace Restless.Vigil
{
    public enum AllyArchetype { Hero, Shadow, Caregiver, Sage, Anima, Mystic }

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
        public Sprite portraitSprite;

        [Header("Lighting")]
        public Color lightColor = new Color(0.9f, 0.8f, 0.6f);
        [Range(0f, 2f)] public float lightIntensity = 0.8f;
        [Range(0.5f, 8f)] public float lightRadius = 3f;

        [Header("Passive")]
        [TextArea(2, 4)] public string passiveDescription;
        public float dreamDurationBonus = 0f;
        [Range(-1f, 1f)] public float restlessnessRateModifier = 0f;
        [Range(0.3f, 1f)]  public float minigameSpeedMultiplier  = 1f;
        [Range(0.1f, 1f)]  public float healthCostMultiplier    = 1f;
        [Range(0, 8)]      public int   inventoryBonusCells      = 0;

        [Header("Incompatibilities")]
        public List<AllyData> incompatibleWith = new();
    }
}
