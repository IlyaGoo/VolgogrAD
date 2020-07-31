using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostrController : MonoBehaviour {

    Vector3 startPos;
    [SerializeField] Vector3 changing = new Vector3(0,-1f, -0.01f);
    Vector3 currentPos;

    void Start () {
        startPos = transform.position;
        ReInitPos();
    }
	
	// Update is called once per frame
	public Vector3 AddOne()
    {
        currentPos += changing;
        return currentPos;
    }

    public void ReInitPos()
    {
        currentPos = startPos - changing;
    }
}
