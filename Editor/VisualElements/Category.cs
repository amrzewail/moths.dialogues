using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.VisualElements
{
    public class Category : VisualElement
    {
        private Label _label;
        private VisualElement _content;

        public VisualElement Content => _content;

        public Category(string title)
        {
            _label = new Label(title);
            this.Add(_label);

            _content = new VisualElement();
            _content.AddToClassList("content");
            this.Add(_content);
        }
    }
}
