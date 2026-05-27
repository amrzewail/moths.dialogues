using Moths.Graphs.Editor;
using Moths.Serialization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueActionNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueAction _action;

        public DialogueActionNode(Dialogue dialogue, Node node, DialogueAction action) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _action = action;

            GUID = action.Guid;
            position = node.position;

            UpdateTexts();
        }

        private void UpdateTexts()
        {
            string name = "No Action";
            if (_action.Action != null)
            {
                name = _action.Action.GetType().Name;
                var attr = _action.Action.GetType().GetCustomAttribute<InterfaceReferenceAttribute>();
                if (attr != null && attr.path != null && attr.path.Length > 0) name = attr.path.Split('/').Last();
            }
            title = string.IsNullOrEmpty(_action.Tag) ? name : $"{name} ({_action.Tag})";

            extensionContainer.Clear();
            extensionContainer.Add(new Label(_action.Description));
        }

        public string InspectorTitle => "Action";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _action.Guid;
            inputContainer.Add(entryPort);

            var exitPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
            exitPort.portName = "Next";
            exitPort.viewDataKey = _action.OutputGuid;
            outputContainer.Add(exitPort);
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var actionsProp = serializedObject.FindProperty("_actions");
            SerializedProperty actionProp = null;
            for (int i = 0; i < actionsProp.arraySize; i++)
            {
                if (actionsProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _action.Guid)
                {
                    actionProp = actionsProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (actionProp != null)
            {
                var tagField = new PropertyField(actionProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTexts());
                inspector.Add(tagField);

                var actionInstanceProp = actionProp.FindPropertyRelative("_action");

                var actionField = new PropertyField(actionInstanceProp, "▼");
                actionField.Bind(serializedObject);

                inspector.Add(actionField);

                actionField.RegisterValueChangeCallback(change => UpdateTexts());
            }


            return inspector;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _node.position = newPos.position;
            _node.Update();
        }

        public string Serialize()
        {
            return _action.Serialize();
        }
    }
}
