using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Smashable : MonoBehaviour
{
    public Color HighlightColor = Color.yellow;
    public Rigidbody DebrisPrefab;
    private Renderer _renderer;
    private Color _baseColor;
    private int groupId;

    public void Init(int groupId)
    {
        this.groupId = groupId;
    }

    public bool SharesGroup(Smashable other)
    {
        return other.groupId == groupId;
    }

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        //_baseColor = _renderer.material.color;
    }

    public virtual void Smash(Vector3 originPoint, float smashForce)
    {
        var debris = Instantiate(DebrisPrefab, transform.position, transform.rotation);
        debris.transform.localScale = transform.localScale;
        Vector3 force = (transform.position - originPoint).normalized * smashForce;
        debris.AddForce(force);
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        //Debug.DrawRay(transform.position, transform.forward * 10f, Color.green);
    }
}
