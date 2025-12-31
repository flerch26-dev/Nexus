using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView graphView;
    private string fileName = "New Narrative";

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolBar();
        GenerateMiniMap();
        GenerateBlackBoard();
    }

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(graphView);
        blackboard.Add(new BlackboardSection { title = "Exposed Properties" });
        blackboard.addItemRequested = _blackboard => { graphView.AddPropertyToBlackboard(new ExposedProperty()); };
        blackboard.editTextRequested = (blackboard1, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField)element).text;
            if (graphView.exposedProperties.Any(x => x.propertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists, please choose another one !", "OK");
                return;
            }

            var propertyIndex = graphView.exposedProperties.FindIndex(x => x.propertyName == oldPropertyName);
            graphView.exposedProperties[propertyIndex].propertyName = newValue;
            ((BlackboardField)element).text = newValue;
        };

        blackboard.SetPosition(new Rect(10, 30, 200, 300));

        graphView.Add(blackboard);
        graphView.blackboard = blackboard;
    }

    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolBar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });

        rootVisualElement.Add(toolbar);
    }

    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap { anchored = true };
        var coords = graphView.contentViewContainer.WorldToLocal(new Vector2(position.width - 210, 30));
        miniMap.SetPosition(new Rect(coords.x, coords.y, 200, 140));
        graphView.Add(miniMap); 
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name", "Please enter a valid file name", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(graphView);

        if (save)
            saveUtility.SaveGraph(fileName);
        else
            saveUtility.LoadGraph(fileName);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
}