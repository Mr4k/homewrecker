using System.Collections.Generic;
using UnityEngine;

public class Gimbal : MonoBehaviour
{
    public Collider BlueXYCircle;
    public Collider RedYZCircle;
    public Collider GreenXZCircle;

    public static Gimbal Singleton;

    private Dictionary<int, Vector3> collidersAndNormals;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collidersAndNormals = new Dictionary<int, Vector3>
        {
            {BlueXYCircle.GetInstanceID(), Vector3.forward},
            {RedYZCircle.GetInstanceID(), Vector3.right},
            {GreenXZCircle.GetInstanceID(), Vector3.up}
        };
        Singleton = this;
    }

    public class TangentResult
    {
        public Vector3 WorldSpacePoint;
        public Vector3 WorldSpaceTangent;
        public Vector3 WorldSpaceNormal;
    }

    public TangentResult GetTangent(Collider collider, Vector3 worldPoint)
    {
        var localPoint = transform.worldToLocalMatrix.MultiplyPoint(worldPoint);
        var localPlaneNormal = collidersAndNormals[collider.GetInstanceID()];
        var localPointOnPlane = Vector3.ProjectOnPlane(localPoint, localPlaneNormal);
        var localNormalFromCenter = localPoint.normalized;
        var localTangent = Vector3.Cross(localPlaneNormal, localNormalFromCenter);
        return new TangentResult()
        {
            WorldSpacePoint = transform.localToWorldMatrix.MultiplyPoint(localPointOnPlane),
            WorldSpaceTangent = transform.localToWorldMatrix.MultiplyVector(localTangent),
            WorldSpaceNormal = transform.localToWorldMatrix.MultiplyVector(localPlaneNormal),
        };
    }
}
