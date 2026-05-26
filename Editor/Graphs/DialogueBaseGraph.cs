using Moths.Graphs.Editor;

namespace Moths.Dialogues.Editor.Graphs
{
    public abstract class DialogueBaseGraph<TData> : BasicGraphVisualElement
    {
        public abstract void Initialize(DialogueCreator editor, TData data);
    }
}
