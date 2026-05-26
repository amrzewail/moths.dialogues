using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueSequence : ISerializable
    {
        [SerializeField] string _guid;
        [SerializeField] string _outputGuid;
        [SerializeField] string _tag;
        [SerializeField] List<DialogueLine> _lines = new();

        public string Guid => _guid;
        public string OutputGuid => _outputGuid;
        public string Tag => _tag;
        public IReadOnlyList<DialogueLine> Lines => _lines;

        public DialogueSequence()
        {
            _guid = System.Guid.NewGuid().ToString();
            _outputGuid = System.Guid.NewGuid().ToString();
        }

        public DialogueSequence(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueSequence>(serializationData);
            _tag = instance.Tag;
            _lines = new List<DialogueLine>(instance._lines);
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        public bool TryGetLine(int index, out DialogueLine line)
        {
            line = default;
            if (index >= 0 && index < _lines.Count)
            {
                line = Lines[index];
                return true;
            }
            return false;
        }

        public bool IsLastLine(int lineIndex)
        {
            return lineIndex == Lines.Count - 1;
        }

        public bool IsSequenceComplete(int lineIndex)
        {
            return lineIndex >= Lines.Count;
        }

        public void AddLine(DialogueLine line) => _lines.Add(line);
        public void RemoveLine(int index) => _lines.RemoveAt(index);
    }
}