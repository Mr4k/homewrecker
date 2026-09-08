using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ScrewableBodyManager : MonoBehaviour
{
    public float minMass = 1f;
    public float maxMass = 100;
    private static ScrewableBodyManager _instance;

    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        ScrewableBody.InitScrewableBodySystem(gameObject);
    }
    private void FixedUpdate()
    {
        ScrewableBody.RefreshDirtyBodyHierarchy();
    }

    public static float GetMaxMass()
    {
        return _instance.maxMass;
    }

    public static float GetMinMass()
    {
        return _instance.minMass;
    }
}