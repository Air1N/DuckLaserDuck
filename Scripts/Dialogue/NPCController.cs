using Game.State;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Dialogue
{
    /// <summary>
    /// Attach to each NPC GameObject. Expects exactly two child GameObjects each
    /// carrying a <see cref="Collider2D"/> marked as trigger:
    /// one for proximity (larger) and one for interaction (smaller).
    /// Assign them in the Inspector.
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Data")]
        [SerializeField] private NPCDialogueData _dialogueData;
        [SerializeField] private DialogueBubble _dialogueBubble;

        [Header("Input")]
        [Tooltip("The InputActionReference for the interact action " +
                 "(e.g. left-click or a dedicated 'Interact' button).")]
        [SerializeField] private InputActionReference _interactAction;

        [Header("Collider Roots")]
        [Tooltip("Child GameObject whose Collider2D (trigger) defines the small " +
                 "interaction zone. The player must be inside here to start interaction dialogue.")]
        [SerializeField] private GameObject _interactionZone;

        [Tooltip("Child GameObject whose Collider2D (trigger) defines the large " +
                 "proximity zone. Proximity sequences fire automatically on enter.")]
        [SerializeField] private GameObject _proximityZone;
        #endregion

        #region Private State
        private bool _playerInInteractionZone;
        private bool _playerInProximityZone;

        [Header("Ambient Idle Lines")]
        [Tooltip("Lines displayed automatically at random intervals while the player " +
         "is inside the proximity zone but not in active dialogue. " +
         "These are plain DialogueLines — no sequence, no prerequisites.")]
        [SerializeField] private DialogueLine[] _ambientIdleLines;

        [Tooltip("Minimum seconds between ambient idle lines.")]
        [SerializeField] private float _ambientIdleMinDelay = 6f;

        [Tooltip("Maximum seconds between ambient idle lines.")]
        [SerializeField] private float _ambientIdleMaxDelay = 14f;
        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (_interactAction != null)
            {
                _interactAction.action.Enable();
                _interactAction.action.performed += OnInteractPerformed;
            }
        }

        private void OnDisable()
        {
            if (_interactAction != null)
                _interactAction.action.performed -= OnInteractPerformed;
        }

        #endregion

        #region Input Callback

        /// <summary>
        /// Called by the Input System when the interact action fires.
        /// Delegates to interaction-type dialogue if the player is in range,
        /// or forwards the request to DialogueManager as a skip if dialogue is active.
        /// </summary>
        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            if (DialogueManager.Instance == null) return;

            if (DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.RequestSkip();
                return;
            }

            if (_playerInInteractionZone)
                StartBestSequence(SequenceTriggerType.Interaction);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Force-starts a specific sequence by its ID.
        /// Respects the one-shot flag. Useful for scripted/game-manager-driven events.
        /// </summary>
        /// <param name="sequenceID">The unique ID of the sequence to trigger.</param>
        public void TriggerSequenceByID(string sequenceID)
        {
            if (_dialogueData == null) return;

            foreach (DialogueSequence seq in _dialogueData.orderedSequences)
            {
                if (seq == null || seq.sequenceID != sequenceID) continue;
                if (seq.oneShot && FlagStateManager.IsSet(seq.sequenceID))
                {
                    Debug.LogWarning($"[NPCController] Sequence '{sequenceID}' is already completed.");
                    return;
                }
                PlaySequence(seq);
                return;
            }
            Debug.LogWarning($"[NPCController] Sequence ID '{sequenceID}' not found on {name}.");
        }

        #endregion

        #region Trigger Detection

        /// <summary>
        /// Receives trigger-enter events from a child <see cref="NPCTriggerRelay"/>.
        /// Proximity entry fires a proximity sequence and starts ambient idle chatter.
        /// </summary>
        public void OnZoneEnter(SequenceTriggerType zoneType)
        {
            if (zoneType == SequenceTriggerType.Interaction)
                _playerInInteractionZone = true;

            if (zoneType == SequenceTriggerType.Proximity)
            {
                _playerInProximityZone = true;

                if (!DialogueManager.Instance.IsDialogueActive)
                    StartBestSequence(SequenceTriggerType.Proximity);

                // Begin ambient idle regardless — StartAmbientIdle is a no-op if pool is empty
                DialogueManager.Instance.StartAmbientIdle(
                    _ambientIdleLines, _dialogueBubble,
                    _ambientIdleMinDelay, _ambientIdleMaxDelay);
            }
        }

        /// <summary>
        /// Receives trigger-exit events from a child <see cref="NPCTriggerRelay"/>.
        /// Stops ambient idle when the player leaves the proximity zone.
        /// </summary>
        public void OnZoneExit(SequenceTriggerType zoneType)
        {
            if (zoneType == SequenceTriggerType.Interaction)
                _playerInInteractionZone = false;

            if (zoneType == SequenceTriggerType.Proximity)
            {
                _playerInProximityZone = false;
                DialogueManager.Instance.StopAmbientIdle();
            }
        }
        #endregion

        #region Sequence Selection

        /// <summary>
        /// Finds the best available sequence for the given trigger type and plays it.
        /// Ordered sequences are checked first; if none qualify, the weighted random
        /// pool for that trigger type is sampled.
        /// </summary>
        /// <param name="triggerType">The trigger context to search within.</param>
        private void StartBestSequence(SequenceTriggerType triggerType)
        {
            DialogueSequence sequence =
                GetAvailableOrderedSequence(triggerType) ??
                GetRandomPoolSequence(triggerType);

            if (sequence != null) PlaySequence(sequence);
        }

        /// <summary>
        /// Walks <see cref="NPCDialogueData.orderedSequences"/> top-to-bottom and returns
        /// the first entry that matches the trigger type, has all prerequisites satisfied,
        /// and has not already been one-shot completed.
        /// </summary>
        private DialogueSequence GetAvailableOrderedSequence(SequenceTriggerType triggerType)
        {
            if (_dialogueData?.orderedSequences == null) return null;

            foreach (DialogueSequence seq in _dialogueData.orderedSequences)
            {
                if (seq == null) continue;
                if (seq.triggerType != triggerType) continue;
                if (seq.oneShot && FlagStateManager.IsSet(seq.sequenceID)) continue;
                if (!FlagStateManager.AllSet(seq.prerequisiteIDs)) continue;
                return seq;
            }
            return null;
        }

        /// <summary>
        /// Draws one sequence at random from the ambient pool matching <paramref name="triggerType"/>.
        /// Entries that are completed one-shots or have unmet prerequisites are excluded
        /// before the weighted draw so weights always reflect actual candidates.
        /// Returns null if the pool is empty or no entry currently qualifies.
        /// </summary>
        private DialogueSequence GetRandomPoolSequence(SequenceTriggerType triggerType)
        {
            WeightedSequence[] pool = triggerType == SequenceTriggerType.Proximity
                ? _dialogueData?.ambientProximityPool
                : _dialogueData?.ambientInteractionPool;

            if (pool == null || pool.Length == 0) return null;

            int totalWeight = 0;
            var candidates = new System.Collections.Generic.List<(DialogueSequence seq, int cumulative)>();

            foreach (WeightedSequence ws in pool)
            {
                if (ws?.sequence == null) continue;
                if (ws.sequence.oneShot && FlagStateManager.IsSet(ws.sequence.sequenceID)) continue;
                if (!FlagStateManager.AllSet(ws.sequence.prerequisiteIDs)) continue;

                totalWeight += ws.weight;
                candidates.Add((ws.sequence, totalWeight));
            }

            if (candidates.Count == 0) return null;

            int roll = Random.Range(0, totalWeight);
            foreach ((DialogueSequence seq, int cumulative) in candidates)
                if (roll < cumulative) return seq;

            return candidates[candidates.Count - 1].seq;
        }

        /// <summary>Sends the resolved sequence to DialogueManager and identifies this NPC as speaker.</summary>
        private void PlaySequence(DialogueSequence sequence)
            => DialogueManager.Instance.StartDialogue(sequence, _dialogueBubble, this);

        #endregion 
    }
}