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

        public DialogueChoice(string guid, LString line)
        {
            _guid = guid;
            _line = line;
        }

        public string Guid => _guid;
        public LString Line => _line;
    }

    [System.Serializable]
    public class DialogueChoices : ISerializable
    {
        [SerializeField] string _guid;
        [SerializeField] string _tag;
        [SerializeField] List<DialogueChoice> _choices = new();

        public string Guid => _guid;
        public string Tag => _tag;
        public IReadOnlyList<DialogueChoice> Choices => _choices;

        public DialogueChoices()
        {
            _guid = System.Guid.NewGuid().ToString();
        }

        public DialogueChoices(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueChoices>(serializationData);
            _tag = instance.Tag;
            foreach (var choice in instance._choices)
            {
                _choices.Add(new DialogueChoice(System.Guid.NewGuid().ToString(), choice.Line));
            }
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }
    }
}