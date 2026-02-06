using UnityEngine;
using System;


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
    public float windDrainPerSec = 4f;
    public float howMuchGetPerKill = 24f;
    public float howMuchSetPerDamage = 0f;
    public bool dieAtZero = true;
    public float aoeRadius = 1.8f;
    public float aoeTick = 0.12f;
    public float aoeCenterDamage = 10f;
    public float maxHp = 100f;
    public event Action Died;
    [SerializeField] private float hp;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashTime = 0.1f;
    [SerializeField] private float shakeAmount = 0.05f;
    private Vector3 _baseLocalPos;
    private Transform _spriteTf;
    private Vector3 _spriteBaseLocalPos;


    private Color _baseColor;
    private float _flashTimer;
    [Range(0f, 1f)] public float aoeEdgeMultiplier = 0.2f;
    public LayerMask enemyMask;

   
    public bool drawGizmos = true;
    public float Hp01 => Mathf.Clamp01(hp / maxHp);
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
        hp = maxHp;
        


        Vector2 direction = UnityEngine.Random.insideUnitCircle;

        direction = (direction == Vector2.zero) ? Vector2.up : direction.normalized;
        _rb.linearVelocity = direction * Mathf.Max(minSpeed, 1f);     

        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        _spriteTf = sprite.transform;
        _spriteBaseLocalPos = _spriteTf.localPosition;

        _baseColor = sprite.color;


    }

    private void Update()
    {
        _wind -= windDrainPerSec * Time.deltaTime;

        _wind = Mathf.Max(0f, _wind);


        _aoeTimer += Time.deltaTime;
        if (_aoeTimer >= aoeTick)
        {
            _aoeTimer -= aoeTick;
            DoAoeTick();
        }

        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
                sprite.color = _baseColor;
        }

        float danger = Mathf.Max(1f - Hp01, 1f - Wind01);
        Color dangerTint = Color.Lerp(_baseColor, Color.yellow, danger * 0.6f);

        if (_flashTimer <= 0f)
            sprite.color = dangerTint;

        bool critical = Hp01 < 0.3f || Wind01 < 0.2f;

        if (critical)
        {
            Vector2 shake = UnityEngine.Random.insideUnitCircle * shakeAmount;
            _spriteTf.localPosition = _spriteBaseLocalPos + (Vector3)shake;
        }
        else
        {
            _spriteTf.localPosition = _spriteBaseLocalPos;
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

    private void OnValidate()
    {
        _enemyFilter.useLayerMask = true;
        _enemyFilter.layerMask = enemyMask;
        _enemyFilter.useTriggers = true;
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
        Debug.Log("You dead");
        Died?.Invoke();
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
    public void TakeWindDamage(float amount)
    {
        if (amount <= 0f) return;
        _wind = Mathf.Max(0f, _wind - amount);
    }
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (hp <= 0f) return;

        hp -= amount;

        if (hp <= 0f)
        {
            hp = 0f;
            Die();
        }
        sprite.color = damageColor;
        _flashTimer = damageFlashTime;

    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

