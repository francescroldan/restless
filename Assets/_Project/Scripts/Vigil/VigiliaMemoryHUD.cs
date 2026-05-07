using UnityEngine;
using Restless.Core;

namespace Restless.Vigil
{
    public class VigiliaMemoryHUD : MonoBehaviour
    {
        private GUIStyle _style;

        private void OnGUI()
        {
            if (SaveManager.Instance == null) return;
            EnsureStyle();

            int collected = SaveManager.Instance.CollectedFragmentCount;
            int target    = GameManager.Instance.DemoFragmentTarget;
            bool complete = collected >= target;

            GUI.color = complete ? new Color(1f, 0.82f, 0.28f) : new Color(0.5f, 0.5f, 0.55f);
            GUI.Label(new Rect(10f, 10f, 140f, 20f),
                complete ? $"✓ Ecos  {collected}/{target}" : $"Ecos  {collected}/{target}",
                _style);

            GUI.color = Color.white;
        }

        private void EnsureStyle()
        {
            if (_style != null) return;
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
        }
    }
}
