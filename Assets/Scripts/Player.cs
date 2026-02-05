using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public float minSpeed = 1.0f;
    public float maxSpeed = 10f;
    public float acceleration = 18f;
    public float weakDrag = 0.25f;
    public bool faceVelocity = true;
    public float speedTurnRateDeg = 260f;
    public float windMax = 100f;
    public float windDrainPerSec = 6f;
    public float howMuchGetPerKill = 14f;
    public float howMuchSetPerDamage = 0.15f;
    public bool dieAtZero = true;
    public float aoeRadius = 1.8f;
    public float aoeTick = 0.12f;
    public float aoeCenterDamage = 10f;
    [Range(0f, 1f)] public float aoeEdgeMultiplier = 0.2f;
    public LayerMask enemyMask;

   
    public bool drawGizmos = true;

    public float Wind01 => Mathf.Clamp01(_wind / windMax);

    private Rigidbody2D _rb;
    private float _wind;
    private float _aoeTimer;
    private ContactFilter2D _enemyFilter;
    private readonly Collider2D[] _hits = new Collider2D[64];


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _enemyFilter = new ContactFilter2D();
        _enemyFilter.useLayerMask = true;
        _enemyFilter.layerMask = enemyMask;
        _enemyFilter.useTriggers = true; 

        _wind = windMax;

        
        Vector2 direction = Random.insideUnitCircle;
        direction = (direction == Vector2.zero) ? Vector2.up : direction.normalized;
        _rb.linearVelocity = direction * Mathf.Max(minSpeed, 1f);
    }

    private void Update()
    {
        _wind -= windDrainPerSec * Time.deltaTime;

        if (dieAtZero && _wind <= 0f)
        {
            _wind = 0f;
            Die();
            return;
        }

        _aoeTimer += Time.deltaTime;
        if (_aoeTimer >= aoeTick)
        {
            _aoeTimer -= aoeTick;
            DoAoeTick();
        }
    }

    private void FixedUpdate()
    {
        
        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;

        if (kb != null)
        {
            if (kb.wKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed) input.y -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
            if (kb.aKey.isPressed) input.x -= 1f;
        }

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        
        Vector2 v = _rb.linearVelocity;

        float dt = Time.fixedDeltaTime;
        const float EPS = 0.001f;

        
        v = Vector2.Lerp(v, Vector2.zero, weakDrag * dt);

        float speed = v.magnitude;

        
        if (speed < minSpeed)
            speed = minSpeed;

        
        if (input.sqrMagnitude > EPS)
        {
            Vector2 currentDir = v.sqrMagnitude > EPS ? v.normalized : input;
            float maxTurn = speedTurnRateDeg * dt;

            Vector2 newDir = RotateToward(currentDir, input, maxTurn);
            v = newDir * speed;
        }

        
        if (v.magnitude > maxSpeed)
            v = v.normalized * maxSpeed;

        if (v.magnitude < minSpeed)
            v = v.normalized * minSpeed;

        
        _rb.linearVelocity = v;

        
        if (faceVelocity && v.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90f;
            _rb.MoveRotation(angle);
        }
    }


    private void DoAoeTick()
    {
        
        int count = Physics2D.OverlapCircle(transform.position, aoeRadius, _enemyFilter, _hits);
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _hits[i];
            if (col == null) continue;

            EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
            if (eh == null || eh.IsDead) continue;

            float dist = Vector2.Distance(transform.position, eh.transform.position);
            float t = Mathf.Clamp01(dist / aoeRadius);

            float mult = Mathf.Lerp(1f, aoeEdgeMultiplier, t);
            float dmg = aoeCenterDamage * mult;

            float dealt = eh.ApplyDamage(dmg);

            if (howMuchSetPerDamage > 0f && dealt > 0f)
                AddWind(dealt * howMuchSetPerDamage);

            if (eh.IsDead)
                AddWind(howMuchGetPerKill);
        }
    }


    public void AddWind(float amount)
    {
        if (amount <= 0f) return;
        _wind = Mathf.Clamp(_wind + amount, 0f, windMax);
    }

    private void Die()
    {
        Debug.Log("Player died");
        enabled = false;
        _rb.linearVelocity = Vector2.zero;
    }

    private static Vector2 RotateToward(Vector2 fromDir, Vector2 toDir, float maxDegrees)
    {
        float fromAng = Mathf.Atan2(fromDir.y, fromDir.x) * Mathf.Rad2Deg;
        float toAng = Mathf.Atan2(toDir.y, toDir.x) * Mathf.Rad2Deg;
        float newAng = Mathf.MoveTowardsAngle(fromAng, toAng, maxDegrees);
        float rad = newAng * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

public class EnemyHealth : MonoBehaviour
{
    public float maxHp = 30f;
    public float hp;
    public bool IsDead => hp <= 0f;

    private void Awake() => hp = maxHp;

    public float ApplyDamage(float amount)
    {
        if (IsDead || amount <= 0f) return 0f;

        float before = hp;
        hp = Mathf.Max(0f, hp - amount);
        float dealt = before - hp;

        if (IsDead) Destroy(gameObject);
        return dealt;
    }
}
