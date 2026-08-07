using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    public Rigidbody Rigidbody { get => _rigidbody; }

    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Color _baseColor;
    private bool _dragged;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponentInChildren<Renderer>();
        _baseColor = _renderer.material.color;
    }

    private void Update()
    {

    }

    public virtual void BeginDrag()
    {
        _dragged = true;
    }

    public virtual void EndDrag()
    {
        _dragged = false;
    }

    public virtual void OnDrag(Vector3 dragDirection) { }

    private void SetHighlight(bool on)
    {
        _renderer.material.color = on ? HighlightColor : _baseColor;
    }
}
