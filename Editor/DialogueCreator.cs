using Moths.Dialogues.Editor.Graphs;
using Moths.Dialogues.Editor.VisualElements;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor
{
    public class DialogueCreator : EditorWindow
    {
        [SerializeField] Dialogue _dialogue;
        [SerializeField] DialogueGraphProperties _properties;

        private VisualElement _graphs;
        private HistoryStack _history;
        private Sidebar _nodeInspector;

        public HistoryStack History => _history;
        public Dialogue Dialogue => _dialogue;
        public DialogueGraphProperties Graph => _properties;

        [MenuItem("Moths/Dialogues/Create Dialogue")]
        public static void CreateDialogueMenu()
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.name = "New Dialogue";
            string path = EditorUtility.SaveFilePanelInProject("Create a dialogue", "New Dialogue", "asset", "");
            if (string.IsNullOrEmpty(path)) return;
            AssetDatabase.CreateAsset(dialogue, path);
            AssetDatabase.Refresh();
        }

        protected virtual void OnEnable()
        {
            _graphs = new VisualElement();
            _graphs.StretchToParentSize();
            _history = new HistoryStack();

            _nodeInspector = new Sidebar();
            _nodeInspector.AddToClassList("right-align");
            _nodeInspector.AddToClassList("inspector");
            _nodeInspector.visible = false;

            rootVisualElement.Add(_graphs);
            rootVisualElement.Add(_history);
            rootVisualElement.Add(_nodeInspector);

            // Load styles
            var styleSheet = Resources.Load<StyleSheet>("Moths.Dialogues/Styles");
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            if (_dialogue)
            {
                OpenGraph<DialogueGraph, Dialogue>(_dialogue);
            }
        }

        public void OpenGraph<T, TData>(TData data) where T : DialogueBaseGraph<TData>
        {
            T t = Activator.CreateInstance<T>();
            t.Initialize(this, data);
            _graphs.Clear();
            _graphs.Add(t);

            string name = (data is Dialogue d) ? d.name : "Sub-Graph";

            History.AddToStack(name, () =>
            {
                _graphs.Clear();
                _graphs.Add(t);
                if (t is IRefreshable refreshable) refreshable.Refresh();
            });
        }

        public void Inspect(string title, VisualElement element)
        {
            _nodeInspector.Content.Clear();
            if (element != null)
            {
                _nodeInspector.title = title;
                _nodeInspector.Content.Add(element);
                _nodeInspector.visible = true;
                return;
            }

            _nodeInspector.visible = false;
        }

        public void SetAssetDirty()
        {
            EditorUtility.SetDirty(_dialogue);
        }

        [OnOpenAsset]
        public static bool OpenDialogueAsset(int instanceID, int line)
        {
            UnityEngine.Object asset = EditorUtility.InstanceIDToObject(instanceID);
            if (!(asset is Dialogue dialogueAsset)) return false;

            string assetPath = AssetDatabase.GetAssetPath(dialogueAsset);
            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            DialogueGraphProperties graphProperties = null;
            foreach (var obj in allAssets)
            {
                if (obj is DialogueGraphProperties)
                {
                    graphProperties = obj as DialogueGraphProperties;
                    break;
                }
            }

            if (graphProperties == null)
            {
                graphProperties = ScriptableObject.CreateInstance<DialogueGraphProperties>();
                graphProperties.name = "Graph";
                AssetDatabase.AddObjectToAsset(graphProperties, assetPath);
                AssetDatabase.SaveAssets();
            }

            DialogueCreator window = EditorWindow.CreateWindow<DialogueCreator>();

            Texture2D icon = Resources.Load<Texture2D>("Moths.Dialogues/icon_dialogue");
            window.titleContent = new GUIContent("Dialogue Creator", icon);
            window._properties = graphProperties;
            window._dialogue = dialogueAsset;

            window.OpenGraph<DialogueGraph, Dialogue>(dialogueAsset);

            return true;
        }
    }
}
