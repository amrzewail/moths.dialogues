using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.VisualElements
{
    public class HistoryStack : VisualElement
    {
        private struct HistoryItem
        {
            public string name;
            public Action callback;
        }

        private List<HistoryItem> _items = new();

        public HistoryStack()
        {
        }

        public void AddToStack(string name, Action callback)
        {
            _items.Add(new HistoryItem { name = name, callback = callback });
            Refresh();
        }

        public void Refresh()
        {
            this.Clear();

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var index = i;

                Button btn = new Button(() =>
                {
                    _items.RemoveRange(index + 1, _items.Count - (index + 1));
                    item.callback();
                    Refresh();
                });
                btn.text = item.name;
                this.Add(btn);

                if (i < _items.Count - 1)
                {
                    this.Add(new Label(">"));
                }
            }
        }
    }
}
