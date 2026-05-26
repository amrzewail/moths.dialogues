using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueLine
    {
        [SerializeField, HideInInspector] Dialogue _dialogue;

        [SerializeField] string _speakerGuid;
        [SerializeField] String _line;

        public DialogueLine(Dialogue dialogueReference)
        {
            this = default;
            _dialogue = dialogueReference;
        }

        public String Line => _line;
        public String Speaker => _dialogue == null ? new String(string.Empty) : _dialogue.GetSpeakerByGuid(_speakerGuid);
    }
}