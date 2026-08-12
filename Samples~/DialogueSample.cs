using Moths.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Dialogues.Samples
{
    [System.Serializable]
    public class SampleProceduralChoices : IProceduralDialogueChoices
    {
        public IReadOnlyList<DialogueChoice> GetChoices()
        {
            return new List<DialogueChoice>()
            {
                new("1", new LString("hello")),
                new("2", new LString("hello 2")),
            };
        }

        public void ProcessChoice(DialogueChoice choice)
        {

        }
    }

    public class DialogueSample : MonoBehaviour
    {
        private DialogueRunner _runner = new();

        [SerializeField] Dialogue _dialogue;

        [SerializeField] SerializableDictionary<string, float> _dictionary = new();
        [SerializeField] SerializableDictionary<string, float> _dictionary2 = new();

        private void Start()
        {
            _runner.OnLine += DialogueLineCallback;
            _runner.OnChoices += DialogueChoicesCallback;
            _runner.OnAction += DialogueActionCallback;
            _runner.OnOutput += DialogueOutputCallback;

            _runner.Start(_dialogue);
        }


        private void Update()
        {
            foreach (var pair in _dictionary2)
            {
                pair.value += Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                _runner.Next();
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                _runner.Next(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _runner.Next(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _runner.Next(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _runner.Next(3);
            }
        }

        private void DialogueLineCallback(DialogueLine line)
        {
            Debug.Log($"[Dialogue] {line.Speaker}: {line.Line}");
        }

        private void DialogueChoicesCallback(IReadOnlyList<DialogueChoice> list)
        {
            Debug.Log($"[Dialogue] Choose between:");
            int index = 0;
            foreach(var choice in list)
            {
                Debug.Log($"[Dialogue] {index++}) {choice.Line}");
            }
        }

        private void DialogueActionCallback()
        {
            Debug.Log($"[Dialogue] Waiting action");
        }


        private void DialogueOutputCallback(DialogueOutput output)
        {
            Debug.Log($"[Dialogue] Ended with output: {output.Name}");
        }

    }
}