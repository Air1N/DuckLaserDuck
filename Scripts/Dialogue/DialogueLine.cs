using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>
    /// Represents a single line of dialogue, including its text,
    /// an optional voice clip, and per-line timing overrides.
    /// </summary>
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("The text to display in the dialogue bubble.")]
        [TextArea(2, 5)]
        public string text;

        [Tooltip("Optional voice-line audio clip that plays when this line begins.")]
        public AudioClip voiceClip;

        [Tooltip("Characters per second for the typewriter effect. " +
                 "Leave at 0 to use the DialogueManager's global default.")]
        public float typingSpeedOverride = 0f;

        [Tooltip("Seconds to wait after the line is fully typed before the " +
                 "player is allowed to advance. Prevents accidental skipping.")]
        [Range(0f, 5f)]
        public float postLineDelay = 0.8f;
    }
}