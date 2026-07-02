using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueRandom : ISerializable
    {
        [SerializeField] string _guid;
        [SerializeField] string _tag;
        [SerializeField] int _outputCount = 2;
        [SerializeField] List<string> _outputs = new();

        public string Guid => _guid;
        public string Tag => _tag;
        public int OutputCount => _outputCount;
        public IReadOnlyList<string> Outputs => _outputs;

        public DialogueRandom()
        {
            _guid = System.Guid.NewGuid().ToString();
            AdjustOutputs();
        }

        public DialogueRandom(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueRandom>(serializationData);
            _tag = instance.Tag;
            _outputCount = instance._outputCount;
            _outputs = instance._outputs != null ? new List<string>(instance._outputs) : new List<string>();
            AdjustOutputs();
        }

        public void AdjustOutputs()
        {
            if (_outputs == null) _outputs = new List<string>();
            if (_outputCount < 0) _outputCount = 0;
            while (_outputs.Count < _outputCount)
            {
                _outputs.Add(System.Guid.NewGuid().ToString());
            }
            while (_outputs.Count > _outputCount)
            {
                _outputs.RemoveAt(_outputs.Count - 1);
            }
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        public string GetRandomOutputGuid()
        {
            if (_outputs == null || _outputs.Count == 0) return string.Empty;
            int index = UnityEngine.Random.Range(0, _outputs.Count);
            return _outputs[index];
        }
    }
}
