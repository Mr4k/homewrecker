using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ScrewableBody : MonoBehaviour
{
    public static int NextScrewableBodyId = 0;

    public HashSet<Screw> AttachedScrews = new HashSet<Screw>();
    public int Id = -1;

    private void Awake()
    {
        Id = NextScrewableBodyId;
        NextScrewableBodyId++;
    }

    public Rigidbody GetRigidbody()
    {
        return GetComponent<Rigidbody>();
    }
}