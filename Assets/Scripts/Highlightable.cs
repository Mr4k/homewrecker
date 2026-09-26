using UnityEngine;

public class Highlightable : MonoBehaviour
{
    private Color originalColor = Color.pink;

    public void Select()
    {
        var rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        rend.material.color = Color.yellow;
    }

    public void Deselect()
    {
        var rend = GetComponent<Renderer>();
        rend.material.color = originalColor;
    }
}
