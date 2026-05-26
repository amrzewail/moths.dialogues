using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Moths.Dialogues.Actions
{
    public class WaitAction : DialogueActionBase
    {
        public override string Description => $"Wait for {_time} seconds";

        [SerializeField] float _time;
        public override async UniTask Execute()
        {
            await UniTask.WaitForSeconds(_time);
        }
    }
}