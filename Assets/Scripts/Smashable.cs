using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Smashable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    public Rigidbody DebrisPrefab;
    private Renderer _renderer;
    private Color _baseColor;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _baseColor = _renderer.material.color;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    public void Smash(Vector3 originPoint, float smashForce)
    {
        var debris = Instantiate(DebrisPrefab, transform.position, transform.rotation, transform.parent);
        debris.transform.localScale = transform.localScale;
        Vector3 force = (transform.position - originPoint) * smashForce;
        debris.AddForce(force);
        Destroy(this.gameObject);
    }

    private void SetHighlight(bool on)
    {
        _renderer.material.color = on ? HighlightColor : _baseColor;
    }

    private void OnDrawGizmos()
    {
        //Debug.DrawRay(transform.position, transform.forward * 10f, Color.green);
    }
}
