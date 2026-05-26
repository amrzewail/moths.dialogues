using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    public class LogAction : DialogueActionBase
    {
        public override string Description => $"Log \"{_text}\"";

        [SerializeField] string _text;
        public override async UniTask Execute()
        {
            Debug.Log(_text);
        }
    }
}