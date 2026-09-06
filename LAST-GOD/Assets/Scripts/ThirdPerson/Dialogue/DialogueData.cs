using System;
using UnityEngine;

namespace LastGod.ThirdPerson.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)]
        public string text;
        public float duration = 3.5f;
        public AudioClip voiceClip;
    }

    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "LastGod/Dialogue/DialogueData")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        public DialogueLine[] lines;
    }
}
