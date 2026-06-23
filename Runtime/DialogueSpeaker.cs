using UnityEngine;
using Moths.Collections;
using Moths.Serialization;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueSpeaker
    {
        [SerializeField] string _guid;
        [SerializeField] LString _name;
        [SerializeField] InterfaceReference<IDialogueSpeakerData> _data;

        public DialogueSpeaker(string guid)
        {
            this = default;
            _guid = guid;
        }

        public string Guid => _guid;
        public LString Name => _name;
        public IDialogueSpeakerData Data => _data.Value;
    }

    public interface IDialogueSpeakerData { }
}