using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer containerCache;

    private List<Edge> edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        if (!SaveNodes(dialogueContainer)) return;

        SaveExposedProperties(dialogueContainer);

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Scripts/Dialogue System/Dialogue v2/Editor/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    private bool SaveNodes(DialogueContainer dialogueContainer)
    {
        if (!edges.Any()) return false;

        var connectedPorts = edges.Where(x => x.input.node != null).ToArray();
        for (var i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        foreach (var dialogueNode in nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.dialogueNodeData.Add(new DialogueNodeData
            {
                Guid = dialogueNode.GUID,
                dialogueText = dialogueNode.dialogueText,
                position = dialogueNode.GetPosition().position
            });
        }
        return true;
    }

    private void SaveExposedProperties(DialogueContainer dialogueContainer)
    {
        dialogueContainer.exposedProperties.AddRange(_targetGraphView.exposedProperties);
    }

    public void LoadGraph(string fileName)
    {
        containerCache = Resources.Load<DialogueContainer>(fileName);
        if (containerCache==null)
        {
            EditorUtility.DisplayDialog("File not found", "Target dialogue graph file does not exist", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void CreateExposedProperties()
    {
        _targetGraphView.ClearBlackboardAndExposedProperties();
        foreach (var exposedProperty in containerCache.exposedProperties)
        {
            _targetGraphView.AddPropertyToBlackboard(exposedProperty);
        }
    }

    private void ConnectNodes()
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodes[i].GUID).ToList();
            for (var j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(
                    containerCache.dialogueNodeData.First(x => x.Guid == targetNodeGuid).position,
                    _targetGraphView.defaultNodeSize
                    ));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in containerCache.dialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.dialogueText, Vector2.zero);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        nodes.Find(x => x.entryPoint).GUID = containerCache.nodeLinks[0].baseNodeGuid;

        foreach (var node in nodes)
        {
            if (node.entryPoint) continue;
            edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            _targetGraphView.RemoveElement(node);
        }
    }
}