using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueSwitch : ISerializable
    {
        [System.Serializable]
        public class SwitchCase
        {
            [SerializeField] private InterfaceReference<IDialogueCondition> _condition = new InterfaceReference<IDialogueCondition>();
            [SerializeField] private string _outputGuid;

            public InterfaceReference<IDialogueCondition> Condition => _condition;
            public string OutputGuid => _outputGuid;

            public SwitchCase(string guid = null)
            {
                _outputGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            }

            public void SetGuid(string guid)
            {
                _outputGuid = guid;
            }
        }


        [SerializeField] private string _guid;
        [SerializeField] private string _tag;
        [SerializeField] private int _count;
        [SerializeField] private List<SwitchCase> _cases = new List<SwitchCase>();
        [SerializeField] private string _defaultOutputGuid;

        public string Guid => _guid;
        public string Tag { get => _tag; set => _tag = value; }
        public int Count => _count;
        public IReadOnlyList<SwitchCase> Cases => _cases;
        public string DefaultOutputGuid => _defaultOutputGuid;

        public DialogueSwitch()
        {
            _guid = System.Guid.NewGuid().ToString();
            _defaultOutputGuid = System.Guid.NewGuid().ToString();
            _count = 2;
            AdjustCases(_count);
        }

        public DialogueSwitch(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueSwitch>(serializationData);
            _tag = instance.Tag;
            _count = instance.Count;
            _cases = new List<SwitchCase>();
            if (instance._cases != null)
            {
                foreach (var c in instance._cases)
                {
                    var newCase = new SwitchCase();
                    newCase.Condition.Copy(c.Condition);
                    _cases.Add(newCase);
                }
            }
            AdjustCases(_count);
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        public void AdjustCases(int count)
        {
            _count = count;
            while (_cases.Count < _count)
            {
                _cases.Add(new SwitchCase(System.Guid.NewGuid().ToString()));
            }
            if (_cases.Count > _count)
            {
                _cases.RemoveRange(_count, _cases.Count - _count);
            }
            if (string.IsNullOrEmpty(_defaultOutputGuid))
            {
                _defaultOutputGuid = System.Guid.NewGuid().ToString();
            }
        }

        public string Evaluate()
        {
            if (_cases != null)
            {
                for (int i = 0; i < _cases.Count; i++)
                {
                    var c = _cases[i];
                    if (c.Condition != null && c.Condition.Value != null)
                    {
                        if (c.Condition.Value.Check())
                        {
                            return c.OutputGuid;
                        }
                    }
                }
            }
            return _defaultOutputGuid;
        }
    }
}
