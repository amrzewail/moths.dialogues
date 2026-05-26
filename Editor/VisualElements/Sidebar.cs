using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.VisualElements
{
    public class Sidebar : ScrollView
    {
        private Label _title;
        private VisualElement _content;

        public string title { get => _title.text; set => _title.text = value; }

        public VisualElement Content => _content;

        public Sidebar()
        {
            this.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            _title = new Label();
            _title.AddToClassList("sidebar-title");
            this.Add(_title);

            _content = new VisualElement();
            this.Add(_content);
        }

        public Category AddCategory(string title)
        {
            Category category = new Category(title);
            _content.Add(category);
            return category;
        }

    }
}
