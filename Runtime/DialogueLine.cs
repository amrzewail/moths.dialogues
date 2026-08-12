using UnityEngine;
using Moths.Collections;
using Moths.Serialization;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueLine
    {
        [SerializeField, HideInInspector] Dialogue _dialogue;

        [SerializeField] string _speakerGuid;
        [SerializeField] LString _line;
        [SerializeField] InterfaceReference<IDialogueLineData> _data;

        public DialogueLine(DialogueLine copy)
        {
            _dialogue = copy._dialogue;
            _speakerGuid = copy._speakerGuid;
            _line = copy._line;
            _data.Copy(copy._data);
        }

        public DialogueLine(Dialogue dialogueReference)
        {
            this = default;
            _dialogue = dialogueReference;
        }

        public bool TryGetData<T>(out T data) where T : IDialogueLineData
        {
            data = default;
            if (_data)
            {
                data = (T)_data.Value;
                return true;
            }
            return false;
        }

        public bool TryGetSpeakerData<T>(out T data) where T : IDialogueSpeakerData
        {
            data = default;
            var speaker = _dialogue.GetSpeakerByGuid(_speakerGuid);
            if (speaker.Data != null)
            {
                data = (T)speaker.Data;
                return true;
            }
            return false;
        }

        public LString Line => _line;
        public LString Speaker => _dialogue == null ? new LString(string.Empty) : _dialogue.GetSpeakerByGuid(_speakerGuid).Name;
    }

    public interface IDialogueLineData { }
}