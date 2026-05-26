using UnityEngine;
using Moths.Collections;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueSpeaker
    {
        [SerializeField] string _guid;
        [SerializeField] LString _name;

        public DialogueSpeaker(string guid)
        {
            this = default;
            _guid = guid;
        }

        public string Guid => _guid;
        public LString Name => _name;
    }
}