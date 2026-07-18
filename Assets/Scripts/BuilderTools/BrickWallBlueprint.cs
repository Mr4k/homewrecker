using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

class BrickWallBlueprint : MonoBehaviour
{
    public Transform BrickPrefab;
    public float HorizontalSpacingBetweenBricksSizeAsPercentageOfBrickSize = 0.1f;
    public float VerticalSpacingBetweenBricksSizeAsPercentageOfBrickSize = 0.1f;
    public Vector3 WallSize;
    public uint WallHeightInBricks = 3;
    public uint WallWidthInBricks = 4;

    private static int ToolVersion = 1;

    private Tuple<float, float> CalculateBrickWidthAndSpacerWidth()
    {
        int numHorizontalSpacers = (int)(WallWidthInBricks == 0 ? 0 : WallWidthInBricks - 1);
        if (numHorizontalSpacers == 0)
        {
            return Tuple.Create(WallSize.x, 0.0f);
        }
        float amountSpacerSpace = numHorizontalSpacers * HorizontalSpacingBetweenBricksSizeAsPercentageOfBrickSize;
        float amountBrick = WallWidthInBricks;
        float totalBrickWidth = WallSize.x * amountBrick / (amountBrick + amountSpacerSpace);
        float totalSpacerWidth = WallSize.x - totalBrickWidth;
        return Tuple.Create(totalBrickWidth / WallWidthInBricks, totalSpacerWidth / numHorizontalSpacers);
    }

    private Tuple<float, float> CalculateBrickHeightAndSpacerHeight()
    {
        int numVerticalSpacers = (int)(WallHeightInBricks == 0 ? 0 : WallHeightInBricks - 1);
        if (numVerticalSpacers == 0)
        {
            return Tuple.Create(WallSize.y, 0.0f);
        }
        float amountSpacerSpace = numVerticalSpacers * VerticalSpacingBetweenBricksSizeAsPercentageOfBrickSize;
        float amountBrick = WallHeightInBricks;
        float totalBrickHeight = WallSize.y * amountBrick / (amountBrick + amountSpacerSpace);
        float totalSpacerHeight = WallSize.y - totalBrickHeight;
        return Tuple.Create(totalBrickHeight / WallHeightInBricks, totalSpacerHeight / numVerticalSpacers);
    }

    public void RefreshWall()
    {
        Instantiate(BrickPrefab, this.gameObject.transform);
    }

    private float CalculateBrickDepth()
    {
        return WallSize.z;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, WallSize);
    }
}

// The second argument in the EditorToolAttribute flags this as a Component tool. That means that it will be instantiated
// and destroyed along with the selection. EditorTool.targets will contain the selected objects matching the type.
[EditorTool("BrickWallBlueprintTool", typeof(BrickWallBlueprint))]
class BrickWallBlueprintTool : EditorTool
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
    [Shortcut("Activate Brick Wall Blueprint Tool", typeof(SceneView), KeyCode.P)]
    static void BrickWallBlueprintToolShortcut()
    {
        if (Selection.GetFiltered<BrickWallBlueprint>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<BrickWallBlueprintTool>();
        else
            Debug.Log("No brick wall blueprints selected!");
    }

    // Called when the active tool is set to this tool instance. Global tools are persisted by the ToolManager,
    // so usually you would use OnEnable and OnDisable to manage native resources, and OnActivated/OnWillBeDeactivated
    // to set up state. See also `EditorTools.{ activeToolChanged, activeToolChanged }` events.
    public override void OnActivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Brick Wall Blueprint Tool"), .1f);
    }

    // Called before the active tool is changed, or destroyed. The exception to this rule is if you have manually
    // destroyed this tool (ex, calling `Destroy(this)` will skip the OnWillBeDeactivated invocation).
    public override void OnWillBeDeactivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Brick Wall Blueprint Tool"), .1f);
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
                if (GUILayout.Button("RefreshWall"))
                {
                    foreach (var obj in targets)
                    {
                        if (obj is BrickWallBlueprint blueprint)
                        {
                            blueprint.RefreshWall();
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();
        }
        Handles.EndGUI();
    }
}