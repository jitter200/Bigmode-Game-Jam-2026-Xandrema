using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8f;
    public float damage = 10f;
    public float lifeTime = 4f;
    public LayerMask hitMask;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    public void Fire(Vector2 dir)
    {
        _rb.linearVelocity = dir.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        Player p = other.GetComponent<Player>();
        if (p != null)
        {
            p.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
