using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues.Conditions
{
    [System.Serializable]
    [InterfaceReference("Core/Any Condition")]
    public class AnyCondition : IDialogueCondition
    {
        public string Description
        {
            get
            {
                List<string> descriptions = new();
                foreach (var condition in _conditions)
                {
                    if (condition.Value == null) continue;
                    if (condition) descriptions.Add(condition.Value.Description);
                }
                return string.Join("\n\n", descriptions);
            }
        }

        [SerializeField] List<InterfaceReference<IDialogueCondition>> _conditions = new();

        public bool Check()
        {
            foreach (var condition in _conditions)
            {
                if (condition && condition.Value.Check()) return true;
            }
            return false;
        }
    }
}