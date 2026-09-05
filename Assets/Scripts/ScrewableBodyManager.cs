using UnityEngine;

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