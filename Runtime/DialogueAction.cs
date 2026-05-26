using Cysharp.Threading.Tasks;
using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    public interface IDialogueAction
    {
        string Description { get; }
        UniTask Execute();
    }

    [System.Serializable]
    public class DialogueAction
    {
        [SerializeField] string _guid;
        [SerializeField] string _outputGuid;
        [SerializeField] string _tag;

        [SerializeField] InterfaceReference<IDialogueAction> _action;

        public string Description => _action ? _action.Value.Description : "No Action";
        public string Guid => _guid;
        public string OutputGuid => _outputGuid;
        public string Tag => _tag;
        public IDialogueAction Action => _action.Value;

        public DialogueAction()
        {
            _guid = System.Guid.NewGuid().ToString();
            _outputGuid = System.Guid.NewGuid().ToString();
        }

        public async UniTask Execute()
        {
            if (Action == null) return;
            await Action.Execute();
        }
    }
}