using Moths.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues
{
    [CreateAssetMenu(menuName = "Moths/Dialogues/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        [SerializeField] string _startingGuid;
        [SerializeField] List<DialogueSpeaker> _speakers = new();
        [SerializeField] List<DialogueSequence> _sequences = new();
        [SerializeField] List<DialogueChoices> _choices = new();
        [SerializeField] List<DialogueAction> _actions = new();
        [SerializeField] List<DialogueOutput> _outputs = new();

        [SerializeField] SerializableDictionary<string, string> _connections = new();

        public string StartingGuid { get => _startingGuid; set => _startingGuid = value; }
        public IReadOnlyList<DialogueSpeaker> Speakers => _speakers;
        public IReadOnlyList<DialogueSequence> Sequences => _sequences;
        public IReadOnlyList<DialogueChoices> Choices => _choices;
        public IReadOnlyList<DialogueAction> Actions => _actions;
        public IReadOnlyList<DialogueOutput> Outputs => _outputs;
        public SerializableDictionary<string, string> Connections => _connections;

        public DialogueElement Start()
        {
            return GetElementByGuid(_startingGuid);
        }

        public DialogueElement Next(DialogueElement element)
        {
            return Next(element.OutputGuid);
        }

        public DialogueElement Next(string guid)
        {
            if (_connections.TryGetValue(guid, out var nextGuid))
            {
                return GetElementByGuid(nextGuid);
            }
            return default;
        }

        public DialogueElement GetElementByGuid(string guid)
        {
            for (int i = 0; i < _sequences.Count; i++)
            {
                if (_sequences[i].Guid == guid) return _sequences[i];
            }
            for (int i = 0; i < _choices.Count; i++)
            {
                if (_choices[i].Guid == guid) return _choices[i];
            }
            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i].Guid == guid) return _actions[i];
            }
            for (int i = 0; i < _outputs.Count; i++)
            {
                if (_outputs[i].Guid == guid) return _outputs[i];
            }
            return default;
        }

        public String GetSpeakerByGuid(string guid)
        {
            for (int i = 0; i < _speakers.Count; i++)
            {
                if (_speakers[i].Guid == guid) return _speakers[i].Name;
            }
            return default;
        }

        public void AddSequence(DialogueSequence sequence) => _sequences.Add(sequence);
        public void RemoveSequence(string guid) => _sequences.RemoveAll(s => s.Guid == guid);

        public void AddChoices(DialogueChoices choices) => _choices.Add(choices);
        public void RemoveChoices(string guid) => _choices.RemoveAll(c => c.Guid == guid);

        public void AddAction(DialogueAction action) => _actions.Add(action);
        public void RemoveAction(string guid) => _actions.RemoveAll(a => a.Guid == guid);

        public void AddOutput(DialogueOutput output) => _outputs.Add(output);
        public void RemoveOutput(string guid) => _outputs.RemoveAll(o => o.Guid == guid);

        public void AddSpeaker(DialogueSpeaker speaker) => _speakers.Add(speaker);
        public void RemoveSpeaker(string guid) => _speakers.RemoveAll(s => s.Guid == guid);
    }
}