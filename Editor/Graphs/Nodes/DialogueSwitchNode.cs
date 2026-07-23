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
    public class DialogueSwitchNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueSwitch _switch;

        public event Action PortsUpdated;

        public DialogueSwitchNode(Dialogue dialogue, Node node, DialogueSwitch switchData) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _switch = switchData;

            GUID = switchData.Guid;
            position = node.position;

            UpdateTexts();
        }

        public string Serialize()
        {
            return _switch.Serialize();
        }

        private void UpdateTexts()
        {
            title = string.IsNullOrEmpty(_switch.Tag) ? "Switch" : $"Switch ({_switch.Tag})";

            extensionContainer.Clear();
            extensionContainer.Add(new Label($"Cases: {_switch.Count}"));
        }

        public string InspectorTitle => "Switch";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _switch.Guid;
            inputContainer.Add(entryPort);

            for (int i = 0; i < _switch.Cases.Count; i++)
            {
                var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
                var caseObj = _switch.Cases[i];
                string conditionDescription = caseObj.Condition.Value?.Description;
                if (string.IsNullOrEmpty(conditionDescription)) conditionDescription = "Unassigned";
                p.portName = $"Out {i}: {conditionDescription}";
                p.viewDataKey = caseObj.OutputGuid;
                outputContainer.Add(p);
            }

            var defaultPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
            defaultPort.portName = "Default";
            defaultPort.viewDataKey = _switch.DefaultOutputGuid;
            outputContainer.Add(defaultPort);
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var switchesProp = serializedObject.FindProperty("_switches");
            SerializedProperty switchProp = null;
            for (int i = 0; i < switchesProp.arraySize; i++)
            {
                if (switchesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _switch.Guid)
                {
                    switchProp = switchesProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (switchProp != null)
            {
                var tagField = new PropertyField(switchProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTexts());
                inspector.Add(tagField);

                var countProp = switchProp.FindPropertyRelative("_count");
                var countField = new PropertyField(countProp, "Case Count");
                countField.Bind(serializedObject);
                countField.RegisterValueChangeCallback(evt =>
                {
                    int count = countProp.intValue;
                    if (count < 0)
                    {
                        count = 0;
                        countProp.intValue = 0;
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                    _switch.AdjustCases(count);
                    UpdateTexts();
                    GeneratePorts();
                    PortsUpdated?.Invoke();
                });
                inspector.Add(countField);

                var casesProp = switchProp.FindPropertyRelative("_cases");
                for (int i = 0; i < casesProp.arraySize; i++)
                {
                    var caseProp = casesProp.GetArrayElementAtIndex(i);
                    var conditionProp = caseProp.FindPropertyRelative("_condition");
                    var conditionField = new PropertyField(conditionProp, $"Case {i}");
                    conditionField.Bind(serializedObject);
                    conditionField.RegisterValueChangeCallback(change => 
                    {
                        UpdateTexts();
                        GeneratePorts();
                        PortsUpdated?.Invoke();
                    });
                    inspector.Add(conditionField);
                }
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
