using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float steering = 12f;
    public float windDamagePerSecond = 6f;
    public float damagePerSecond = 15f;
    private Rigidbody2D _rb;
    private Transform _player;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    private void Start()
    {
        
        Player p = FindFirstObjectByType<Player>();
        if (p != null) _player = p.transform;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 toPlayer = (Vector2)(_player.position - transform.position);
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Vector2 desiredVel = toPlayer.normalized * moveSpeed;

        
        Vector2 newVel = Vector2.Lerp(_rb.linearVelocity, desiredVel, steering * Time.fixedDeltaTime);
        _rb.linearVelocity = newVel;
    }
    

    private void OnTriggerStay2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        player.TakeDamage(damagePerSecond * Time.deltaTime);
    }


}