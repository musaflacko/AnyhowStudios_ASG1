using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    Rigidbody2D playerRig;
    float _speed = 0;
    SpriteRenderer spriteRenderer;
    public float speed = 10;
    public static float playerXpos;


    // Start is called before the first frame update
    void Start()
    {
        playerRig = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        playerXpos = transform.position.x;
    }

    public void FixedUpdate()
    {
        playerRig.velocity = new Vector2(_speed, playerRig.velocity.y);
    }

    public void RunR()
    {
        _speed = speed;
        spriteRenderer.flipX = true;
    }
    public void RunL()
    {
        _speed = -speed;
        spriteRenderer.flipX = false;
    }
    public void Stop()
    {
        _speed = 0; 
    }


}
