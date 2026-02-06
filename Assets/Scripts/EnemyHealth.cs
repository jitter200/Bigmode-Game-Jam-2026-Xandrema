using UnityEngine;

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

