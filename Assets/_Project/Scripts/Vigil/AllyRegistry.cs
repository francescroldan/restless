using System.Collections.Generic;
using UnityEngine;

namespace Restless.Vigil
{
    [CreateAssetMenu(menuName = "Restless/Ally Registry", fileName = "AllyRegistry")]
    public class AllyRegistry : ScriptableObject
    {
        [SerializeField] private List<AllyData> _allies = new();

        public IReadOnlyList<AllyData> All => _allies;

        public AllyData GetById(string id)
        {
            foreach (var ally in _allies)
                if (ally != null && ally.id == id) return ally;
            return null;
        }
    }
}
