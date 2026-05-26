using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueOutput : ISerializable
    {
        [SerializeField] string _guid;
        [SerializeField] string _name;
        [SerializeField] string _tag;

        public DialogueOutput()
        {
            _name = "Output";
            _guid = System.Guid.NewGuid().ToString();
        }

        public DialogueOutput(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueOutput>(serializationData);
            _name = instance.Name;
            _tag = instance.Tag;
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        public string Guid => _guid;
        public string Name => _name;
        public string Tag => _tag;
    }
}