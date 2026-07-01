using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Moths.Dialogues.Editor
{
    [CustomPropertyDrawer(typeof(Dialogue))]
    public class DialoguePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property drawing to support prefab overrides and normal styling
            EditorGUI.BeginProperty(position, label, property);

            // Draw prefix label and get the remaining rect for our custom controls
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Layout sizing
            float buttonWidth = 42f;
            float spacing = 2f;
            float dropdownWidth = position.width - (buttonWidth + spacing);

            // Calculate control rects
            Rect dropdownRect = new Rect(position.x, position.y, dropdownWidth, position.height);
            Rect openRect = new Rect(position.x + dropdownWidth + spacing, position.y, buttonWidth, position.height);

            // Retrieve current reference
            Dialogue currentDialogue = property.objectReferenceValue as Dialogue;

            // Determine content for the dropdown button (with custom icon and name, without folder prefix)
            GUIContent dropdownContent;
            if (property.hasMultipleDifferentValues)
            {
                dropdownContent = new GUIContent("—");
            }
            else if (currentDialogue != null)
            {
                Texture2D dialogueIcon = Resources.Load<Texture2D>("Moths.Dialogues/icon_dialogue");
                dropdownContent = new GUIContent(currentDialogue.name, dialogueIcon, "Select Dialogue");
            }
            else
            {
                dropdownContent = new GUIContent("None");
            }

            // 1. Dropdown Button
            if (GUI.Button(dropdownRect, dropdownContent, EditorStyles.popup))
            {
                var dropdown = new DialogueDropdown(
                    new AdvancedDropdownState(),
                    onSelected: (selectedDialogue) =>
                    {
                        property.serializedObject.Update();
                        property.objectReferenceValue = selectedDialogue;
                        property.serializedObject.ApplyModifiedProperties();
                    },
                    onCreateNew: () =>
                    {
                        CreateAndAssignNewDialogue(property);
                    }
                );
                dropdown.Show(dropdownRect);
            }

            // 2. Open Button (Only enabled when a Dialogue is assigned)
            EditorGUI.BeginDisabledGroup(currentDialogue == null || property.hasMultipleDifferentValues);
            if (GUI.Button(openRect, new GUIContent("Open", "Open this Dialogue in the Dialogue Creator"), EditorStyles.miniButton))
            {
                AssetDatabase.OpenAsset(currentDialogue);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private void CreateAndAssignNewDialogue(SerializedProperty property)
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.name = "New Dialogue";
            string path = EditorUtility.SaveFilePanelInProject("Create a dialogue", "New Dialogue", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(dialogue, path);
            AssetDatabase.Refresh();

            property.serializedObject.Update();
            property.objectReferenceValue = dialogue;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    public class DialogueDropdown : AdvancedDropdown
    {
        private readonly Action<Dialogue> _onSelected;
        private readonly Action _onCreateNew;

        public DialogueDropdown(AdvancedDropdownState state, Action<Dialogue> onSelected, Action onCreateNew) : base(state)
        {
            _onSelected = onSelected;
            _onCreateNew = onCreateNew;
            minimumSize = new Vector2(300, 350);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Dialogue");

            // Add the None option at the top
            root.AddChild(new DialogueDropdownItem(null, "None"));

            // Add the Create New option
            root.AddChild(new DialogueDropdownItem(null, "Create New Dialogue...", isCreateNew: true));

            // Find all Dialogue assets in the project
            string[] guids = AssetDatabase.FindAssets("t:Dialogue");
            List<Dialogue> dialogues = new List<Dialogue>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(path);
                if (dialogue != null)
                {
                    dialogues.Add(dialogue);
                }
            }

            // Sort dialogues by folder name, then dialogue name
            dialogues.Sort((a, b) =>
            {
                string pathA = AssetDatabase.GetAssetPath(a);
                string pathB = AssetDatabase.GetAssetPath(b);
                string folderA = GetFolderPrefix(pathA);
                string folderB = GetFolderPrefix(pathB);

                int folderCompare = string.Compare(folderA, folderB, StringComparison.OrdinalIgnoreCase);
                if (folderCompare != 0) return folderCompare;

                return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
            });

            // Add to the dropdown
            foreach (var dialogue in dialogues)
            {
                string path = AssetDatabase.GetAssetPath(dialogue);
                string folder = GetFolderPrefix(path);
                string displayName = $"{folder}/{dialogue.name}";
                root.AddChild(new DialogueDropdownItem(dialogue, displayName));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is DialogueDropdownItem dialogueItem)
            {
                if (dialogueItem.IsCreateNew)
                {
                    _onCreateNew?.Invoke();
                }
                else
                {
                    _onSelected?.Invoke(dialogueItem.Dialogue);
                }
            }
        }

        private string GetFolderPrefix(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return "Unsaved";
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(dir)) return "Root";

            dir = dir.Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(dir);
            if (string.IsNullOrEmpty(folderName) || folderName == "Assets")
            {
                return "Assets";
            }
            return folderName;
        }
    }

    public class DialogueDropdownItem : AdvancedDropdownItem
    {
        public Dialogue Dialogue { get; }
        public bool IsCreateNew { get; }

        public DialogueDropdownItem(Dialogue dialogue, string name, bool isCreateNew = false) : base(name)
        {
            Dialogue = dialogue;
            IsCreateNew = isCreateNew;

            if (isCreateNew)
            {
                // Try to load a creation icon (plus icon)
                Texture2D createIcon = EditorGUIUtility.IconContent("Toolbar Plus").image as Texture2D;
                if (createIcon != null)
                {
                    this.icon = createIcon;
                }
            }
            else
            {
                // Try to assign the dialogue icon
                Texture2D icon = Resources.Load<Texture2D>("Moths.Dialogues/icon_dialogue");
                if (icon != null && dialogue != null)
                {
                    this.icon = icon;
                }
            }
        }
    }
}
