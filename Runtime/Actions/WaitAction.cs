using Cysharp.Threading.Tasks;
using Moths.Serialization;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    [System.Serializable]
    [InterfaceReference("Core/Wait Action")]
    public class WaitAction : IDialogueAction
    {
        public string Description => $"Wait for <b>{_time}</b> seconds";

        [SerializeField] float _time;
        public async UniTask Execute()
        {
            await UniTask.WaitForSeconds(_time);
        }
    }
}