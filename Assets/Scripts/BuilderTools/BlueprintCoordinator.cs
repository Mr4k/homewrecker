using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlueprintCoordinator : MonoBehaviour
{
    private static Queue<Action> blueprintRefreshesForNextFixedUpdate = new Queue<Action>();

    void Update()
    {
        while (blueprintRefreshesForNextFixedUpdate.Count > 0)
        {
            var refresh = blueprintRefreshesForNextFixedUpdate.Dequeue();
            refresh.Invoke();
        }
    }

    public static void QueueRefresh(Action refresh)
    {
        blueprintRefreshesForNextFixedUpdate.Enqueue(refresh);
    }
}