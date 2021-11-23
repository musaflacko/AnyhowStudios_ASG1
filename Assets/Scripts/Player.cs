using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D playerRig;
    float _speed = 0;
    SpriteRenderer spriteRenderer;
    public float speed;
    public static float playerXpos;
    private bool moveLeft;
    private bool moveRight;


    // Start is called before the first frame update
    void Start()
    {
        playerRig = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        speed = 10f;
        moveLeft = false;
        moveRight = false;
    
    }

    private void Update()
    {
        if (moveLeft)
        {
            playerRig.velocity = new Vector2(-speed, 0f);
        }

        if (moveRight)
        {
            playerRig.velocity = new Vector2(speed, 0f);
        }
    }

    public void MoveLeft()
    {
        moveLeft = true;
    }
    public void MoveRight()
    {
        moveRight = true;
    }
    public void StopMoving()
    {
        moveRight = false;
        moveLeft = false;
        playerRig.velocity = Vector2.zero;
    }
}