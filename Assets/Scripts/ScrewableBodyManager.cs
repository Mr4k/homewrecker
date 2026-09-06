using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ScrewableBodyManager : MonoBehaviour
{
    private void Start()
    {
        ScrewableBody.InitScrewableBodySystem(gameObject);
    }
    private void FixedUpdate()
    {
        ScrewableBody.RefreshDirtyBodyHierarchy();
    }
}