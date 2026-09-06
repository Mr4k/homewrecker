using UnityEngine;

public class SliceableBodyManager : MonoBehaviour
{
    public Sliceable SliceableObjectPrefab;

    void Start()
    {
        _singleton = this;
    }

    public static GameObject GetSliceablePrefab()
    {
        return _singleton.SliceableObjectPrefab.gameObject;
    }

    private static SliceableBodyManager _singleton;
}