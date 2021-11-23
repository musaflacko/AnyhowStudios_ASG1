using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateObject : MonoBehaviour
{
    public GameObject ufo;
    public GameObject virus;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Inst());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Inst()
    {
        yield return new WaitForSeconds(2f);
        ufo.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        for(int i = 0; i<5; i++)
        {
            Instantiate(virus, transform.position, transform.rotation);
            yield return new WaitForSeconds(5f);
        }
        ufo.SetActive(false);

    }
}
