using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moths.Collections;

namespace Moths.Dialogues
{
    [System.Serializable]
    public struct DialogueChoice
    {
        [SerializeField] string _guid;
        [SerializeField] LString _line;

        public DialogueChoice(string guid)
        {
            this = default;
            _guid = guid;
        }

        public string Guid => _guid;
        public LString Line => _line;
    }

    [System.Serializable]
    public class DialogueChoices
    {
        [SerializeField] string _guid;
        [SerializeField] string _tag;
        [SerializeField] List<DialogueChoice> _choices = new();

        public DialogueChoices()
        {
            _guid = System.Guid.NewGuid().ToString();
        }

        public string Guid => _guid;
        public string Tag => _tag;
        public IReadOnlyList<DialogueChoice> Choices => _choices;

        public void AddChoice(DialogueChoice choice) => _choices.Add(choice);
        public void RemoveChoice(string guid) => _choices.RemoveAll(c => c.Guid == guid);

#if UNITY_EDITOR
        public IList GetChoicesList() => _choices;
#endif
    }
}