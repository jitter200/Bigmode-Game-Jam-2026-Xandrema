using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Player
{
    [SerializeField] private Rigidbody2D rigidbody;
    private Transform Render;
    public float baseSpeed = 5f;
    public float maxSpeed = 10f;
    public float jump = 5f;
    public bool isGround;
    public float rayDistance = 0.6f;
    private float currentSpeed;
    private Vector3 moveDirection;


    private void Awake()
    {
        Render = transform.Find("Render");
    }

    void Start()
    {
        currentSpeed = ClampSpeed(baseSpeed, 0f, maxSpeed);
    }
    void Update()
    {
        RaycastHit2D foundGround = Physics2D.Raycast(rigidbody.position, new Vector2(0f,-1f), rayDistance, LayerMask.GetMask("Ground"));

        isGround = foundGround.collider != null ;
        
        if (Input.GetKeyDown(KeyCode.Space) && isGround == true)
        {
            rigidbody.AddForce(new Vector2(0, jump), ForceMode2D.Impulse);
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            Move(0f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Move(-1f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Move(1f);
        }
        if (Input.GetKey(KeyCode.S)) inputY -= 1f;
    }

}
