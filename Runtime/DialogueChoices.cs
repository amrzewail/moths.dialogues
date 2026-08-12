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

    public interface IDialogueChoiceData
    {

    }

    [System.Serializable]
    public struct DialogueChoice
    {
        [SerializeField] string _guid;
        [SerializeField] LString _line;
        [SerializeField] InterfaceReference<IDialogueChoiceData> _data;

        public DialogueChoice(string guid, DialogueChoice copy)
        {
            _guid = guid;
            _line = copy._line;
            _data.Copy(copy._data);
        }

        public DialogueChoice(string guid)
        {
            this = default;
            _guid = guid;
        }

        public DialogueChoice(string guid, LString line)
        {
            this = default;
            _guid = guid;
            _line = line;
        }

        public string Guid => _guid;
        public LString Line => _line;

        public bool TryGetData<T>(out T data) where T : IDialogueChoiceData
        {
            data = default;
            if (_data)
            {
                data = (T)_data.Value;
                return true;
            }
            return false;
        }

    }

    [System.Serializable]
    public class DialogueChoices : ISerializable
    {
        [SerializeField] string _guid;
        [SerializeField] string _tag;
        [SerializeField] InterfaceReference<IProceduralDialogueChoices> _proceduralChoices;
        [SerializeField] List<DialogueChoice> _choices = new();
        [SerializeField] string _proceduralGuid;

        public string Guid => _guid;
        public string Tag => _tag;
        public bool IsProcedural => _proceduralChoices && _proceduralChoices.Value != null;
        public string ProceduralGuid => _proceduralGuid;
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
            _proceduralGuid = System.Guid.NewGuid().ToString();
        }

        public DialogueChoices(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueChoices>(serializationData);
            _tag = instance.Tag;
            _proceduralChoices.Copy(instance._proceduralChoices);
            _choices = new();
            foreach (var choice in instance._choices)
            {
                _choices.Add(new(System.Guid.NewGuid().ToString(), choice));
            }
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }
    }
}