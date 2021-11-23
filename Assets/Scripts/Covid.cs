using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Covid : MonoBehaviour
{
    public float speed = 2;
    float i = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        transform.Translate(new Vector2(i * speed * Time.deltaTime, 0));
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Edge")
        {
            Destroy(gameObject);
            InstantiateObject.score += 1;
            Debug.Log(InstantiateObject.score);
        }
    }

    public void StartRun()
    {
        i = (Player.playerXpos - transform.position.x) / Mathf.Abs(Player.playerXpos - transform.position.x);
        //Debug.Log(i);
        //transform.rotation = new Quaternion(0, Mathf.Acos(i), 0, 0);
        GetComponent<SpriteRenderer>().flipX = i < 0;
    }

}