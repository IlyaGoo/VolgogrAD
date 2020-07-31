using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRangeMoving : MonoBehaviour {

    [SerializeField] float range = 1;
    [SerializeField] float speed = 0.15f;
    public Vector3 currentTarget;
    Vector3 startPos;

	// Use this for initialization
	void Start () {
        startPos = currentTarget = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 delta = currentTarget - transform.position;
        if (delta.magnitude > 0.1f)
        {
            delta.Normalize();
            float moveSpeed = speed * Time.deltaTime;
            transform.position = transform.position + (delta * moveSpeed);
        }
        else
        {
            var y = Random.Range(-range, range);
            currentTarget = startPos + new Vector3(Random.Range(-range, range), y, y / 100);
        }
    }
}
