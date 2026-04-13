using System.Collections;
using UnityEngine;
using TMPro;
using Game.State;

namespace Game.Dialogue
{
    /// <summary>
    /// Singleton that drives all active dialogue. Handles the typewriter reveal,
    /// skip-lock timing, voice-line audio, and sequence completion events.
    /// Place on a persistent GameObject in the first loaded scene.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class DialogueManager : MonoBehaviour
    {
        #region Singleton
        public static DialogueManager Instance { get; private set; }


        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
        }
        #endregion

        #region Inspector Settings
        [Header("Typewriter Settings")]
        [Tooltip("Global characters-per-second rate used when a line has no override.")]
        [SerializeField] private float _defaultTypingSpeed = 45f;

        [Tooltip("Seconds after a line starts before any skip input is accepted. " +
                 "Prevents accidental skips on the first frame.")]
        [SerializeField] private float _skipLockDuration = 0.45f;
        #endregion

        #region Events
        /// <summary>Fired when a new dialogue sequence begins.</summary>
        public event System.Action OnDialogueStarted;

        /// <summary>Fired with the completed sequence's ID when a sequence finishes.</summary>
        public event System.Action<string> OnSequenceCompleted;

        /// <summary>Fired when all dialogue ends and the bubble is hidden.</summary>
        public event System.Action OnDialogueEnded;
        #endregion

        #region Public State
        /// <summary>True while a dialogue sequence is actively running.</summary>
        public bool IsDialogueActive { get; private set; }
        #endregion

        #region Private State
        private AudioSource _audioSource;
        private DialogueSequence _currentSequence;
        private DialogueBubble _currentBubble;
        private Coroutine _sequenceCoroutine;
        // Ambient idle state
        private Coroutine _ambientCoroutine;
        private NPCController _currentSpeaker;

        // Per-line flags written by coroutine, read by RequestSkip
        private bool _typingComplete;
        private bool _awaitingAdvance;
        private bool _skipRequested;
        private bool _skipLocked;
        private DialogueLine[] _resolvedLines;

        // Compiled regex reused across all strip calls — compiled once, never GC'd mid-play
        private static readonly System.Text.RegularExpressions.Regex _richTagRegex =
            new System.Text.RegularExpressions.Regex(
                @"<[^>]*>",
                System.Text.RegularExpressions.RegexOptions.Compiled);
        #endregion

        #region Public API

        /// <summary>
        /// Begins playing a dialogue sequence on the supplied bubble.
        /// Lines are resolved from the sequence's line groups at call time so random
        /// picks are locked in for the duration of that playback.
        /// Stops any active ambient idle coroutine before beginning.
        /// If dialogue is already active it is stopped first.
        /// </summary>
        /// <param name="sequence">The sequence to play.</param>
        /// <param name="bubble">The world-space bubble to display text on.</param>
        /// <param name="speaker">The NPCController initiating the dialogue (may be null).</param>
        public void StartDialogue(DialogueSequence sequence, DialogueBubble bubble,
                                  NPCController speaker = null)
        {
            if (sequence == null || bubble == null) return;

            StopAmbientIdle();
            StopActiveDialogue();

            _currentSequence = sequence;
            _currentBubble = bubble;
            _currentSpeaker = speaker;
            _resolvedLines = sequence.ResolveLines();
            IsDialogueActive = true;

            OnDialogueStarted?.Invoke();
            _sequenceCoroutine = StartCoroutine(PlaySequenceCoroutine());
        }

        /// <summary> 
        /// Called by player input to complete the current typed line,
        /// or to advance to the next line once the current one is finished.
        /// Respects the skip-lock window.
        /// </summary>
        public void RequestSkip()
        {
            if (!IsDialogueActive || _skipLocked) return;
            _skipRequested = true;
        }

        /// <summary>Immediately stops any active dialogue and hides the bubble.</summary>
        public void StopActiveDialogue()
        {
            StopAmbientIdle();
            if (_sequenceCoroutine != null)
            {
                StopCoroutine(_sequenceCoroutine);
                _sequenceCoroutine = null;
            }
            Cleanup();
        }
        #endregion

        #region Public Query API

        /// <summary>
        /// Returns true if the sequence with the given ID is the one currently playing.
        /// Safe to call every frame from other scripts.
        /// </summary>
        public bool IsSequencePlaying(string sequenceID)
            => IsDialogueActive && _currentSequence != null
               && _currentSequence.sequenceID == sequenceID;

        /// <summary>
        /// Returns true if the given NPCController is the one currently speaking.
        /// Use this to drive animations, UI highlights, etc. from other scripts.
        /// </summary>
        public bool IsSpeaking(NPCController npc)
            => IsDialogueActive && _currentSpeaker == npc;

        /// <summary>
        /// The NPCController whose dialogue is currently active, or null if none.
        /// </summary>
        public NPCController CurrentSpeaker => _currentSpeaker;

        /// <summary>
        /// The sequence currently playing, or null if none.
        /// Read the sequenceID from this to identify what is playing.
        /// </summary>
        public DialogueSequence CurrentSequence => _currentSequence;

        #endregion

        #region Sequence Coroutine

        private IEnumerator PlaySequenceCoroutine()
        {
            _currentBubble.Show();

            for (int i = 0; i < _resolvedLines.Length; i++)
            {
                if (_resolvedLines[i] == null) continue; // safety: empty group
                yield return StartCoroutine(PlayLineCoroutine(_resolvedLines[i]));
            }

            if (_currentSequence.oneShot)
                FlagStateManager.Set(_currentSequence.sequenceID);

            OnSequenceCompleted?.Invoke(_currentSequence.sequenceID);
            Cleanup();
        }

        private IEnumerator PlayLineCoroutine(DialogueLine line)
        {
            _typingComplete = false;
            _awaitingAdvance = false;
            _skipRequested = false;
            _skipLocked = true;

            // Play voice line if assigned
            if (line.voiceClip != null)
            {
                _audioSource.clip = line.voiceClip;
                _audioSource.Play();
            }

            // Begin typewriter reveal
            Coroutine typingCoroutine = StartCoroutine(TypewriterCoroutine(line));

            // --- PHASE 1: Skip lock (player cannot interact) ---
            yield return new WaitForSeconds(_skipLockDuration);
            _skipLocked = false;

            // --- PHASE 2: Wait for typing to complete (skip completes it instantly) ---
            while (!_typingComplete)
            {
                if (_skipRequested)
                {
                    StopCoroutine(typingCoroutine);
                    _currentBubble.ShowFullText(line.text);                // TMP renders tags
                    _currentBubble.SetVisibleCount(StripRichText(line.text).Length); // correct count
                    _typingComplete = true;
                    _skipRequested = false;
                }
                yield return null;
            }

            // --- PHASE 3: Post-line delay (skip re-locked briefly) ---
            _skipLocked = true;
            _audioSource.Stop();
            yield return new WaitForSeconds(line.postLineDelay);
            _skipLocked = false;

            // --- PHASE 4: Wait for player to advance ---
            _awaitingAdvance = true;
            _skipRequested = false;
            yield return new WaitUntil(() => _skipRequested);
        }

        #endregion



        #region Typewriter Coroutine

        /// <summary>
        /// Typewriter reveal for a line during active (player-driven) dialogue.
        /// Sets _typingComplete when all visible characters have been shown.
        /// Uses <see cref="StripRichText"/> so the visible count excludes tag characters.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueLine line)
        {
            string displayText = line.text;
            string strippedText = StripRichText(line.text);
            int length = strippedText.Length;

            _currentBubble.SetText(displayText, 0);

            float delay = 1f / Mathf.Max(
                line.typingSpeedOverride > 0f ? line.typingSpeedOverride : _defaultTypingSpeed, 1f);

            for (int i = 0; i <= length; i++)
            {
                _currentBubble.SetVisibleCount(i);
                yield return new WaitForSeconds(delay);
            }

            _typingComplete = true;
        }

        #endregion

        #region Ambient Idle

        /// <summary>
        /// Starts a looping ambient idle coroutine for an NPC that randomly picks lines
        /// from <paramref name="pool"/> and displays them on <paramref name="bubble"/>
        /// with a random delay between <paramref name="minDelay"/> and <paramref name="maxDelay"/>
        /// seconds. The coroutine stops automatically when real dialogue begins.
        /// Call this from NPCController when the player is nearby but not interacting.
        /// </summary>
        /// <param name="pool">Array of lines to draw from at random.</param>
        /// <param name="bubble">The bubble to display on.</param>
        /// <param name="minDelay">Minimum seconds between idle lines.</param>
        /// <param name="maxDelay">Maximum seconds between idle lines.</param>
        public void StartAmbientIdle(DialogueLine[] pool, DialogueBubble bubble,
                                      float minDelay = 6f, float maxDelay = 14f)
        {
            if (pool == null || pool.Length == 0 || bubble == null) return;

            StopAmbientIdle();
            _ambientCoroutine = StartCoroutine(AmbientIdleCoroutine(pool, bubble, minDelay, maxDelay));
        }

        /// <summary>Stops the ambient idle coroutine and hides the bubble if it is showing.</summary>
        public void StopAmbientIdle()
        {
            if (_ambientCoroutine == null) return;
            StopCoroutine(_ambientCoroutine);
            _ambientCoroutine = null;
        }

        private IEnumerator AmbientIdleCoroutine(DialogueLine[] pool, DialogueBubble bubble,
                                                  float minDelay, float maxDelay)
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

                // Don't interrupt real dialogue
                if (IsDialogueActive) continue;

                DialogueLine line = pool[Random.Range(0, pool.Length)];
                if (line == null) continue;

                bubble.Show();

                // Type out the line without player interaction — it auto-dismisses
                yield return StartCoroutine(TypewriterCoroutine(line, bubble));

                yield return new WaitForSeconds(line.postLineDelay + 1.2f);
                bubble.Hide();
            }
        }

        /// <summary>
        /// Shared typewriter that writes to a specified bubble rather than _currentBubble.
        /// Used by ambient idle so it does not interfere with the main dialogue state.
        /// </summary>
        private IEnumerator TypewriterCoroutine(DialogueLine line, DialogueBubble bubble)
        {
            if (line.voiceClip != null) _audioSource.PlayOneShot(line.voiceClip);

            // Strip rich-text tags to get the true visible character count
            // but pass the original text to TMP so tags render correctly
            string displayText = line.text;
            string strippedText = StripRichText(line.text);
            int length = strippedText.Length;

            bubble.SetText(displayText, 0);

            float delay = 1f / Mathf.Max(
                line.typingSpeedOverride > 0f ? line.typingSpeedOverride : _defaultTypingSpeed, 1f);

            for (int i = 0; i <= length; i++)
            {
                bubble.SetVisibleCount(i);
                yield return new WaitForSeconds(delay);
            }
        }

        #endregion

        #region Helpers

        private void Cleanup()
        {
            IsDialogueActive = false;
            _skipLocked = false;
            _currentSpeaker = null;
            _audioSource.Stop();

            _currentBubble?.Hide();
            _currentBubble = null;
            _currentSequence = null;
            _resolvedLines = null;

            OnDialogueEnded?.Invoke();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Removes all Unity/TMP rich-text tags from a string so that character counts
        /// used by the typewriter reflect only visible glyphs.
        /// Example: "&lt;color=green&gt;Hello&lt;/color&gt;" → "Hello" (length 5, not 22).
        /// </summary>
        /// <param name="text">Raw text potentially containing rich-text tags.</param>
        /// <returns>Plain string with all tags removed.</returns>
        public static string StripRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return _richTagRegex.Replace(text, string.Empty);
        }

        #endregion
    }
}