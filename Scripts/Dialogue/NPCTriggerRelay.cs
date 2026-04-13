using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>
    /// Place on each zone child GameObject (interaction zone, proximity zone).
    /// Set <see cref="_zoneType"/> in the Inspector to identify which zone this is.
    /// Forwards 2D trigger enter/exit events up to the parent <see cref="NPCController"/>.
    /// Requires a <see cref="Collider2D"/> set to Is Trigger on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class NPCTriggerRelay : MonoBehaviour
    {
        #region Inspector Fields
        [Tooltip("Whether this collider is the Interaction zone (small, player must be " +
                 "inside to talk) or the Proximity zone (large, triggers ambient dialogue " +
                 "automatically when the player walks near).")]
        [SerializeField] private SequenceTriggerType _zoneType;

        [Tooltip("The NPCController to notify. Auto-resolved from the parent " +
                 "hierarchy if left empty.")]
        [SerializeField] private NPCController _controller;
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_controller == null)
                _controller = GetComponentInParent<NPCController>();

            if (_controller == null)
                Debug.LogError($"[NPCTriggerRelay] No NPCController found in parent of {name}.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _controller?.OnZoneEnter(_zoneType);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _controller?.OnZoneExit(_zoneType);
        }

        #endregion
    }
}