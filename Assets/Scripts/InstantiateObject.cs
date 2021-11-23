using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateObject : MonoBehaviour
{
    public GameObject flyingSaucer;
    public GameObject virus;
    public static int score;

    public GameObject gameplay;
    public GameObject poster;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Inst());
    }

    // Update is called once per frame
    void Update()
    {
        CheckScore();
    }
    IEnumerator Inst()
    {
        yield return new WaitForSeconds(2f);
        flyingSaucer.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < 10; i++)
        {
            Instantiate(virus, transform.position, transform.rotation);
            yield return new WaitForSeconds(5f);
        }
        flyingSaucer.SetActive(false);
    }

    public void CheckScore()
    {
        if(score == 5)
        {
            poster.SetActive(true);
            gameplay.SetActive(false);
        }
    }
}