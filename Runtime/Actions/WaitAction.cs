using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    [System.Serializable]
    public class WaitAction : IDialogueAction
    {
        public string Description => $"Wait for {_time} seconds";

        [SerializeField] float _time;
        public async UniTask Execute()
        {
            await UniTask.WaitForSeconds(_time);
        }
    }
}