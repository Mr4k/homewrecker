using System;
using UnityEngine;
public abstract class BaseTool : MonoBehaviour
{
    public abstract String GetName();
    public abstract void ActiveToolUpdate(Camera camera);

    public virtual void ActiveToolFixedUpdate(FirstPersonCharacterController character) { }

    public virtual void ToolSelected() { }

    public virtual void ToolDeselected() { }
}