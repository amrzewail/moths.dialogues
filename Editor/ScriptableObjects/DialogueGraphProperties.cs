using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Moths.Dialogues.Editor
{
    [System.Serializable]
    public struct Node
    {
        private DialogueGraphProperties _properties;

        public string guid;
        public Vector2 position;

        public void SetProperties(DialogueGraphProperties properties) => _properties = properties;

        public void Update()
        {
            if (!_properties) return;

            _properties.UpdateNode(this);
            EditorUtility.SetDirty(_properties);
        }

        public static implicit operator bool(Node node) => !string.IsNullOrEmpty(node.guid);
    }

    public class DialogueGraphProperties : ScriptableObject
    {
        [SerializeField, HideInInspector] List<Node> _nodes = new();

        public IReadOnlyList<Node> Nodes => _nodes;

        public Node FindNodeByGuid(string guid)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Node node = Nodes[i];
                if (node.guid == guid) return node;
            }
            return default;
        }

        public Node FindNodeByGuid(string guid, out bool isNew)
        {
            isNew = false;

            for (int i = 0; i < Nodes.Count; i++)
            {
                Node node = Nodes[i];
                node.SetProperties(this);
                if (node.guid == guid) return node;
            }

            isNew = true;
            Node n = new()
            {
                guid = guid,
            };
            n.SetProperties(this);

            _nodes.Add(n);

            return n;
        }

        public void UpdateNode(Node node)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                Node n = _nodes[i];
                if (n.guid == node.guid)
                {
                    _nodes[i] = node;
                    break;
                }
            }
        }
    }
}
