using Moths.Graphs.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueOutputNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueOutput _output;

        public DialogueOutputNode(Dialogue dialogue, Node node, DialogueOutput output) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _output = output;

            GUID = output.Guid;
            UpdateTitle();
            position = node.position;
        }

        public string Serialize()
        {
            return _output.Serialize();
        }

        private void UpdateTitle()
        {
            string baseTitle = string.IsNullOrEmpty(_output.Name) ? "Output" : _output.Name;
            title = string.IsNullOrEmpty(_output.Tag) ? baseTitle : $"{baseTitle} ({_output.Tag})";
        }

        public string InspectorTitle => "Output";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _output.Guid;
            inputContainer.Add(entryPort);
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var outputsProp = serializedObject.FindProperty("_outputs");
            SerializedProperty outputProp = null;
            for (int i = 0; i < outputsProp.arraySize; i++)
            {
                if (outputsProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _output.Guid)
                {
                    outputProp = outputsProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (outputProp != null)
            {
                var tagField = new PropertyField(outputProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTitle());
                inspector.Add(tagField);

                var nameField = new PropertyField(outputProp.FindPropertyRelative("_name"));
                nameField.Bind(serializedObject);
                inspector.Add(nameField);

                nameField.RegisterValueChangeCallback(change =>
                {
                    UpdateTitle();
                });
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
