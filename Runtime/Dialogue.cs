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
        [SerializeField] List<DialogueCondition> _conditions = new();
        [SerializeField] List<DialogueJump> _jumps = new();
        [SerializeField] List<DialogueRandom> _randoms = new();
        [SerializeField] List<DialogueSwitch> _switches = new();
        [SerializeField] List<DialogueNested> _dialogues = new();

        [SerializeField] SerializableDictionary<string, string> _connections = new();

        public string StartingGuid { get => _startingGuid; set => _startingGuid = value; }
        public IReadOnlyList<DialogueSpeaker> Speakers => _speakers;
        public IReadOnlyList<DialogueSequence> Sequences => _sequences;
        public IReadOnlyList<DialogueChoices> Choices => _choices;
        public IReadOnlyList<DialogueAction> Actions => _actions;
        public IReadOnlyList<DialogueOutput> Outputs => _outputs;
        public IReadOnlyList<DialogueCondition> Conditions => _conditions;
        public IReadOnlyList<DialogueJump> Jumps => _jumps;
        public IReadOnlyList<DialogueRandom> Randoms => _randoms;
        public IReadOnlyList<DialogueSwitch> Switches => _switches;
        public IReadOnlyList<DialogueNested> Dialogues => _dialogues;
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
            return GetElementByGuid(guid);
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
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].Guid == guid) return _conditions[i];
            }
            for (int i = 0; i < _jumps.Count; i++)
            {
                if (_jumps[i].Guid == guid) return _jumps[i];
            }
            for (int i = 0; i < _randoms.Count; i++)
            {
                if (_randoms[i].Guid == guid) return _randoms[i];
            }
            for (int i = 0; i < _switches.Count; i++)
            {
                if (_switches[i].Guid == guid) return _switches[i];
            }
            for (int i = 0; i < _dialogues.Count; i++)
            {
                if (_dialogues[i].Guid == guid) return _dialogues[i];
            }
            return default;
        }

        public DialogueSpeaker GetSpeakerByGuid(string guid)
        {
            for (int i = 0; i < _speakers.Count; i++)
            {
                if (_speakers[i].Guid == guid) return _speakers[i];
            }
            return default;
        }

        public void SortOutputs(Comparison<DialogueOutput> comparison)
        {
            _outputs.Sort(comparison);
        }

        public void AddSequence(DialogueSequence sequence) => _sequences.Add(sequence);
        public void RemoveSequence(string guid) => _sequences.RemoveAll(s => s.Guid == guid);

        public void AddChoices(DialogueChoices choices) => _choices.Add(choices);
        public void RemoveChoices(string guid) => _choices.RemoveAll(c => c.Guid == guid);

        public void AddAction(DialogueAction action) => _actions.Add(action);
        public void RemoveAction(string guid) => _actions.RemoveAll(a => a.Guid == guid);

        public void AddOutput(DialogueOutput output) => _outputs.Add(output);
        public void RemoveOutput(string guid) => _outputs.RemoveAll(o => o.Guid == guid);

        public void AddCondition(DialogueCondition condition) => _conditions.Add(condition);
        public void RemoveCondition(string guid) => _conditions.RemoveAll(c => c.Guid == guid);

        public void AddJump(DialogueJump jump) => _jumps.Add(jump);
        public void RemoveJump(string guid) => _jumps.RemoveAll(j => j.Guid == guid);

        public void AddRandom(DialogueRandom random) => _randoms.Add(random);
        public void RemoveRandom(string guid) => _randoms.RemoveAll(r => r.Guid == guid);

        public void AddSwitch(DialogueSwitch switchData) => _switches.Add(switchData);
        public void RemoveSwitch(string guid) => _switches.RemoveAll(s => s.Guid == guid);

        public void AddDialogue(DialogueNested dialogue) => _dialogues.Add(dialogue);
        public void RemoveDialogue(string guid) => _dialogues.RemoveAll(d => d.Guid == guid);

        public void AddSpeaker(DialogueSpeaker speaker) => _speakers.Add(speaker);
        public void RemoveSpeaker(string guid) => _speakers.RemoveAll(s => s.Guid == guid);
    }
}