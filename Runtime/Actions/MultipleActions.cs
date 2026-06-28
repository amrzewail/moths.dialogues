using Cysharp.Threading.Tasks;
using Moths.Serialization;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    [System.Serializable]
    [InterfaceReference("Core/Multiple Actions")]
    public class MultipleActions : IDialogueAction
    {
        [SerializeField] InterfaceReference<IDialogueAction>[] _actions;

        public string Description
        {
            get
            {
                string name = string.Empty;
                for (int i = 0; i < _actions.Length; i++)
                {
                    if (_actions[i].Value == null) continue;
                    name += _actions[i].Value.Description;
                    if (i < _actions.Length - 1) name += "\n\n";
                }
                return name;
            }
        }

        public async UniTask Execute()
        {
            for (int i = 0; i < _actions.Length; i++) await _actions[i].Value.Execute();
        }
    }
}