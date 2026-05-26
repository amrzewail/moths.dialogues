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
    [System.Serializable]
    public struct SerializedDialogueGraph
    {
        [System.Serializable]
        public struct SerializedConnection
        {
            public string outputNodeGuid;
            public string inputNodeGuid;
            public int outputPortIndex;
            public int inputPortIndex;
        }

        [System.Serializable]
        public struct SerializedNode
        {
            public string type;
            public string guid;
            public Vector2 position;
            public string data;
        }

        public List<SerializedNode> nodes;
        public List<SerializedConnection> connections;
    }

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
            Button newConditionBtn = new Button(() => NewElementCallback<DialogueCondition>(_dialogue.AddCondition)) { text = "New Condition" };
            Button newActionBtn = new Button(() => NewElementCallback<DialogueAction>(_dialogue.AddAction)) { text = "New Action" };
            Button newJumpBtn = new Button(() => NewElementCallback<DialogueJump>(_dialogue.AddJump)) { text = "New Jump" };
            Button newOutputBtn = new Button(() => NewElementCallback<DialogueOutput>(_dialogue.AddOutput)) { text = "New Output" };

            newSequenceBtn.AddToClassList("sidebar-element-btn");
            newChoicesBtn.AddToClassList("sidebar-element-btn");
            newConditionBtn.AddToClassList("sidebar-element-btn");
            newActionBtn.AddToClassList("sidebar-element-btn");
            newJumpBtn.AddToClassList("sidebar-element-btn");
            newOutputBtn.AddToClassList("sidebar-element-btn");

            editCategory.Content.Add(newSequenceBtn);
            editCategory.Content.Add(newChoicesBtn);
            editCategory.Content.Add(newConditionBtn);
            editCategory.Content.Add(newActionBtn);
            editCategory.Content.Add(newJumpBtn);
            editCategory.Content.Add(newOutputBtn);

            _graphView.EdgeCreated += EdgeCreatedCallback;
            _graphView.EdgeRemoved += EdgeRemovedCallback;
            _graphView.NodeSelected += NodeSelectedCallback;
            _graphView.NodeUnselected += NodeUnselectedCallback;
            _graphView.NodeRemoved += NodeRemovedCallback;

            _graphView.serializeGraphElements = SerializeGraphElementsCallback;
            _graphView.canPasteSerializedData = data => true;
            _graphView.unserializeAndPaste = UnserializePasteCallback;

            this.Add(sidebar);

            Refresh();
        }

        private void NodeRemovedCallback(UnityEditor.Experimental.GraphView.Node node)
        {
            _editor.Inspect(null, null);

            if (node is DialogueSequenceNode sequenceNode) _dialogue.RemoveSequence(sequenceNode.GUID);
            else if (node is DialogueChoicesNode choicesNode) _dialogue.RemoveChoices(choicesNode.GUID);
            else if (node is DialogueConditionNode conditionNode) _dialogue.RemoveCondition(conditionNode.GUID);
            else if (node is DialogueActionNode actionNode) _dialogue.RemoveAction(actionNode.GUID);
            else if (node is DialogueJumpNode jumpNode) _dialogue.RemoveJump(jumpNode.GUID);
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

        private string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            SerializedDialogueGraph serialized;
            serialized.nodes = new();
            serialized.connections = new();
            foreach(var element in elements)
            {
                if (element is BasicNode node)
                {
                    if (element is ISerializable serializable)
                    {
                        serialized.nodes.Add(new()
                        {
                            position = element.GetPosition().position,
                            type = element.GetType().FullName,
                            guid = node.GUID,
                            data = serializable.Serialize()
                        });
                    }
                }
                else if (element is Edge edge)
                {
                    serialized.connections.Add(new()
                    {
                        outputNodeGuid = ((BasicNode)edge.output.node).GUID,
                        outputPortIndex = edge.output.node.Query<Port>().ToList().IndexOf(edge.output),
                        inputNodeGuid = ((BasicNode)edge.input.node).GUID,
                        inputPortIndex = edge.input.node.Query<Port>().ToList().IndexOf(edge.input),
                    });
                }

            }
            return JsonUtility.ToJson(serialized);
        }
        private void UnserializePasteCallback(string operationName, string data)
        {
            _graphView.ClearSelection();

            SerializedDialogueGraph copyData = JsonUtility.FromJson<SerializedDialogueGraph>(data);
            if (copyData.nodes == null) return;

            Dictionary<string, BasicNode> oldGuidToNewNodeMap = new();
            foreach(var node in copyData.nodes)
            {
                var type = Type.GetType(node.type);
                BasicNode newNode = null;
                if (type == typeof(DialogueActionNode))
                {
                    var action = new DialogueAction(node.data);
                    _dialogue.AddAction(action);
                    newNode = AddDialogueNode(action, node.position - copyData.nodes[0].position);
                }

                if (newNode != null)
                {
                    oldGuidToNewNodeMap[node.guid] = newNode;

                    _graphView.AddToSelection(newNode);
                }
            }

            foreach (var connection in copyData.connections)
            {
                // Only paste the edge if BOTH connecting nodes were copied and pasted
                if (oldGuidToNewNodeMap.TryGetValue(connection.outputNodeGuid, out BasicNode newOutputNode) &&
                    oldGuidToNewNodeMap.TryGetValue(connection.inputNodeGuid, out BasicNode newInputNode))
                {
                    // Find the specific ports on the new nodes based on the port name
                    Port outputPort = newOutputNode.Query<Port>().AtIndex(connection.outputPortIndex);
                    Port inputPort = newInputNode.Query<Port>().AtIndex(connection.inputPortIndex);

                    if (outputPort != null && inputPort != null)
                    {
                        Edge edge = _graphView.LinkNodes(outputPort, inputPort);
                        _dialogue.Connections[edge.output.viewDataKey] = edge.input.viewDataKey;
                        _graphView.AddToSelection(edge);
                    }
                }
            }

            EditorUtility.SetDirty(_dialogue);
            Refresh();
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
                AddDialogueNode(action);
            }

            foreach (var condition in _dialogue.Conditions)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(condition.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                _graphView.AddNode(new DialogueConditionNode(_dialogue, nodeMetadata, condition));
            }

            foreach (var jump in _dialogue.Jumps)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(jump.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                _graphView.AddNode(new DialogueJumpNode(_dialogue, nodeMetadata, jump));
            }

            foreach (var output in _dialogue.Outputs)
            {
                var nodeMetadata = _editor.Graph.FindNodeByGuid(output.Guid, out var isNew);
                if (isNew) nodeMetadata.position = _graphView.GetViewportCenter();
                _graphView.AddNode(new DialogueOutputNode(_dialogue, nodeMetadata, output));
            }
        }

        private BasicNode AddDialogueNode(DialogueAction action, Vector3 offset = default)
        {
            var nodeMetadata = _editor.Graph.FindNodeByGuid(action.Guid, out var isNew);
            if (isNew) nodeMetadata.position = _graphView.GetViewportCenter() + offset;
            var node = new DialogueActionNode(_dialogue, nodeMetadata, action);
            _graphView.AddNode(node);
            return node;
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
