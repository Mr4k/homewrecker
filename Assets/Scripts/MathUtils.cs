using System;
using UnityEngine;

public class MathUtils
{
    public struct PlaneBasisForRadialCoordinateTransform
    {
        public Vector3 XAxis;
        public Vector3 YAxis;
    }
    public static PlaneBasisForRadialCoordinateTransform ComputePlaneBasisForRadialTransform(Vector3 start, Vector3 end, Vector3 cameraOrigin)
    {
        Vector3 originToStart = start - cameraOrigin;
        Vector3 originToEnd = end - cameraOrigin;
        Vector3 originToMidpoint = (originToStart + originToEnd) / 2;
        Vector3 normal = Vector3.Cross(originToStart, originToEnd);
        Vector3 xAxis = Vector3.Cross(originToMidpoint, normal);
        Vector3 yAxis = originToMidpoint;
        if (Vector3.Dot(xAxis, originToEnd) < 0)
        {
            // end should be positive along x-axis, start should be neg
            xAxis *= -1;
        }
        return new PlaneBasisForRadialCoordinateTransform()
        {
            XAxis = xAxis,
            YAxis = yAxis,
        };
    }

    // angle should range from -pi/2 to pi/2
    public static float PlanePointToAngle(PlaneBasisForRadialCoordinateTransform basis, Vector3 cameraOrigin, Vector3 point)
    {
        Vector3 v = point - cameraOrigin;
        float x = Vector3.Dot(basis.XAxis, v);
        float y = Vector3.Dot(basis.YAxis, v);
        return (float)(Math.Atan2(y, x) - Math.PI / 2.0);
    }
}