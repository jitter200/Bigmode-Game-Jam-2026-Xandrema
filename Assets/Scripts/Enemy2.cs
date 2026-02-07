using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy2 : MonoBehaviour
{
    
    public float cooldown = 0.4f;

    private float _timer;

    private void Update()
    {
        _timer -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_timer > 0f) return;

        Player p = other.GetComponent<Player>();
        if (p == null) return;

        
        _timer = cooldown;
    }
}
