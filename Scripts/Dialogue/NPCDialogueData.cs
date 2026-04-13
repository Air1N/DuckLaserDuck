using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>
    /// Describes one entry inside a random sequence pool,
    /// with an optional relative weight to bias the random draw.
    /// </summary>
    [System.Serializable]
    public class WeightedSequence
    {
        public DialogueSequence sequence;

        [Tooltip("Relative probability weight. Higher = more likely to be chosen " +
                 "compared to other entries in the same pool.")]
        [Min(1)]
        public int weight = 1;
    }

    /// <summary>
    /// Holds all dialogue sequences for a single NPC.
    ///
    /// <para>
    /// <b>How sequence selection works:</b><br/>
    /// When the player enters the proximity zone or interacts, the NPC first scans
    /// the <see cref="orderedSequences"/> list top-to-bottom and plays the first entry
    /// that qualifies (correct trigger type, prerequisites met, not already completed
    /// if one-shot). If no ordered sequence qualifies, one entry is drawn at random
    /// from the matching ambient pool.
    /// </para>
    ///
    /// <para>
    /// <b>Typical authoring pattern:</b><br/>
    /// Put story-critical or first-time dialogue in <see cref="orderedSequences"/> as
    /// one-shots. Once those are all played out, the NPC automatically falls through
    /// to the ambient pool — so a shopkeeper who greeted you properly on your first
    /// visit will randomly say "See anything you like?" or "Best prices in town!"
    /// on every visit after. You can also leave <see cref="orderedSequences"/> empty
    /// for a trigger type entirely and rely solely on the ambient pool from the start.
    /// </para>
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewNPCDialogueData",
        menuName = "Game/Dialogue/NPC Dialogue Data")]
    public class NPCDialogueData : ScriptableObject
    {
        [Tooltip("Display name used in debug output.")]
        public string npcName;

        [Header("Ordered Sequences")]
        [Tooltip("Evaluated top-to-bottom for both trigger types. " +
                 "The first entry whose trigger type matches, prerequisites are met, " +
                 "and (if one-shot) has not already been completed will play. " +
                 "Typical use: story dialogue, first-meeting lines, quest hand-ins — " +
                 "anything that must happen in a specific order or only once.")]
        public DialogueSequence[] orderedSequences;

        [Header("Ambient Random Pool — Proximity")]
        [Tooltip("Used when NO ordered sequence qualifies for a proximity trigger. " +
                 "One entry is chosen by weighted random draw each time. " +
                 "Typical use: idle chatter, 'hey come over here' lines, " +
                 "flavour calls that play every time the player walks past " +
                 "once the one-time greeting sequence is done.")]
        public WeightedSequence[] ambientProximityPool;

        [Header("Ambient Random Pool — Interaction")]
        [Tooltip("Used when NO ordered sequence qualifies for an interaction trigger. " +
                 "One entry is chosen by weighted random draw each time. " +
                 "Typical use: repeat-visit shop lines, generic responses " +
                 "once all story sequences have been completed.")]
        public WeightedSequence[] ambientInteractionPool;
    }
}