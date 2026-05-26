using Moths.Graphs.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueConditionNode : BasicNode, IInspectable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueCondition _condition;

        public DialogueConditionNode(Dialogue dialogue, Node node, DialogueCondition condition) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _condition = condition;

            GUID = condition.Guid;
            position = node.position;

            UpdateTexts();
        }

        private void UpdateTexts()
        {
            string baseTitle = "No Condition";
            if (_condition.Condition != null) baseTitle = _condition.Condition.GetType().Name;

            title = string.IsNullOrEmpty(_condition.Tag) ? baseTitle : $"{baseTitle} ({_condition.Tag})";

            extensionContainer.Clear();
            extensionContainer.Add(new Label(_condition.Description));
        }

        public string InspectorTitle => "Condition";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _condition.Guid;
            inputContainer.Add(entryPort);

            var truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
            truePort.portName = "True";
            truePort.viewDataKey = _condition.TrueOutputGuid;
            outputContainer.Add(truePort);

            var falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
            falsePort.portName = "False";
            falsePort.viewDataKey = _condition.FalseOutputGuid;
            outputContainer.Add(falsePort);
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var conditionsProp = serializedObject.FindProperty("_conditions");
            SerializedProperty conditionProp = null;
            for (int i = 0; i < conditionsProp.arraySize; i++)
            {
                if (conditionsProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _condition.Guid)
                {
                    conditionProp = conditionsProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (conditionProp != null)
            {
                var tagField = new PropertyField(conditionProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTexts());
                inspector.Add(tagField);

                var conditionInstanceProp = conditionProp.FindPropertyRelative("_condition");

                var conditionField = new PropertyField(conditionInstanceProp, "Condition");
                conditionField.Bind(serializedObject);

                conditionField.RegisterValueChangeCallback(change => UpdateTexts());

                inspector.Add(conditionField);
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