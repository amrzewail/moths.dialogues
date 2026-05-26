using UnityEngine;
using Moths.Collections;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueLine
    {
        [SerializeField, HideInInspector] Dialogue _dialogue;

        [SerializeField] string _speakerGuid;
        [SerializeField] LString _line;

        public DialogueLine(Dialogue dialogueReference)
        {
            this = default;
            _dialogue = dialogueReference;
        }

        public LString Line => _line;
        public LString Speaker => _dialogue == null ? new LString(string.Empty) : _dialogue.GetSpeakerByGuid(_speakerGuid);
    }
}