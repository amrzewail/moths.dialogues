using Moths.Graphs.Editor;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueNestedNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueNested _nested;
        public event Action PortsUpdated;

        public DialogueNestedNode(Dialogue dialogue, Node node, DialogueNested nested) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _nested = nested;

            GUID = nested.Guid;
            position = node.position;

            UpdateTexts();
        }

        public string Serialize() => _nested.Serialize();

        private void UpdateTexts()
        {
            title = string.IsNullOrEmpty(_nested.Tag) ? "Dialogue" : $"Dialogue ({_nested.Tag})";

            extensionContainer.Clear();
            extensionContainer.Add(new Label($"{(_nested.Dialogue ? _nested.Dialogue.name : "<No Dialogue>")}"));
        }

        public string InspectorTitle => "Dialogue";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _nested.Guid;
            inputContainer.Add(entryPort);

            _nested.UpdateOutputs();

            if (_nested.Dialogue)
            {
                for (int i = 0; i < _nested.Dialogue.Outputs.Count; i++)
                {
                    var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
                    p.portName = $"{_nested.Dialogue.Outputs[i].Name}";
                    p.viewDataKey = _nested.GetPortOutput(_nested.Dialogue.Outputs[i].Guid);
                    outputContainer.Add(p);
                }
            }

            base.RefreshPorts();

            PortsUpdated?.Invoke();
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var dialoguesProp = serializedObject.FindProperty("_dialogues");
            SerializedProperty dialogueProp = null;
            for (int i = 0; i < dialoguesProp.arraySize; i++)
            {
                if (dialoguesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _nested.Guid)
                {
                    dialogueProp = dialoguesProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (dialogueProp != null)
            {
                var tagField = new PropertyField(dialogueProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => { UpdateTexts(); });
                inspector.Add(tagField);

                var assetProp = dialogueProp.FindPropertyRelative("_dialogue");
                var assetField = new PropertyField(assetProp, "Dialogue");
                assetField.Bind(serializedObject);
                assetField.RegisterValueChangeCallback(evt =>
                {
                    UpdateTexts();
                    GeneratePorts();
                });
                assetField.TrackPropertyValue(dialogueProp, evt =>
                {
                    UpdateTexts();
                    GeneratePorts();
                });
                inspector.Add(assetField);
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
