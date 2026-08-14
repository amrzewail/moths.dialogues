using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    [System.Serializable]
    public class DialogueNested : ISerializable
    {
        [System.Serializable]
        public struct Output
        {
            public string dialogueOutput;
            public string portOutput;
        }

        [SerializeField] string _guid;
        [SerializeField] string _tag;
        [SerializeField] Dialogue _dialogue;
        [SerializeField] List<Output> _outputs;

        public string Guid => _guid;
        public string Tag => _tag;
        public Dialogue Dialogue => _dialogue;

        public DialogueNested()
        {
            _guid = System.Guid.NewGuid().ToString();
            _outputs = new();
        }

        public DialogueNested(string serializationData) : this()
        {
            var instance = JsonUtility.FromJson<DialogueNested>(serializationData);
            _tag = instance.Tag;
            _dialogue = instance.Dialogue;
            _outputs.AddRange(instance._outputs);
            for (int i = 0; i < _outputs.Count; i++) _outputs[i] = new() { dialogueOutput = _outputs[i].dialogueOutput, portOutput = System.Guid.NewGuid().ToString() };
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        public string GetPortOutput(string dialogueOutput)
        {
            foreach(var output in _outputs)
            {
                if (output.dialogueOutput == dialogueOutput) return output.portOutput;
            }
            return string.Empty;
        }

        public void UpdateOutputs()
        {
            if (!_dialogue)
            {
                _outputs.Clear();
                return;
            }
            HashSet<string> dialogueOutputs = new();
            foreach (var output in _dialogue.Outputs) dialogueOutputs.Add(output.Guid);
            for (int i = _outputs.Count - 1; i >= 0; i--)
            {
                if (!dialogueOutputs.Contains(_outputs[i].dialogueOutput))
                {
                    _outputs.RemoveAt(i);
                }
                else
                {
                    dialogueOutputs.Remove(_outputs[i].dialogueOutput);
                }
            }

            foreach (var output in dialogueOutputs) _outputs.Add(new() { dialogueOutput = output, portOutput = System.Guid.NewGuid().ToString() });
        }

        public void Next(DialogueRunner runner, int choiceIndex)
        {
            runner.Next(choiceIndex);
        }
    }
}