using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Covid : MonoBehaviour
{

    public int score;
    public float speed = 2;
    float i = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(i * speed * Time.deltaTime, 0));
    }



    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "edge")
        {
            Destroy(gameObject);
            score += 1;
            Debug.Log(score);
        }
    }

}
