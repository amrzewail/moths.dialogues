using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public interface IInspectable
    {
        string InspectorTitle { get; }
        VisualElement GetInspector();
    }
}
