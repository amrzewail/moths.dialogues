using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueJump : ISerializable
    {
        [SerializeField] string _guid = "";
        [SerializeField] string _outputGuid = "";
        [SerializeField] string _targetTag = "";

        public string Guid => _guid;
        public string OutputGuid => _outputGuid;
        public string TargetTag => _targetTag;

        public DialogueJump()
        {
            _guid = System.Guid.NewGuid().ToString();
        }

        public DialogueJump(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueJump>(serializationData);
            _targetTag = instance.TargetTag;
            _outputGuid = instance.OutputGuid;
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

#if UNITY_EDITOR
        public void SetTarget(string tag, string guid)
        {
            _targetTag = tag;
            _outputGuid = guid;
        }
#endif
    }
}