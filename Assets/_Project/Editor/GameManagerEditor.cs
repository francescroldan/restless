using UnityEditor;
using UnityEngine;
using Restless.Core;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gm    = (GameManager)target;
        var state = gm.ProtagonistStateDebug;
        if (state == null) return;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("── Protagonist State ──", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int   mental   = EditorGUILayout.IntSlider("Mental Health",   Mathf.RoundToInt(state.mentalHealth),   0, 100);
        int   physical = EditorGUILayout.IntSlider("Physical Health", Mathf.RoundToInt(state.physicalHealth), 0, 100);
        int   duration = EditorGUILayout.IntSlider("Dream Duration Override (0 = auto)", Mathf.RoundToInt(state.dreamDurationOverride), 0, 180);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(state, "Edit Protagonist State");
            state.mentalHealth          = mental;
            state.physicalHealth        = physical;
            state.dreamDurationOverride = duration;
            EditorUtility.SetDirty(state);
        }

        EditorGUILayout.LabelField("Abrupt Wake-Ups",    state.totalAbruptWakeUps.ToString());
        EditorGUILayout.LabelField("Effective Duration", $"{state.BaseDreamDuration:F0}s");
    }
}
