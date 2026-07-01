using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.WSA;

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
                    onCreateNew: path =>
                    {
                        CreateAndAssignNewDialogue(path, property);
                    }
                );
                dropdown.Show(dropdownRect);
            }

            // 2. Open Button (Only enabled when a Dialogue is assigned)
            EditorGUI.BeginDisabledGroup(currentDialogue == null || property.hasMultipleDifferentValues);
            if (GUI.Button(openRect, new GUIContent("Edit", "Edit this Dialogue in the Dialogue Creator"), EditorStyles.miniButton))
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

        private void CreateAndAssignNewDialogue(string folderPath, SerializedProperty property)
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.name = "New Dialogue";

            string path = EditorUtility.SaveFilePanelInProject("Create a dialogue", "New Dialogue", "asset", "", folderPath);
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
        private readonly Action<string> _onCreateNew;

        public DialogueDropdown(AdvancedDropdownState state, Action<Dialogue> onSelected, Action<string> onCreateNew) : base(state)
        {
            _onSelected = onSelected;
            _onCreateNew = onCreateNew;
            minimumSize = new Vector2(300, 350);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Dialogue");

            // Add the None option at the top
            root.AddChild(new DialogueDropdownItem(null, "None", null));

            // Add the Create New option
            root.AddChild(new DialogueDropdownItem(null, "New Dialogue...", "Assets", isCreateNew: true));

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

            // Sort dialogues by parent folder name, then dialogue name
            dialogues.Sort((a, b) =>
            {
                string pathA = AssetDatabase.GetAssetPath(a);
                string pathB = AssetDatabase.GetAssetPath(b);
                string folderA = GetImmediateParentFolder(pathA);
                string folderB = GetImmediateParentFolder(pathB);

                int folderCompare = string.Compare(folderA, folderB, StringComparison.OrdinalIgnoreCase);
                if (folderCompare != 0) return folderCompare;

                return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
            });

            // Dictionary to store created parent folder items to avoid duplicate folder nodes
            var folderCache = new Dictionary<string, AdvancedDropdownItem>();

            // Add dialogues to the dropdown
            foreach (var dialogue in dialogues)
            {
                string path = AssetDatabase.GetAssetPath(dialogue);
                if (string.IsNullOrEmpty(path)) continue;

                string parentFolder = GetImmediateParentFolder(path);

                if (parentFolder == "Assets")
                {
                    // Add directly to root if it is in the root assets folder
                    root.AddChild(new DialogueDropdownItem(dialogue, dialogue.name, "Assets"));
                }
                else
                {
                    // Add to its parent folder submenu
                    if (!folderCache.TryGetValue(parentFolder, out var folderItem))
                    {
                        folderItem = new AdvancedDropdownItem(parentFolder);

                        // Set folder icon
                        Texture2D folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
                        if (folderIcon != null)
                        {
                            folderItem.icon = folderIcon;
                        }
                        root.AddChild(folderItem);
                        folderCache[parentFolder] = folderItem;

                        folderItem.AddChild(new DialogueDropdownItem(null, "New Dialogue...", Path.GetDirectoryName(path), isCreateNew: true));
                    }

                    folderItem.AddChild(new DialogueDropdownItem(dialogue, dialogue.name, "Assets"));
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is DialogueDropdownItem dialogueItem)
            {
                if (dialogueItem.IsCreateNew)
                {
                    _onCreateNew?.Invoke(dialogueItem.Path);
                }
                else
                {
                    _onSelected?.Invoke(dialogueItem.Dialogue);
                }
            }
        }

        private string GetImmediateParentFolder(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return "Assets";
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(dir)) return "Assets";

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
        public string Path { get; }

        public DialogueDropdownItem(Dialogue dialogue, string name, string path, bool isCreateNew = false) : base(name)
        {
            Dialogue = dialogue;
            IsCreateNew = isCreateNew;
            Path = path;

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
