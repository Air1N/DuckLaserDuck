// using UnityEngine;
// using Game.State;

// /// <summary>
// /// When the chosen condition is met, executes one or more actions:
// /// activate, deactivate, destroy, set/clear flags, play cutscenes,
// /// or register objects for room cleanup.
// /// </summary>
// public class ApplyOnCondition : ConditionMonitor
// {
//     /* ── Action Definition ───────────────────────────────── */

//     public enum ActionType
//     {
//         Activate,
//         Deactivate,
//         Destroy,
//         SetFlag,
//         ClearFlag,
//         PlayCutscene,
//         RegisterForRoomCleanup,
//     }

//     [System.Serializable]
//     public class ActionEntry
//     {
//         public ActionType type;

//         [Tooltip("Target for Activate / Deactivate / Destroy / RegisterForRoomCleanup. " +
//                  "Uses this GameObject if left empty.")]
//         public GameObject target;

//         [Tooltip("Flag ID for SetFlag / ClearFlag.")]
//         public string flagID;

//         [Tooltip("Cutscene prefab for PlayCutscene.")]
//         public Cutscene cutscenePrefab;

//         [Tooltip("Pause game during cutscene.")]
//         public bool pauseDuringCutscene = true;
//     }

//     /* ── Inspector ───────────────────────────────────────── */

//     [Header("Actions")]
//     [SerializeField] private ActionEntry[] actions;

//     /* ── Execution ───────────────────────────────────────── */

//     protected override void Execute()
//     {
//         if (actions == null) return;

//         foreach (var a in actions)
//         {
//             GameObject t = a.target != null ? a.target : gameObject;

//             switch (a.type)
//             {
//                 case ActionType.Activate:
//                     t.SetActive(true);
//                     break;

//                 case ActionType.Deactivate:
//                     t.SetActive(false);
//                     break;

//                 case ActionType.Destroy:
//                     Destroy(t);
//                     break;

//                 case ActionType.SetFlag:
//                     if (!string.IsNullOrEmpty(a.flagID))
//                         FlagStateManager.Set(a.flagID);
//                     break;

//                 case ActionType.ClearFlag:
//                     if (!string.IsNullOrEmpty(a.flagID))
//                         FlagStateManager.Clear(a.flagID);
//                     break;

//                 case ActionType.PlayCutscene:
//                     if (a.cutscenePrefab != null && CutsceneManager.Instance != null)
//                         CutsceneManager.Instance.Play(a.cutscenePrefab, a.pauseDuringCutscene);
//                     break;

//                 case ActionType.RegisterForRoomCleanup:
//                     var room = FindFirstObjectByType<RoomManager>();
//                     if (room != null)
//                         room.trashedObjectsAtRoomEnd.Add(t);
//                     break;
//             }
//         }
//     }
// }