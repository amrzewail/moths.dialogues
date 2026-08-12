using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moths.Collections;
using Moths.Serialization;

namespace Moths.Dialogues
{
    public interface IProceduralDialogueChoices
    {
        IReadOnlyList<DialogueChoice> GetChoices();
        void ProcessChoice(DialogueChoice choice);
    }

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
        [SerializeField] InterfaceReference<IProceduralDialogueChoices> _proceduralChoices;
        [SerializeField] List<DialogueChoice> _choices = new();

        public string Guid => _guid;
        public string Tag => _tag;
        public bool IsProcedural => _proceduralChoices != null && _proceduralChoices.Value != null;
        public IReadOnlyList<DialogueChoice> Choices => _proceduralChoices ? _proceduralChoices.Value.GetChoices() : _choices;

        public void ProcessChoice(DialogueChoice choice)
        {
            if (IsProcedural)
            {
                _proceduralChoices.Value.ProcessChoice(choice);
            }
        }

        public DialogueChoices()
        {
            _guid = System.Guid.NewGuid().ToString();
        }

        public DialogueChoices(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueChoices>(serializationData);
            _tag = instance.Tag;
            _proceduralChoices.Copy(instance._proceduralChoices);
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