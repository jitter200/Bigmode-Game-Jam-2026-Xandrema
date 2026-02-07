using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 1.5f;
    public float steering = 8f;

    [Header("Shooting")]
    public GameObject bulletPrefab;   
    public float shootRange = 7f;
    public float shootCooldown = 1.2f;

    private float _t;
    private Transform _player;

    private void Start()
    {
        var p = FindFirstObjectByType<Player>();
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        _t -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > shootRange) return;
        if (_t > 0f) return;

        Shoot();
        _t = shootCooldown;
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector2 dir = (_player.position - transform.position).normalized;

        GameObject go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // если на пуле есть Bullet.cs Ч запускаем полЄт
        var b = go.GetComponent<Bullet>();
        if (b != null) b.Fire(dir);
        else
        {
            // запасной вариант: просто скорость
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * 8f;
        }
    }
}
