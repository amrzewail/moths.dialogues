using Cysharp.Threading.Tasks;
using Moths.Serialization;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    [System.Serializable]
    [InterfaceReference("Debug/Log")]
    public class LogAction : IDialogueAction
    {
        public string Description => $"Log \"{_text}\"";

        [SerializeField] string _text;
        public async UniTask Execute()
        {
            Debug.Log(_text);
        }
    }
}