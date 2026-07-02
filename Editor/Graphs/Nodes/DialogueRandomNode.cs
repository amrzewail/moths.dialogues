using Moths.Graphs.Editor;
using Moths.Serialization;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueRandomNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueRandom _random;

        public event Action PortsUpdated;

        public DialogueRandomNode(Dialogue dialogue, Node node, DialogueRandom random) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _random = random;

            GUID = random.Guid;
            position = node.position;

            UpdateTexts();
        }

        public string Serialize()
        {
            return _random.Serialize();
        }

        private void UpdateTexts()
        {
            title = string.IsNullOrEmpty(_random.Tag) ? "Random" : $"Random ({_random.Tag})";

            extensionContainer.Clear();
            extensionContainer.Add(new Label($"Outputs: {_random.OutputCount}"));
        }

        public string InspectorTitle => "Random";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _random.Guid;
            inputContainer.Add(entryPort);

            for (int i = 0; i < _random.Outputs.Count; i++)
            {
                var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
                p.portName = $"Out {i}";
                p.viewDataKey = _random.Outputs[i];
                outputContainer.Add(p);
            }
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var randomsProp = serializedObject.FindProperty("_randoms");
            SerializedProperty randomProp = null;
            for (int i = 0; i < randomsProp.arraySize; i++)
            {
                if (randomsProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _random.Guid)
                {
                    randomProp = randomsProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (randomProp != null)
            {
                var tagField = new PropertyField(randomProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTexts());
                inspector.Add(tagField);

                var countProp = randomProp.FindPropertyRelative("_outputCount");
                var countField = new PropertyField(countProp, "Output Count");
                countField.Bind(serializedObject);
                countField.RegisterValueChangeCallback(evt =>
                {
                    int count = countProp.intValue;
                    if (count < 0)
                    {
                        count = 0;
                        countProp.intValue = 0;
                    }
                    
                    var outputsProp = randomProp.FindPropertyRelative("_outputs");
                    while (outputsProp.arraySize < count)
                    {
                        outputsProp.InsertArrayElementAtIndex(outputsProp.arraySize);
                        outputsProp.GetArrayElementAtIndex(outputsProp.arraySize - 1).stringValue = System.Guid.NewGuid().ToString();
                    }
                    while (outputsProp.arraySize > count)
                    {
                        outputsProp.DeleteArrayElementAtIndex(outputsProp.arraySize - 1);
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                    _random.AdjustOutputs();
                    UpdateTexts();
                    GeneratePorts();
                    PortsUpdated?.Invoke();
                });
                inspector.Add(countField);
            }

            return inspector;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _node.position = newPos.position;
            _node.Update();
        }
    }
}
