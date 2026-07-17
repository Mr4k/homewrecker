using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    public Rigidbody Rigidbody { get => _rigidbody; }

    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Color _baseColor;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponentInChildren<Renderer>();
        _baseColor = _renderer.material.color;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    private void SetHighlight(bool on)
    {
        _renderer.material.color = on ? HighlightColor : _baseColor;
    }
}
