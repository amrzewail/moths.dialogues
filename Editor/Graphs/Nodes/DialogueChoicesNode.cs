using Moths.Graphs.Editor;
using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueChoicesNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueChoices _choices;

        public event Action PortsUpdated;

        public DialogueChoicesNode(Dialogue dialogue, Node node, DialogueChoices choices) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _choices = choices;

            GUID = choices.Guid;
            UpdateTitle();
            position = node.position;
        }

        public string Serialize()
        {
            return _choices.Serialize();
        }

        private void UpdateTitle()
        {
            title = string.IsNullOrEmpty(_choices.Tag) ? "Choices" : $"Choices ({_choices.Tag})";
        }

        public string InspectorTitle => "Choices";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _choices.Guid;
            inputContainer.Add(entryPort);

            foreach (var choice in _choices.Choices)
            {
                var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
                p.portName = choice.Line.ToString();
                p.viewDataKey = choice.Guid;
                outputContainer.Add(p);
            }

            PortsUpdated?.Invoke();
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var choicesListProp = serializedObject.FindProperty("_choices");
            SerializedProperty choicesProp = null;
            for (int i = 0; i < choicesListProp.arraySize; i++)
            {
                if (choicesListProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _choices.Guid)
                {
                    choicesProp = choicesListProp.GetArrayElementAtIndex(i);
                    break;
                }
            }

            if (choicesProp != null)
            {
                var tagField = new PropertyField(choicesProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTitle());
                inspector.Add(tagField);
            }

            var listProp = choicesProp.FindPropertyRelative("_choices");

            // Create a main container to hold the entire list and the Add button
            VisualElement choicesContainer = new VisualElement();
            choicesContainer.style.marginTop = 10;
            choicesContainer.style.marginBottom = 10;
            choicesContainer.style.paddingLeft = 10;
            choicesContainer.style.borderLeftWidth = 2;
            choicesContainer.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));

            // Local function to draw and redraw the list when array changes occur
            void DrawChoices()
            {
                choicesContainer.Clear();
                serializedObject.Update(); // Ensure we have the latest serialized data

                // Re-find the choices property to ensure it's still valid after updates
                var choicesListPropCurrent = serializedObject.FindProperty("_choices");
                choicesProp = null;
                for (int j = 0; j < choicesListPropCurrent.arraySize; j++)
                {
                    if (choicesListPropCurrent.GetArrayElementAtIndex(j).FindPropertyRelative("_guid").stringValue == _choices.Guid)
                    {
                        choicesProp = choicesListPropCurrent.GetArrayElementAtIndex(j);
                        break;
                    }
                }

                if (choicesProp == null) return;
                listProp = choicesProp.FindPropertyRelative("_choices");

                for (int i = 0; i < listProp.arraySize; i++)
                {
                    int currentIndex = i; // Crucial: Capture the index into a local variable for the button lambdas
                    var choiceProp = listProp.GetArrayElementAtIndex(currentIndex);

                    // --- 1. Row Container ---
                    var choiceElement = new VisualElement();
                    choiceElement.style.flexDirection = FlexDirection.Row; // Align items horizontally
                    choiceElement.style.alignItems = Align.Center; // Vertically center items in the row
                    choiceElement.style.marginBottom = 4;
                    choiceElement.style.paddingBottom = 4;
                    choiceElement.style.borderBottomWidth = 1;
                    choiceElement.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.5f)); // Subtle separator

                    // --- 2. The Property Field ---
                    // Pass string.Empty to hide the default label and save space
                    var line = new PropertyField(choiceProp.FindPropertyRelative("_line"), string.Empty);
                    line.style.flexGrow = 1; // Make it expand to fill all empty horizontal space
                    line.Bind(serializedObject);

                    line.RegisterValueChangeCallback(change =>
                    {
                        GeneratePorts();
                    });

                    // --- 3. Buttons Container ---
                    var buttonGroup = new VisualElement();
                    buttonGroup.style.flexDirection = FlexDirection.Row;
                    buttonGroup.style.marginLeft = 10;

                    // Move Up Button
                    var moveUpBtn = new Button(() =>
                    {
                        choicesContainer.Clear();
                        listProp.MoveArrayElement(currentIndex, currentIndex - 1);
                        serializedObject.ApplyModifiedProperties();
                        DrawChoices(); // Redraw UI

                        GeneratePorts();
                    })
                    { text = "▲" };
                    moveUpBtn.SetEnabled(currentIndex > 0); // Disable if it's the first element

                    // Move Down Button
                    var moveDownBtn = new Button(() =>
                    {
                        choicesContainer.Clear();
                        listProp.MoveArrayElement(currentIndex, currentIndex + 1);
                        serializedObject.ApplyModifiedProperties();
                        DrawChoices(); // Redraw UI

                        GeneratePorts();
                    })
                    { text = "▼" };
                    moveDownBtn.SetEnabled(currentIndex < listProp.arraySize - 1); // Disable if it's the last element

                    // Delete Button
                    var deleteBtn = new Button(() =>
                    {
                        choicesContainer.Clear();
                        int oldSize = listProp.arraySize;
                        listProp.DeleteArrayElementAtIndex(currentIndex);

                        // Unity quirk: Deleting a reference type often just sets it to null on the first pass.
                        // If the size didn't change, delete it again to actually remove the element.
                        if (listProp.arraySize == oldSize)
                        {
                            listProp.DeleteArrayElementAtIndex(currentIndex);
                        }

                        serializedObject.ApplyModifiedProperties();
                        DrawChoices(); // Redraw UI

                        GeneratePorts();
                    })
                    { text = "✖" };
                    deleteBtn.style.backgroundColor = new StyleColor(new Color(0.6f, 0.2f, 0.2f, 1f)); // Slight red tint for danger

                    // Assemble the buttons
                    buttonGroup.Add(moveUpBtn);
                    buttonGroup.Add(moveDownBtn);
                    buttonGroup.Add(deleteBtn);

                    // Assemble the row
                    choiceElement.Add(line);
                    choiceElement.Add(buttonGroup);

                    choicesContainer.Add(choiceElement);
                }

                // --- 4. Add Choice Button ---
                var addBtn = new Button(() =>
                {
                    choicesContainer.Clear();
                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                    listProp.GetArrayElementAtIndex(listProp.arraySize - 1).boxedValue = new DialogueChoice(System.Guid.NewGuid().ToString());
                    serializedObject.ApplyModifiedProperties();
                    DrawChoices(); // Redraw UI

                    GeneratePorts();
                    RefreshPorts();
                })
                { text = "Add Choice" };

                addBtn.style.marginTop = 5;
                addBtn.style.height = 25;
                addBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.5f, 0.2f, 1f)); // Slight green tint

                choicesContainer.Add(addBtn);
            }

            // Draw the initial list
            DrawChoices();

            // Add the fully functional container to your inspector
            inspector.Add(choicesContainer);
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
