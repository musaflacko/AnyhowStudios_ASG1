using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootEnemy : MonoBehaviour {
	public int distanceOfRay = 20; //raycast distance, or cursor point from camera to object
	private RaycastHit hit; //to receive what object is hit by the raycast	

	private int targetsHit;

	// Update is called once per frame
	void Update()
	{
		Shooter();
	}

	public void Shooter()
    {
		Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); //remove the raycast from the center point of the camera
																			   //Click Input
		Debug.DrawLine(transform.position, hit.point, Color.red);
		if (Physics.Raycast(ray, out hit, distanceOfRay))
		{ //when there is something from the raycast
			if (Input.GetButtonDown("Fire1") && hit.transform.CompareTag("Enemy"))//when something is an enemy tag and we press a button
			{
				Destroy(hit.transform.gameObject); //destroy gameobject;
				targetsHit += 1;
				Debug.Log("TargetHits: " + targetsHit);
			}
		}
	}

}
