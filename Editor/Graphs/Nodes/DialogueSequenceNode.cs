using Moths.Graphs.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueSequenceNode : BasicNode, IInspectable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueSequence _sequence;

        public DialogueSequenceNode(Dialogue dialogue, Node node, DialogueSequence sequence) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _sequence = sequence;

            GUID = sequence.Guid;
            UpdateTitle();
            position = node.position;
        }

        private void UpdateTitle()
        {
            title = string.IsNullOrEmpty(_sequence.Tag) ? "Sequence" : $"Sequence ({_sequence.Tag})";
        }

        public string InspectorTitle => "Sequence";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _sequence.Guid;
            inputContainer.Add(entryPort);

            var exitPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueElement));
            exitPort.portName = "Next";
            exitPort.viewDataKey = _sequence.OutputGuid;
            outputContainer.Add(exitPort);

            UpdateLinesContainer();
        }

        private void UpdateLinesContainer()
        {
            extensionContainer.Clear();
            if (_sequence.Lines == null) return;

            foreach(var line in _sequence.Lines)
            {
                extensionContainer.Add(new Label($"<b>{line.Speaker}</b>: {line.Line}"));
            }

            RefreshExpandedState();
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);
            
            // We need to find the specific sequence in the list
            var sequencesProp = serializedObject.FindProperty("_sequences");
            SerializedProperty sequenceProp = null;
            SerializedProperty linesProp = null;
            for (int i = 0; i < sequencesProp.arraySize; i++)
            {
                if (sequencesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_guid").stringValue == _sequence.Guid)
                {
                    sequenceProp = sequencesProp.GetArrayElementAtIndex(i);
                    linesProp = sequenceProp.FindPropertyRelative("_lines");
                    break;
                }
            }

            if (sequenceProp != null)
            {
                var tagField = new PropertyField(sequenceProp.FindPropertyRelative("_tag"), "Tag");
                tagField.Bind(serializedObject);
                tagField.RegisterValueChangeCallback(evt => UpdateTitle());
                inspector.Add(tagField);
            }

            // Create a main container to hold the entire list and the Add button
            VisualElement linesContainer = new VisualElement();
            linesContainer.style.marginTop = 10;
            linesContainer.style.marginBottom = 10;
            linesContainer.style.paddingLeft = 10;
            linesContainer.style.borderLeftWidth = 2;
            linesContainer.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));

            // Local function to draw and redraw the list when array changes occur
            void DrawLines()
            {
                linesContainer.Clear();
                serializedObject.Update(); // Ensure we have the latest serialized data

                // Re-find the lines property to ensure it's still valid after updates
                var sequencesPropCurrent = serializedObject.FindProperty("_sequences");
                linesProp = null;
                for (int j = 0; j < sequencesPropCurrent.arraySize; j++)
                {
                    if (sequencesPropCurrent.GetArrayElementAtIndex(j).FindPropertyRelative("_guid").stringValue == _sequence.Guid)
                    {
                        linesProp = sequencesPropCurrent.GetArrayElementAtIndex(j).FindPropertyRelative("_lines");
                        break;
                    }
                }

                if (linesProp == null) return;

                for (int i = 0; i < linesProp.arraySize; i++)
                {
                    int currentIndex = i; // Crucial: Capture the index into a local variable for the button lambdas
                    var lineProp = linesProp.GetArrayElementAtIndex(currentIndex);

                    var verticalContainer = new VisualElement();
                    verticalContainer.style.marginBottom = 4;
                    verticalContainer.style.paddingBottom = 4;
                    verticalContainer.style.borderBottomWidth = 1;
                    verticalContainer.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.5f)); // Subtle separator

                    // --- 1. Row Container ---
                    var lineElement = new VisualElement();
                    lineElement.style.flexDirection = FlexDirection.Row; // Align items horizontally
                    lineElement.style.alignItems = Align.Center; // Vertically center items in the row

                    var speakerGuidProp = lineProp.FindPropertyRelative("_speakerGuid");

                    var speakersList = _dialogue.Speakers.ToList();
                    speakersList.Add(default);
                    var speaker = new PopupField<DialogueSpeaker>(
                        "",
                        speakersList,
                        speakersList.SingleOrDefault(s => s.Guid == speakerGuidProp.stringValue),
                        (speaker) => $"<b>{speaker.Name}</b>",
                        (speaker) => speaker.Name);

                    speaker.RegisterValueChangedCallback(change =>
                    {
                        speakerGuidProp.stringValue = change.newValue.Guid;
                        serializedObject.ApplyModifiedProperties();
                        UpdateLinesContainer();
                    });

                    speaker.style.flexGrow = 1;

                    // --- 2. The Property Field ---
                    // Pass string.Empty to hide the default label and save space
                    var line = new PropertyField(lineProp.FindPropertyRelative("_line"), "");
                    line.style.flexGrow = 1; // Make it expand to fill all empty horizontal space
                    line.Bind(serializedObject);

                    line.RegisterValueChangeCallback(change =>
                    {
                        UpdateLinesContainer();
                    });


                    // --- 3. Buttons Container ---
                    var buttonGroup = new VisualElement();
                    buttonGroup.style.flexDirection = FlexDirection.Row;
                    buttonGroup.style.marginLeft = 10;

                    // Move Up Button
                    var moveUpBtn = new Button(() =>
                    {
                        linesContainer.Clear();
                        linesProp.MoveArrayElement(currentIndex, currentIndex - 1);
                        serializedObject.ApplyModifiedProperties();
                        DrawLines(); // Redraw UI
                        UpdateLinesContainer();
                    })
                    { text = "▲" };
                    moveUpBtn.SetEnabled(currentIndex > 0); // Disable if it's the first element

                    // Move Down Button
                    var moveDownBtn = new Button(() =>
                    {
                        linesContainer.Clear();
                        linesProp.MoveArrayElement(currentIndex, currentIndex + 1);
                        serializedObject.ApplyModifiedProperties();
                        DrawLines(); // Redraw UI
                        UpdateLinesContainer();
                    })
                    { text = "▼" };
                    moveDownBtn.SetEnabled(currentIndex < linesProp.arraySize - 1); // Disable if it's the last element

                    // Delete Button
                    var deleteBtn = new Button(() =>
                    {
                        linesContainer.Clear();
                        int oldSize = linesProp.arraySize;
                        linesProp.DeleteArrayElementAtIndex(currentIndex);

                        // Unity quirk: Deleting a reference type often just sets it to null on the first pass.
                        // If the size didn't change, delete it again to actually remove the element.
                        if (linesProp.arraySize == oldSize)
                        {
                            linesProp.DeleteArrayElementAtIndex(currentIndex);
                        }

                        serializedObject.ApplyModifiedProperties();
                        DrawLines(); // Redraw UI
                        UpdateLinesContainer();
                    })
                    { text = "✖" };
                    deleteBtn.style.backgroundColor = new StyleColor(new Color(0.6f, 0.2f, 0.2f, 1f)); // Slight red tint for danger

                    // Assemble the buttons
                    buttonGroup.Add(moveUpBtn);
                    buttonGroup.Add(moveDownBtn);
                    buttonGroup.Add(deleteBtn);

                    // Assemble the row
                    lineElement.Add(speaker);
                    lineElement.Add(buttonGroup);

                    verticalContainer.Add(lineElement);
                    verticalContainer.Add(line);

                    linesContainer.Add(verticalContainer);
                }

                // --- 4. Add Choice Button ---
                var addBtn = new Button(() =>
                {
                    linesContainer.Clear();
                    linesProp.InsertArrayElementAtIndex(linesProp.arraySize);
                    linesProp.GetArrayElementAtIndex(linesProp.arraySize - 1).boxedValue = new DialogueLine(_dialogue);
                    serializedObject.ApplyModifiedProperties();
                    DrawLines(); // Redraw UI
                    UpdateLinesContainer();
                })
                { text = "Add Line" };

                addBtn.style.marginTop = 5;
                addBtn.style.height = 25;
                addBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.5f, 0.2f, 1f)); // Slight green tint

                linesContainer.Add(addBtn);
            }

            // Draw the initial list
            DrawLines();

            // Add the fully functional container to your inspector
            inspector.Add(linesContainer);
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
