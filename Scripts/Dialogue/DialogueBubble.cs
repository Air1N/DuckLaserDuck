using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Dialogue
{
    /// <summary>
    /// Sits on the root of a World Space Canvas that is a child of an NPC GameObject.
    /// Exposes simple text-control methods for <see cref="DialogueManager"/> to call.
    /// The Canvas follows the NPC automatically because it is parented to it.
    /// </summary>
    public class DialogueBubble : MonoBehaviour
    {
        #region Inspector References
        [Header("References")]
        [Tooltip("The TMP_Text component inside the bubble panel.")]
        [SerializeField] private TMP_Text _textComponent;

        [Tooltip("The root panel GameObject (background image, etc.). " +
                 "This is what Show/Hide toggles.")]
        [SerializeField] private GameObject _bubbleRoot;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Always start hidden
            _bubbleRoot.SetActive(false);
        }
        #endregion

        #region Public API

        /// <summary>Makes the bubble visible.</summary>
        public void Show() => _bubbleRoot.SetActive(true);

        /// <summary>Hides the bubble and clears its text.</summary>
        public void Hide()
        {
            _bubbleRoot.SetActive(false);
            if (_textComponent != null) _textComponent.text = string.Empty;
        }

        /// <summary>
        /// Sets the full text content and controls how many characters are visible.
        /// TMP processes the full string (including rich-text tags) before revealing.
        /// </summary>
        /// <param name="fullText">The complete line text.</param>
        /// <param name="visibleCount">Number of characters to show immediately.</param>
        public void SetText(string fullText, int visibleCount)
        {
            _textComponent.text = fullText;
            _textComponent.maxVisibleCharacters = visibleCount;
        }

        /// <summary>Reveals exactly <paramref name="count"/> characters of the current text.</summary>
        public void SetVisibleCount(int count)
            => _textComponent.maxVisibleCharacters = count;

        /// <summary>Instantly reveals the full text (used when the player skips typing).</summary>
        public void ShowFullText(string fullText)
        {
            _textComponent.text = fullText;
            _textComponent.maxVisibleCharacters = fullText.Length;
        }

        #endregion 
    }
}