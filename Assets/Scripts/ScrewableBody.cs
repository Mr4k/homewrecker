using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ScrewableBody : MonoBehaviour
{
    public HashSet<Screw> AttachedScrews = new HashSet<Screw>();

    public Rigidbody GetRigidbody()
    {
        return GetComponent<Rigidbody>();
    }
}