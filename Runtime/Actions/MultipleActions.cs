using Cysharp.Threading.Tasks;
using Moths.Serialization;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    public class MultipleActions : DialogueActionBase
    {
        [SerializeField] InterfaceReference<DialogueActionBase>[] _actions;

        public override string Description
        {
            get
            {
                string name = string.Empty;
                for (int i = 0; i < _actions.Length; i++)
                {
                    name += _actions[i].Value.Description;
                    if (i < _actions.Length - 1) name += "\n";
                }
                return name;
            }
        }

        public override async UniTask Execute()
        {
            for (int i = 0; i < _actions.Length; i++) await _actions[i].Value.Execute();
        }
    }
}