using System;
using System.Collections.Generic;
using UnityEngine;

public class Gimbal : MonoBehaviour
{
    public enum RotationAxisName
    {
        X,
        Y,
        Z,
    }
    public Collider BlueXYCircle;
    public Collider RedYZCircle;
    public Collider GreenXZCircle;

    public static Gimbal Singleton;

    private Dictionary<int, Tuple<Vector3, RotationAxisName>> collidersAndNormals;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collidersAndNormals = new Dictionary<int, Tuple<Vector3, RotationAxisName>>
        {
            {BlueXYCircle.GetInstanceID(), Tuple.Create(Vector3.forward, RotationAxisName.Z)},
            {RedYZCircle.GetInstanceID(), Tuple.Create(Vector3.right, RotationAxisName.X)},
            {GreenXZCircle.GetInstanceID(), Tuple.Create(Vector3.up, RotationAxisName.Y)}
        };
        Singleton = this;
    }

    public class TangentResult
    {
        public Vector3 WorldSpacePoint;
        public Vector3 WorldSpaceTangent;
        public Vector3 WorldSpaceNormal;
        public RotationAxisName LocalAxis;
    }

    public TangentResult GetTangent(Collider collider, Vector3 worldPoint)
    {
        var localPoint = transform.worldToLocalMatrix.MultiplyPoint(worldPoint);
        var tuple = collidersAndNormals[collider.GetInstanceID()];
        var localPlaneNormal = tuple.Item1;
        var axis = tuple.Item2;
        var localPointOnPlane = Vector3.ProjectOnPlane(localPoint, localPlaneNormal);
        var localNormalFromCenter = localPoint.normalized;
        var localTangent = Vector3.Cross(localPlaneNormal, localNormalFromCenter);
        return new TangentResult()
        {
            WorldSpacePoint = transform.localToWorldMatrix.MultiplyPoint(localPointOnPlane),
            WorldSpaceTangent = transform.localToWorldMatrix.MultiplyVector(localTangent),
            WorldSpaceNormal = transform.localToWorldMatrix.MultiplyVector(localPlaneNormal),
            LocalAxis = axis
        };
    }
}
