using System;
using UnityEngine;

public class FireInternal : MonoBehaviour
{
    public LayerMask AcceptableLayers;

    public float Size = 1;
    public float BaseRadius = 1;
    public float BaseHeight = 1;

    public CapsuleCollider FireOuterShellCollider;
    public ParticleSystem FireParticleSystem;

    void UpdateSize()
    {
        float newRadius = Size * BaseRadius;
        FireOuterShellCollider.radius = newRadius;
        var myCollider = GetComponent<CapsuleCollider>();
        myCollider.radius = newRadius;
        var shape = FireParticleSystem.shape;
        shape.radius = newRadius;
    }

    void OnTriggerStay(Collider other)
    {
        var myCollider = GetComponent<Collider>();
        float distance;
        var intersects = Physics.ComputePenetration(myCollider, transform.position, transform.rotation,
        other, other.transform.position, other.transform.rotation, out _, out distance);
        if (intersects && distance > Math.Max(Math.Max(other.bounds.extents.x, other.bounds.extents.y), other.bounds.extents.z))
        {
            Destroy(other.gameObject);
            Size += 0.2f;
            UpdateSize();
        }
    }
}