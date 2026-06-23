using Moths.Graphs.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    [System.Serializable]
    public class DialogueStartNode : StartNode<DialogueElement>, IInspectable
    {
        private Dialogue _dialogue;
        public DialogueStartNode(Dialogue dialogue, string title) : base(title)
        {
            _dialogue = dialogue;
        }

        public string InspectorTitle => "Dialogue Start";

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            var serializedObject = new SerializedObject(_dialogue);

            var listProp = serializedObject.FindProperty("_speakers");

            // Create a main container to hold the entire list and the Add button
            VisualElement speakersContainer = new VisualElement();
            speakersContainer.style.marginTop = 10;
            speakersContainer.style.marginBottom = 10;
            speakersContainer.style.paddingLeft = 10;
            speakersContainer.style.borderLeftWidth = 2;
            speakersContainer.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));

            // Local function to draw and redraw the list when array changes occur
            void DrawSpeakers()
            {
                speakersContainer.Clear();
                serializedObject.Update(); // Ensure we have the latest serialized data

                for (int i = 0; i < listProp.arraySize; i++)
                {
                    int currentIndex = i; // Crucial: Capture the index into a local variable for the button lambdas
                    var speakerProp = listProp.GetArrayElementAtIndex(currentIndex);

                    var speakerContainer = new VisualElement();
                    speakerContainer.style.marginBottom = 4;
                    speakerContainer.style.paddingBottom = 4;
                    speakerContainer.style.borderBottomWidth = 1;
                    speakerContainer.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f)); // Subtle separator

                    // --- 1. Row Container ---
                    var rowContainer = new VisualElement();
                    rowContainer.style.flexDirection = FlexDirection.Row; // Align items horizontally
                    rowContainer.style.alignItems = Align.Center; // Vertically center items in the row

                    // --- 2. The Property Field ---
                    // Pass string.Empty to hide the default label and save space
                    var name = new PropertyField(speakerProp.FindPropertyRelative("_name"), string.Empty);
                    name.style.flexGrow = 1; // Make it expand to fill all empty horizontal space
                    name.Bind(serializedObject);

                    var data = new PropertyField(speakerProp.FindPropertyRelative("_data"));
                    data.style.flexGrow = 1;
                    data.Bind(serializedObject);

                    // --- 3. Buttons Container ---
                    var buttonGroup = new VisualElement();
                    buttonGroup.style.flexDirection = FlexDirection.Row;
                    buttonGroup.style.marginLeft = 10;

                    // Move Up Button
                    var moveUpBtn = new Button(() =>
                    {
                        listProp.MoveArrayElement(currentIndex, currentIndex - 1);
                        serializedObject.ApplyModifiedProperties();
                        DrawSpeakers(); // Redraw UI
                    })
                    { text = "▲" };
                    moveUpBtn.SetEnabled(currentIndex > 0); // Disable if it's the first element

                    // Move Down Button
                    var moveDownBtn = new Button(() =>
                    {
                        listProp.MoveArrayElement(currentIndex, currentIndex + 1);
                        serializedObject.ApplyModifiedProperties();
                        DrawSpeakers(); // Redraw UI
                    })
                    { text = "▼" };
                    moveDownBtn.SetEnabled(currentIndex < listProp.arraySize - 1); // Disable if it's the last element

                    // Delete Button
                    var deleteBtn = new Button(() =>
                    {
                        int oldSize = listProp.arraySize;
                        listProp.DeleteArrayElementAtIndex(currentIndex);

                        // Unity quirk: Deleting a reference type often just sets it to null on the first pass.
                        // If the size didn't change, delete it again to actually remove the element.
                        if (listProp.arraySize == oldSize)
                        {
                            listProp.DeleteArrayElementAtIndex(currentIndex);
                        }

                        serializedObject.ApplyModifiedProperties();
                        DrawSpeakers(); // Redraw UI
                    })
                    { text = "✖" };
                    deleteBtn.style.backgroundColor = new StyleColor(new Color(0.6f, 0.2f, 0.2f, 1f)); // Slight red tint for danger

                    // Assemble the buttons
                    buttonGroup.Add(moveUpBtn);
                    buttonGroup.Add(moveDownBtn);
                    buttonGroup.Add(deleteBtn);

                    // Assemble the row
                    rowContainer.Add(name);
                    rowContainer.Add(buttonGroup);

                    speakerContainer.Add(rowContainer);
                    speakerContainer.Add(data);

                    speakersContainer.Add(speakerContainer);
                }

                // --- 4. Add Choice Button ---
                var addBtn = new Button(() =>
                {
                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                    listProp.GetArrayElementAtIndex(listProp.arraySize - 1).boxedValue = new DialogueSpeaker(System.Guid.NewGuid().ToString());
                    serializedObject.ApplyModifiedProperties();
                    DrawSpeakers(); // Redraw UI
                })
                { text = "Add Speaker" };

                addBtn.style.marginTop = 5;
                addBtn.style.height = 25;
                addBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.5f, 0.2f, 1f)); // Slight green tint

                speakersContainer.Add(addBtn);
            }

            // Draw the initial list
            DrawSpeakers();

            // Add the fully functional container to your inspector
            inspector.Add(speakersContainer);
            return inspector;


        }
    }

    [System.Serializable]
    public class StartNode<TTargetType> : BasicNode
    {
        public new const string GUID = "$$START_NODE";

        public StartNode(string title)
        {
            base.title = title;
            base.GUID = GUID;
        }

        public override bool IsMovable() => false;
        public override bool IsCopiable() => false;

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(TTargetType));
            p.portName = "";
            p.viewDataKey = GUID;
            outputContainer.Add(p);
        }
    }
}
