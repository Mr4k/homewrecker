using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEditor;

public abstract class BaseBlueprint : MonoBehaviour
{
    public abstract void RefreshBlueprint();
}

// The second argument in the EditorToolAttribute flags this as a Component tool. That means that it will be instantiated
// and destroyed along with the selection. EditorTool.targets will contain the selected objects matching the type.
[EditorTool("BlueprintTool", typeof(BaseBlueprint))]
class BlueprintTool : EditorTool
{
    // Global tools (tools that do not specify a target type in the attribute) are lazy initialized and persisted by
    // a ToolManager. Component tools (like this example) are instantiated and destroyed with the current selection.
    void OnEnable()
    {
        // Allocate unmanaged resources or perform one-time set up functions here
    }

    void OnDisable()
    {
        // Free unmanaged resources, state teardown.
    }

    // The second "context" argument accepts an EditorWindow type.
    [Shortcut("Activate Blueprint Tool", typeof(SceneView), KeyCode.P)]
    static void BlueprintToolShortcut()
    {
        if (Selection.GetFiltered<BaseBlueprint>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<BlueprintTool>();
        else
            Debug.Log("No blueprints selected!");
    }

    // Called when the active tool is set to this tool instance. Global tools are persisted by the ToolManager,
    // so usually you would use OnEnable and OnDisable to manage native resources, and OnActivated/OnWillBeDeactivated
    // to set up state. See also `EditorTools.{ activeToolChanged, activeToolChanged }` events.
    public override void OnActivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Blueprint Tool"), .1f);
    }

    // Called before the active tool is changed, or destroyed. The exception to this rule is if you have manually
    // destroyed this tool (ex, calling `Destroy(this)` will skip the OnWillBeDeactivated invocation).
    public override void OnWillBeDeactivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Blueprint Tool"), .1f);
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;

        Handles.BeginGUI();
        using (new GUILayout.HorizontalScope())
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button("Refresh Blueprint"))
                {
                    foreach (var obj in targets)
                    {
                        if (obj is BaseBlueprint blueprint)
                        {
                            blueprint.RefreshBlueprint();
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();
        }
        Handles.EndGUI();
    }
}