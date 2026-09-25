using UnityEngine;

public class Highlightable : MonoBehaviour
{
    public void Select()
    {
        var rend = GetComponent<Renderer>();
        rend.material.color = Color.yellow;
    }

    public void Deselect()
    {
        var rend = GetComponent<Renderer>();
        rend.material.color = Color.white;
    }
}
