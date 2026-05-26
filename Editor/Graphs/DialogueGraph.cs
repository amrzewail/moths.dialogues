using Moths.Graphs.Editor;
using Moths.Dialogues.Editor.Graphs.Nodes;
using Moths.Dialogues.Editor.VisualElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Moths.Dialogues.Editor.Graphs
{
    public class DialogueGraph : DialogueBaseGraph<Dialogue>, IRefreshable
    {
        private DialogueCreator _editor;
        private Dialogue _dialogue;

        public override void Initialize(DialogueCreator editor, Dialogue data)
        {
            _editor = editor;
            _dialogue = data;

            Sidebar sidebar = new Sidebar();
            sidebar.title = _dialogue.name;

            var editCategory = sidebar.AddCategory("ELEMENTS");

            Button newSequenceBtn = new Button(() => NewElementCallback<DialogueSequence>(_dialogue.AddSequence)) { text = "New Sequence" };
            Button newChoicesBtn = new Button(() => NewElementCallback<DialogueChoices>(_dialogue.AddChoices)) { text = "New Choices" };
            Button newActionBtn = new Button(() => NewElementCallback<DialogueAction>(_dialogue.AddAction)) { text = "New Action" };
            Button newOutputBtn = new Button(() => NewElementCallback<DialogueOutput>(_dialogue.AddOutput)) { text = "New Output" };

            newSequenceBtn.AddToClassList("sidebar-element-btn");
            newChoicesBtn.AddToClassList("sidebar-element-btn");
            newActionBtn.AddToClassList("sidebar-element-btn");
            newOutputBtn.AddToClassList("sidebar-element-btn");

            editCategory.Content.Add(newSequenceBtn);
            editCategory.Content.Add(newChoicesBtn);
            editCategory.Content.Add(newActionBtn);
            editCategory.Content.Add(newOutputBtn);

            _graphView.EdgeCreated += EdgeCreatedCallback;
            _graphView.EdgeRemoved += EdgeRemovedCallback;
            _graphView.NodeSelected += NodeSelectedCallback;
            _graphView.NodeUnselected += NodeUnselectedCallback;
            _graphView.NodeRemoved += NodeRemovedCallback;

            this.Add(sidebar);

            Refresh();
        }

        private void NodeRemovedCallback(UnityEditor.Experimental.GraphView.Node node)
        {
            _editor.Inspect(null, null);

            if (node is DialogueSequenceNode sequenceNode) _dialogue.RemoveSequence(sequenceNode.GUID);
            else if (node is DialogueChoicesNode choicesNode) _dialogue.RemoveChoices(choicesNode.GUID);
            else if (node is DialogueActionNode actionNode) _dialogue.RemoveAction(actionNode.GUID);
            else if (node is DialogueOutputNode outputNode) _dialogue.RemoveOutput(outputNode.GUID);

            if (_dialogue.StartingGuid == (node as BasicNode).GUID) _dialogue.StartingGuid = string.Empty;

            EditorUtility.SetDirty(_dialogue);
            Refresh();
        }

        private void EdgeCreatedCallback(Edge edge)
        {
            if (edge.output.node is DialogueStartNode)
            {
                _dialogue.StartingGuid = edge.input.viewDataKey;
            }
            else
            {
                _dialogue.Connections[edge.output.viewDataKey] = edge.input.viewDataKey;
            }
            EditorUtility.SetDirty(_dialogue);
        }

        private void EdgeRemovedCallback(Edge edge)
        {
            try {
                if (edge.output.node is DialogueStartNode)
                {
                    _dialogue.StartingGuid = string.Empty;
                }
                else
                {
                    _dialogue.Connections.Remove(edge.output.viewDataKey);
                }
                EditorUtility.SetDirty(_dialogue);
            }
            catch { }
        }

        private void NodeUnselectedCallback(BasicNode node)
        {
            _editor.Inspect(null, null);
        }

        private void NodeSelectedCallback(BasicNode node)
        {
            if (node is IInspectable inspectable)
            {
                _editor.Inspect(inspectable.InspectorTitle, inspectable.GetInspector());
            }
        }

        public void Refresh()
        {
            RefreshNodes();
            RefreshConnections();
        }

        private void RefreshNodes()
        {
            _graphView.ClearNodes();

            var startNode = new DialogueStartNode(_dialogue, "Dialogue Start");
            _graphView.AddNode(startNode);

            foreach (var sequence in _dialogue.Sequences)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(sequence.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                _graphView.AddNode(new DialogueSequenceNode(_dialogue, nodeMetadata, sequence));
            }

            foreach (var choices in _dialogue.Choices)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(choices.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                var choicesNode = new DialogueChoicesNode(_dialogue, nodeMetadata, choices);
                _graphView.AddNode(choicesNode);
                choicesNode.PortsUpdated += RefreshConnections;
            }

            foreach (var action in _dialogue.Actions)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(action.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                _graphView.AddNode(new DialogueActionNode(_dialogue, nodeMetadata, action));
            }

            foreach (var output in _dialogue.Outputs)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(output.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                _graphView.AddNode(new DialogueOutputNode(_dialogue, nodeMetadata, output));
            }
        }

        private void RefreshConnections()
        {
            _graphView.ClearEdges();

            if (!string.IsNullOrEmpty(_dialogue.StartingGuid))
            {
                if (_graphView.GetNodeByGUID(_dialogue.StartingGuid) == null)
                {
                    _dialogue.StartingGuid = string.Empty;
                    EditorUtility.SetDirty(_dialogue);
                }
                else
                {
                    _graphView.LinkNodes(DialogueStartNode.GUID, _dialogue.StartingGuid);
                }
            }

            foreach (var connection in _dialogue.Connections)
            {
                try
                {
                    _graphView.LinkNodes(connection.key, connection.value);
                }
                catch { }
            }
        }

        private void NewElementCallback<T>(Action<T> addMethod) where T : new()
        {
            T element = new T();
            addMethod(element);
            EditorUtility.SetDirty(_dialogue);
            Refresh();
        }
    }
}
