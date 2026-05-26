using Cysharp.Threading.Tasks;
using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public abstract class DialogueActionBase
    {
        public abstract string Description { get; }
        public abstract UniTask Execute();
    }

    [System.Serializable]
    public class DialogueAction
    {
        [SerializeField] string _guid;
        [SerializeField] string _outputGuid;

        [SerializeField] InterfaceReference<DialogueActionBase> _action;

        public string Description => _action ? _action.Value.Description : "No Action";
        public string Guid => _guid;
        public string OutputGuid => _outputGuid;
        public DialogueActionBase Action => _action;

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