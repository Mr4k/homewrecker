using Unity.VisualScripting;
using UnityEngine;

public class ArrayBlueprint : BaseBlueprint
{
    GameObject BaseObjectPrefab;

    Vector3 PerAxisSpacing = Vector3.one;

    Vector3Int NumRepeatsPerAxis = Vector3Int.one;

    public override void RefreshBlueprint()
    {
        var outputContainer = GetComponentInChildren<BlueprintOutputContainer>();
        if (outputContainer != null)
        {
            DestroyImmediate(outputContainer.gameObject);
        }
        var containerGameObject = new GameObject("BlueprintOutputContainer", typeof(BlueprintOutputContainer));
        for (int x = 0; x < NumRepeatsPerAxis.x; x++)
        {
            for (int y = 0; y < NumRepeatsPerAxis.y; y++)
            {
                for (int z = 0; z < NumRepeatsPerAxis.z; z++)
                {
                    var objPos = new Vector3(x, y, z);
                    objPos.Scale(PerAxisSpacing);
                    var obj = Instantiate(BaseObjectPrefab, containerGameObject.transform);
                    obj.transform.position = objPos;
                }
            }
        }
    }
}
