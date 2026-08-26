using System;
using System.Collections.Generic;
using UnityEngine;

// note that right now in this class (width, height, depth) has been swizzled to (width, depth, height)

class FloorBaseLayerBlueprint : BaseBlueprint
{
    public Sliceable WoodenPlankPrefab;
    public ScrewBlueprint ScrewBlueprintPrefab;
    public float HorizontalSpacingBetweenFloorboardSizeAsPercentageOfFloorboardSize = 0.1f;
    public float VerticalSpacingBetweenFloorboardSizeAsPercentageOfFloorboardSize = 0.1f;
    public Vector3 FloorSize;
    public float PercentageOfFloorDepthForSupportBeams = 0.9f;
    public float SupportBeamWidthAsAPercentageOfAsPercentageOfFloorboardWidth = 0.2f;
    public float PercentageOfFloorboardMinDimensionForScrewHoles = 0.15f;
    public float PercentageOfIntraFloorScrewSpaceAlongWidth = 0.75f;
    public float PercentageOfIntraFloorScrewSpaceAlongHeight = 0.9f;
    public uint FloorHeightInFloorboards = 3;
    public uint FloorWidthInFloorboards = 4;

    private static int ToolVersion = 1;

    private float GetScrewDiameter(float floorboardWidth, float floorboardHeight)
    {
        return PercentageOfFloorboardMinDimensionForScrewHoles / 2.0f * Math.Min(floorboardWidth, floorboardHeight);
    }

    private Tuple<float, float, float> CalculateFloorboardWidthAndSpacerWidth()
    {
        int numHorizontalSpacers = (int)(FloorWidthInFloorboards == 0 ? 0 : FloorWidthInFloorboards - 1);
        if (numHorizontalSpacers == 0)
        {
            return Tuple.Create(FloorSize.x, 0.0f, 0.0f);
        }
        float amountSpacerSpace = numHorizontalSpacers * HorizontalSpacingBetweenFloorboardSizeAsPercentageOfFloorboardSize;
        float amountFloorboard = FloorWidthInFloorboards;
        float totalFloorboardWidth = FloorSize.x * amountFloorboard / (amountFloorboard + amountSpacerSpace);
        float totalSpacerWidth = FloorSize.x - totalFloorboardWidth;
        return Tuple.Create(totalFloorboardWidth / FloorWidthInFloorboards, totalSpacerWidth / numHorizontalSpacers, totalSpacerWidth / (numHorizontalSpacers + 1));
    }

    private Tuple<float, float> CalculateFloorboardHeightAndSpacerHeight()
    {
        int numVerticalSpacers = (int)(FloorHeightInFloorboards == 0 ? 0 : FloorHeightInFloorboards - 1);
        if (numVerticalSpacers == 0)
        {
            return Tuple.Create(FloorSize.y, 0.0f);
        }
        float amountSpacerSpace = numVerticalSpacers * VerticalSpacingBetweenFloorboardSizeAsPercentageOfFloorboardSize;
        float amountFloorboard = FloorHeightInFloorboards;
        float totalFloorboardHeight = FloorSize.y * amountFloorboard / (amountFloorboard + amountSpacerSpace);
        float totalSpacerHeight = FloorSize.y - totalFloorboardHeight;
        return Tuple.Create(totalFloorboardHeight / FloorHeightInFloorboards, totalSpacerHeight / numVerticalSpacers);
    }

    private void CreateFloorBoardScrew(Transform screwBlueprintParent, ScrewableBody floorboardBody, ScrewableBody supportBeamBody, Vector3 localFloorboardCenter, float floorboardWidth, float floorboardHeight, int xSign, bool isHalf)
    {
        for (int ySign = -1; ySign < 2; ySign += 2)
        {
            var floorboardScrewBlueprint = Instantiate(ScrewBlueprintPrefab, screwBlueprintParent);
            var widthOffsetFromEdge = (1.0f - PercentageOfIntraFloorScrewSpaceAlongWidth) * floorboardWidth / 2.0f;
            var halfEdgeWidth = isHalf ? floorboardWidth / 4 : floorboardWidth / 2;
            var heightOffsetFromCenter = PercentageOfIntraFloorScrewSpaceAlongHeight * floorboardHeight / 2.0f;
            floorboardScrewBlueprint.Body1 = floorboardBody;
            floorboardScrewBlueprint.Body2 = supportBeamBody;
            floorboardScrewBlueprint.transform.localPosition = new Vector3(-xSign * halfEdgeWidth + xSign * widthOffsetFromEdge, ySign * heightOffsetFromCenter, -0.1f) + localFloorboardCenter;
            floorboardScrewBlueprint.ScrewDiameter = GetScrewDiameter(floorboardWidth, floorboardHeight);
            floorboardScrewBlueprint.RefreshBlueprint();
        }
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

        var floorboardContainerGameObject = new GameObject("BlueprintFloorboardContainer");
        floorboardContainerGameObject.transform.SetParent(containerGameObject.transform);
        floorboardContainerGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var supportBeamContainerGameObject = new GameObject("BlueprintSupportBeamContainer");
        supportBeamContainerGameObject.transform.SetParent(containerGameObject.transform);
        supportBeamContainerGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var screwContainerGameObject = new GameObject("BlueprintScrewContainer");
        screwContainerGameObject.transform.SetParent(containerGameObject.transform);
        screwContainerGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        (float FloorboardWidth, float spacerWidth, float spacerWidthHalf) = CalculateFloorboardWidthAndSpacerWidth();
        (float FloorboardHeight, float spacerHeight) = CalculateFloorboardHeightAndSpacerHeight();
        float FloorboardDepth = CalculateFloorboardDepth();

        // support beams
        var supportBeamDepth = FloorSize.z * PercentageOfFloorDepthForSupportBeams;
        var supportBeamsInOrder = new List<ScrewableBody>();

        // bottom slats for on rows
        {
            float nextX = -FloorSize.x / 2;
            // the + 1 is so we get the end as well
            for (int x = 0; x < FloorWidthInFloorboards + 1; x++)
            {
                var width = SupportBeamWidthAsAPercentageOfAsPercentageOfFloorboardWidth * FloorboardWidth;
                var centerAdj = 0.0f;
                if (x == 0)
                {
                    width /= 2;
                    centerAdj += width / 2;
                }
                if (x + 1 == FloorWidthInFloorboards + 1)
                {
                    width /= 2;
                    centerAdj -= width / 2;
                }
                var supportBeam = Instantiate(WoodenPlankPrefab, supportBeamContainerGameObject.transform);
                supportBeam.transform.localPosition = new Vector3(nextX + centerAdj, 0, FloorboardDepth + supportBeamDepth / 2f - FloorSize.z / 2);
                supportBeam.transform.localScale = new Vector3(width, FloorSize.y, supportBeamDepth);
                nextX += FloorboardWidth + spacerWidth;
                supportBeamsInOrder.Add(supportBeam.gameObject.GetComponent<ScrewableBody>());
            }
        }
        // bottom slats for off rows
        {
            float nextX = -FloorSize.x / 2 + FloorboardWidth / 2 + spacerWidthHalf;
            // here we don't want the first or the last floorboard
            for (int x = 0; x < FloorWidthInFloorboards; x++)
            {
                var width = SupportBeamWidthAsAPercentageOfAsPercentageOfFloorboardWidth * FloorboardWidth;
                var supportBeam = Instantiate(WoodenPlankPrefab, supportBeamContainerGameObject.transform);
                supportBeam.transform.localPosition = new Vector3(nextX, 0, FloorboardDepth + supportBeamDepth / 2f - FloorSize.z / 2);
                supportBeam.transform.localScale = new Vector3(width, FloorSize.y, supportBeamDepth);
                nextX += FloorboardWidth + spacerWidthHalf;
                supportBeamsInOrder.Add(supportBeam.gameObject.GetComponent<ScrewableBody>());
            }
        }
        supportBeamsInOrder.Sort((a, b) =>
        {
            return a.transform.localPosition.x.CompareTo(b.transform.localPosition.x);
        });


        // floorboards
        float nextY = -FloorSize.y / 2;
        for (int y = 0; y < FloorHeightInFloorboards; y++)
        {
            float rowSpacerWidth = spacerWidth;
            float nextX = -FloorSize.x / 2;
            uint numHorizontalFloorboards = FloorWidthInFloorboards;
            if (FloorWidthInFloorboards == 0)
            {
                break;
            }
            if (y % 2 == 1)
            {
                rowSpacerWidth = spacerWidthHalf;
            }
            var currSupportBeamIdx = 0;
            if (y % 2 == 1)
            {
                // use a half Floorboard at the start and end
                var halfFloorboard = Instantiate(WoodenPlankPrefab, floorboardContainerGameObject.transform);
                halfFloorboard.transform.localScale = new Vector3(FloorboardWidth / 2, FloorboardHeight, FloorboardDepth);
                halfFloorboard.transform.localPosition = new Vector3(nextX + FloorboardWidth / 4, nextY + FloorboardHeight / 2, -FloorSize.z / 2 + FloorboardDepth / 2);
                nextX += FloorboardWidth / 2 + rowSpacerWidth;
                numHorizontalFloorboards -= 1;
                CreateFloorBoardScrew(floorboardContainerGameObject.transform, halfFloorboard.GetComponent<ScrewableBody>(), supportBeamsInOrder[currSupportBeamIdx], halfFloorboard.transform.localPosition, FloorboardWidth, FloorboardHeight, -1, true);
                CreateFloorBoardScrew(floorboardContainerGameObject.transform, halfFloorboard.GetComponent<ScrewableBody>(), supportBeamsInOrder[currSupportBeamIdx + 1], halfFloorboard.transform.localPosition, FloorboardWidth, FloorboardHeight, 1, true);
                currSupportBeamIdx += 1;
            }
            for (int x = 0; x < numHorizontalFloorboards; x++)
            {
                var Floorboard = Instantiate(WoodenPlankPrefab, floorboardContainerGameObject.transform);
                Floorboard.transform.localScale = new Vector3(FloorboardWidth, FloorboardHeight, FloorboardDepth);
                Floorboard.transform.localPosition = new Vector3(nextX + FloorboardWidth / 2, nextY + FloorboardHeight / 2, -FloorSize.z / 2 + FloorboardDepth / 2);
                nextX += FloorboardWidth + rowSpacerWidth;
                CreateFloorBoardScrew(floorboardContainerGameObject.transform, Floorboard.GetComponent<ScrewableBody>(), supportBeamsInOrder[currSupportBeamIdx], Floorboard.transform.localPosition, FloorboardWidth, FloorboardHeight, -1, false);
                CreateFloorBoardScrew(floorboardContainerGameObject.transform, Floorboard.GetComponent<ScrewableBody>(), supportBeamsInOrder[currSupportBeamIdx + 2], Floorboard.transform.localPosition, FloorboardWidth, FloorboardHeight, 1, false);
                currSupportBeamIdx += 2;
            }
            if (y % 2 == 1)
            {
                // use a half Floorboard at the start and end
                var halfFloorboard = Instantiate(WoodenPlankPrefab, floorboardContainerGameObject.transform);
                halfFloorboard.transform.localScale = new Vector3(FloorboardWidth / 2, FloorboardHeight, FloorboardDepth);
                halfFloorboard.transform.localPosition = new Vector3(nextX + FloorboardWidth / 4, nextY + FloorboardHeight / 2, -FloorSize.z / 2 + FloorboardDepth / 2);
                CreateFloorBoardScrew(floorboardContainerGameObject.transform, halfFloorboard.GetComponent<ScrewableBody>(), supportBeamsInOrder[currSupportBeamIdx], halfFloorboard.transform.localPosition, FloorboardWidth, FloorboardHeight, -1, true);
                CreateFloorBoardScrew(floorboardContainerGameObject.transform, halfFloorboard.GetComponent<ScrewableBody>(), supportBeamsInOrder[currSupportBeamIdx + 1], halfFloorboard.transform.localPosition, FloorboardWidth, FloorboardHeight, 1, true);
            }
            nextY += FloorboardHeight + spacerHeight;
        }
    }

    private float CalculateFloorboardDepth()
    {
        return FloorSize.z * (1.0f - PercentageOfFloorDepthForSupportBeams);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, FloorSize);
    }
}