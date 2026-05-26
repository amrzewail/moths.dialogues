using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueSpeaker
    {
        [SerializeField] string _guid;
        [SerializeField] String _name;

        public DialogueSpeaker(string guid)
        {
            this = default;
            _guid = guid;
        }

        public string Guid => _guid;
        public String Name => _name;
    }
}