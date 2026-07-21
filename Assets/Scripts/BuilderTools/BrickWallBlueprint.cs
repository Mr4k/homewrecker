using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

class BrickWallBlueprint : BaseBlueprint
{
    public Smashable BrickPrefab;
    public float HorizontalSpacingBetweenBricksSizeAsPercentageOfBrickSize = 0.1f;
    public float VerticalSpacingBetweenBricksSizeAsPercentageOfBrickSize = 0.1f;
    public Vector3 WallSize;
    public uint WallHeightInBricks = 3;
    public uint WallWidthInBricks = 4;

    private static int ToolVersion = 1;

    private Tuple<float, float, float> CalculateBrickWidthAndSpacerWidth()
    {
        int numHorizontalSpacers = (int)(WallWidthInBricks == 0 ? 0 : WallWidthInBricks - 1);
        if (numHorizontalSpacers == 0)
        {
            return Tuple.Create(WallSize.x, 0.0f, 0.0f);
        }
        float amountSpacerSpace = numHorizontalSpacers * HorizontalSpacingBetweenBricksSizeAsPercentageOfBrickSize;
        float amountBrick = WallWidthInBricks;
        float totalBrickWidth = WallSize.x * amountBrick / (amountBrick + amountSpacerSpace);
        float totalSpacerWidth = WallSize.x - totalBrickWidth;
        return Tuple.Create(totalBrickWidth / WallWidthInBricks, totalSpacerWidth / numHorizontalSpacers, totalSpacerWidth / (numHorizontalSpacers + 1));
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

    public override void RefreshBlueprint()
    {
        var outputContainer = GetComponentInChildren<BlueprintOutputContainer>();
        if (outputContainer != null)
        {
            DestroyImmediate(outputContainer.gameObject);
        }
        var containerGameObject = new GameObject("BlueprintOutputContainer", typeof(BlueprintOutputContainer));
        containerGameObject.transform.SetParent(transform);
        containerGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        int groupId = containerGameObject.GetInstanceID();
        (float brickWidth, float spacerWidth, float spacerWidthHalf) = CalculateBrickWidthAndSpacerWidth();
        (float brickHeight, float spacerHeight) = CalculateBrickHeightAndSpacerHeight();
        float brickDepth = CalculateBrickDepth();
        float nextY = -WallSize.y / 2;
        for (int y = 0; y < WallHeightInBricks; y++)
        {
            float rowSpacerWidth = spacerWidth;
            float nextX = -WallSize.x / 2;
            uint numHorizontalBricks = WallWidthInBricks;
            if (WallWidthInBricks == 0)
            {
                break;
            }
            if (y % 2 == 1)
            {
                rowSpacerWidth = spacerWidthHalf;
            }
            if (y % 2 == 1)
            {
                // use a half brick at the start and end
                var halfBrick = Instantiate(BrickPrefab, containerGameObject.transform);
                halfBrick.Init(groupId);
                halfBrick.transform.localScale = new Vector3(brickWidth / 2, brickHeight, brickDepth);
                halfBrick.transform.localPosition = new Vector3(nextX + brickWidth / 4, nextY + brickHeight / 2, 0);
                nextX += brickWidth / 2 + rowSpacerWidth;
                numHorizontalBricks -= 1;
            }
            for (int x = 0; x < numHorizontalBricks; x++)
            {
                var brick = Instantiate(BrickPrefab, containerGameObject.transform);
                brick.Init(groupId);
                brick.transform.localScale = new Vector3(brickWidth, brickHeight, brickDepth);
                brick.transform.localPosition = new Vector3(nextX + brickWidth / 2, nextY + brickHeight / 2, 0);
                nextX += brickWidth + rowSpacerWidth;
            }
            if (y % 2 == 1)
            {
                // use a half brick at the start and end
                var halfBrick = Instantiate(BrickPrefab, containerGameObject.transform);
                halfBrick.Init(groupId);
                halfBrick.transform.localScale = new Vector3(brickWidth / 2, brickHeight, brickDepth);
                halfBrick.transform.localPosition = new Vector3(nextX + brickWidth / 4, nextY + brickHeight / 2, 0);
            }
            nextY += brickHeight + spacerHeight;
        }
    }

    private float CalculateBrickDepth()
    {
        return WallSize.z;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, WallSize);
    }
}