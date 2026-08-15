using System.Collections.Generic;
using UnityEngine;

public class SmashTool : BaseTool
{
    public float SmashRange = 5f;
    public override void ActiveToolUpdate(Transform cameraTransform)
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, SmashRange))
        {
            if (hit.collider && hit.collider.GetComponent<Smashable>())
            {
                var smashableGroupObjects = new List<Smashable>();
                var smashable = hit.collider.GetComponent<Smashable>();
                smashableGroupObjects.Add(smashable);
                var colliders = Physics.OverlapSphere(hit.point, 0.25f);
                foreach (var col in colliders)
                {
                    var otherSmashable = col.gameObject.GetComponent<Smashable>();
                    if (otherSmashable != null && otherSmashable != smashable && otherSmashable.SharesGroup(smashable))
                    {
                        smashableGroupObjects.Add(otherSmashable);
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("smashhhed!");
                    foreach (var s in smashableGroupObjects)
                    {
                        s.Smash(cameraTransform.position, 400);
                    }
                }
            }
        }
    }

    public override string GetName()
    {
        return "Smash Tool";
    }
}