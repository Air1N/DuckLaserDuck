#if UNITY_EDITOR
using Game.State;
using UnityEditor;
using UnityEngine;

namespace Game.Dialogue.Debuga
{
    /// <summary>
    /// Editor window for inspecting and resetting dialogue state at runtime.
    /// Open via  Tools → Dialogue Debug  while in Play Mode.
    /// All changes write through <see cref="DialogueState"/> so they persist correctly.
    /// </summary>
    public class DialogueDebugMenu : EditorWindow
    {
        #region State
        private string _singleID = string.Empty;
        private string _flagID = string.Empty;
        private Vector2 _scroll;
        #endregion

        #region Window Lifecycle

        [MenuItem("Tools/Dialogue Debug")]
        public static void Open() => GetWindow<DialogueDebugMenu>("Dialogue Debug");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dialogue State Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use this window.", MessageType.Info);
                return;
            }

            DrawResetSection();
            EditorGUILayout.Space(6);
            DrawFlagSection();
            EditorGUILayout.Space(6);
            DrawCompletedList();
        }

        // Repaints the window at ~10 fps so the completed list stays live
        private void OnInspectorUpdate() => Repaint();

        #endregion

        #region Draw Sections

        private void DrawResetSection()
        {
            EditorGUILayout.LabelField("Reset / Replay", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _singleID = EditorGUILayout.TextField(
                new GUIContent("Sequence / Flag ID",
                    "Type the sequenceID or flagID to remove from the completed set."),
                _singleID);
            if (GUILayout.Button("Reset Single", GUILayout.Width(100)))
            {
                if (!string.IsNullOrWhiteSpace(_singleID))
                    FlagStateManager.Clear(_singleID.Trim());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Reset ALL Dialogue Progress"))
                if (EditorUtility.DisplayDialog(
                    "Reset All?",
                    "This will wipe every completed sequence and flag. Cannot be undone.",
                    "Reset", "Cancel"))
                    FlagStateManager.Reset();
            GUI.color = Color.white;
        }

        private void DrawFlagSection()
        {
            EditorGUILayout.LabelField("Manual Flags", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _flagID = EditorGUILayout.TextField(
                new GUIContent("Flag ID", "Set or clear a non-dialogue prerequisite flag."),
                _flagID);

            if (GUILayout.Button("Set", GUILayout.Width(48)))
                if (!string.IsNullOrWhiteSpace(_flagID))
                    FlagStateManager.Set(_flagID.Trim());

            if (GUILayout.Button("Clear", GUILayout.Width(48)))
                if (!string.IsNullOrWhiteSpace(_flagID))
                    FlagStateManager.Clear(_flagID.Trim());

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCompletedList()
        {
            EditorGUILayout.LabelField(
                $"Completed IDs & Flags ({FlagStateManager.GetAll().Count})",
                EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(160));
            foreach (string id in FlagStateManager.GetAll())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(id, EditorStyles.miniLabel);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    FlagStateManager.Clear(id);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}
#endif