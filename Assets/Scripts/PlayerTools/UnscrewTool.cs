using System;
using System.Collections.Generic;
using UnityEngine;

public class UnscrewTool : BaseTool
{
    public float UnscrewRange = 5;
    private HashSet<Screw> lastHighlightedScrews = new HashSet<Screw>();
    public override void ActiveToolUpdate(Camera camera)
    {
        Transform cameraTransform = camera.transform;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, UnscrewRange))
        {
            var screw = hit.collider.GetComponentInParent<Screw>();
            if (screw != null)
            {
                Debug.Log("screew");
                if (Input.GetMouseButtonDown(0))
                {
                    screw.Unscrew();
                }
            }
        }
    }

    public override string GetName()
    {
        return "Unscrewler";
    }
}
