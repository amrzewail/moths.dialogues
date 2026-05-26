using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueOutput
    {
        [SerializeField] string _guid;
        [SerializeField] string _name;
        [SerializeField] string _tag;

        public DialogueOutput()
        {
            _name = "Output";
            _guid = System.Guid.NewGuid().ToString();
        }

        public string Guid => _guid;
        public string Name => _name;
        public string Tag => _tag;
    }
}