using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>Determines how a sequence is initiated at runtime.</summary>
    public enum SequenceTriggerType
    {
        /// <summary>Player must click/interact while in the interaction zone.</summary>
        Interaction,
        /// <summary>Fires automatically when the player enters the proximity zone.</summary>
        Proximity
    }

    /// <summary>
    /// A pool of <see cref="DialogueLine"/> entries from which one is chosen at random.
    /// Use a pool with one entry to get deterministic behaviour for that step.
    /// </summary> 
    [System.Serializable]
    public class DialogueLineGroup
    {
        [Tooltip("One of these lines will be chosen at random when this step is reached.")]
        public DialogueLine[] options;

        /// <summary>Returns a random line from this group's options.</summary>
        public DialogueLine Pick()
        {
            if (options == null || options.Length == 0) return null;
            return options[Random.Range(0, options.Length)];
        }
    }

    /// <summary>
    /// A ScriptableObject representing one ordered block of dialogue.
    /// Each step in the sequence is a <see cref="DialogueLineGroup"/>; if a group
    /// contains multiple options one is picked at random when that step is reached.
    /// Sequences are selected at runtime by <see cref="NPCController"/>.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewDialogueSequence",
        menuName = "Game/Dialogue/Dialogue Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        #region Identity
        [Header("Identity")]
        [Tooltip("Globally unique ID used for save data and prerequisite checks.")]
        public string sequenceID;
        #endregion

        #region Flow Control
        [Header("Flow Control")]
        [Tooltip("IDs of sequences that must be completed before this one becomes available.")]
        public string[] prerequisiteIDs;

        [Tooltip("If true, this sequence is marked complete after playing once and will not play again.")]
        public bool oneShot = true;

        [Tooltip("How this sequence is triggered.")]
        public SequenceTriggerType triggerType = SequenceTriggerType.Interaction;
        #endregion

        #region Content
        [Header("Content — Line Groups")]
        [Tooltip("Each entry is a step in the sequence. " +
                 "If a step has multiple options, one is chosen at random. " +
                 "Use a single option per step for deterministic dialogue.")]
        public DialogueLineGroup[] lineGroups;
        #endregion

        #region Public API

        /// <summary>
        /// Resolves the sequence into a concrete array of <see cref="DialogueLine"/> objects
        /// by picking one random option from each <see cref="DialogueLineGroup"/>.
        /// Call once per playback so the selection stays consistent for that run.
        /// </summary>
        /// <returns>Ordered array of resolved lines ready for playback.</returns>
        public DialogueLine[] ResolveLines()
        {
            if (lineGroups == null || lineGroups.Length == 0)
                return System.Array.Empty<DialogueLine>();

            DialogueLine[] resolved = new DialogueLine[lineGroups.Length];
            for (int i = 0; i < lineGroups.Length; i++)
                resolved[i] = lineGroups[i].Pick();

            return resolved;
        }

        #endregion
    }
}