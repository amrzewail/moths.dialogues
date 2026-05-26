using Moths.Serialization;
using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueCondition
    {
        [SerializeField] string _guid;
        [SerializeField] string _trueOutputGuid;
        [SerializeField] string _falseOutputGuid;
        [SerializeField] string _tag;

        [SerializeField] InterfaceReference<IDialogueCondition> _condition;

        public string Guid => _guid;
        public string TrueOutputGuid => _trueOutputGuid;
        public string FalseOutputGuid => _falseOutputGuid;
        public string Tag => _tag;
        public IDialogueCondition Condition => _condition.Value;
        public string Description => _condition ? _condition.Value.Description : "No Condition";

        public DialogueCondition()
        {
            _guid = System.Guid.NewGuid().ToString();
            _trueOutputGuid = System.Guid.NewGuid().ToString();
            _falseOutputGuid = System.Guid.NewGuid().ToString();
        }

        public bool Check()
        {
            if (Condition == null) return true;
            return Condition.Check();
        }
    }
}