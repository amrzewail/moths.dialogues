using Moths.Graphs.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Dialogues.Editor.Graphs.Nodes
{
    public class DialogueJumpNode : BasicNode, IInspectable, ISerializable
    {
        private Dialogue _dialogue;
        private Node _node;
        private DialogueJump _jump;

        public DialogueJumpNode(Dialogue dialogue, Node node, DialogueJump jump) : base()
        {
            _dialogue = dialogue;
            _node = node;
            _jump = jump;

            GUID = jump.Guid;
            UpdateTitle();
            position = node.position;
        }

        public string Serialize()
        {
            return _jump.Serialize();
        }

        private void UpdateTitle()
        {
            title = string.IsNullOrEmpty(_jump.TargetTag) ? "Jump" : $"Jump ({_jump.TargetTag})";
        }

        public string InspectorTitle => "Jump";

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueElement));
            entryPort.portName = "In";
            entryPort.viewDataKey = _jump.Guid;
            inputContainer.Add(entryPort);

            // Jump node doesn't need an output port in the graph because it's a "teleport"
            // But for consistency with your Next() logic which uses OutputGuid, we keep the data link.
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();
            
            // Get all unique tags and their guids
            var tagMap = new Dictionary<string, string>();
            var guidMap = new Dictionary<string, string>();
            
            foreach (var s in _dialogue.Sequences) if (!string.IsNullOrEmpty(s.Tag)) tagMap[s.Tag] = s.Guid;
            foreach (var c in _dialogue.Choices) if (!string.IsNullOrEmpty(c.Tag)) tagMap[c.Tag] = c.Guid;
            foreach (var a in _dialogue.Actions) if (!string.IsNullOrEmpty(a.Tag)) tagMap[a.Tag] = a.Guid;
            foreach (var o in _dialogue.Outputs) if (!string.IsNullOrEmpty(o.Tag)) tagMap[o.Tag] = o.Guid;
            foreach (var cond in _dialogue.Conditions) if (!string.IsNullOrEmpty(cond.Tag)) tagMap[cond.Tag] = cond.Guid;
            
            foreach (var s in _dialogue.Sequences) guidMap[s.Guid] = s.Tag;
            foreach (var c in _dialogue.Choices) guidMap[c.Guid] = c.Tag;
            foreach (var a in _dialogue.Actions) guidMap[a.Guid] = a.Tag;
            foreach (var o in _dialogue.Outputs) guidMap[o.Guid] = o.Tag;
            foreach (var cond in _dialogue.Conditions) guidMap[cond.Guid] = cond.Tag;

            var tags = tagMap.Keys.ToList();
            tags.Sort();

            if (tags.Count == 0) tags.Add("");

            string targetGuid = _jump.OutputGuid;
            var targetTag = "";
            if (guidMap.TryGetValue(targetGuid, out var tag)) targetTag = tag;

            if (string.IsNullOrEmpty(targetGuid) && tags.Count > 0)
            {
                targetTag = tags[0];
                if (tagMap.TryGetValue(targetTag, out var guid)) targetGuid = guid;
            }

            var dropdown = new PopupField<string>("Target Tag", tags, targetTag);

            if (_jump.TargetTag != targetTag)
            {
                _jump.SetTarget(targetTag, targetGuid);
                EditorUtility.SetDirty(_dialogue);
                UpdateTitle();
            }

            dropdown.RegisterValueChangedCallback(evt =>
            {
                if (tagMap.TryGetValue(evt.newValue, out string targetGuid))
                {
                    _jump.SetTarget(evt.newValue, targetGuid);
                    EditorUtility.SetDirty(_dialogue);
                    UpdateTitle();
                }
            });

            inspector.Add(dropdown);
            return inspector;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _node.position = newPos.position;
            _node.Update();
        }
    }
}