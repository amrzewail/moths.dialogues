using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueJump
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

#if UNITY_EDITOR
        public void SetTarget(string tag, string guid)
        {
            _targetTag = tag;
            _outputGuid = guid;
        }
#endif
    }
}