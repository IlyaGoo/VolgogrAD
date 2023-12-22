using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Arrow : NetworkBehaviourExtension {
    [SerializeField] private GameObject arrow;
    [SerializeField] private float radius = 2;
    [SerializeField] Transform arrowRender;

    // Use this for initialization
    void Start () {
        if (!isLocalPlayer) enabled = false;
        arrowRender = arrow.transform;
    }
	
	// Update is called once per frame
	void Update () {
        var targetTransform = taskManager.GetTargetTransform();
        if (targetTransform == null)
        {
            arrow.SetActive(false);
        }
        else
        {
            var pos2 = targetTransform.position;
            arrow.SetActive(true);
            float a = Mathf.Abs(transform.position.y - pos2.y);
            float b = Mathf.Abs(transform.position.x - pos2.x);
            float c = Mathf.Sqrt(a * a + b * b);
            if (c < 2) {arrow.SetActive(false); return; };
            float t1 = a * radius / c;
            float t2 = b * radius / c;

            int tt1 = pos2.y > transform.position.y ? 1 : -1;
            int tt2 = pos2.x > transform.position.x ? 1 : -1;

            float angle = Mathf.Asin(a/c)/Mathf.PI*180;

            float dopangle = pos2.x > transform.position.x ? pos2.y > transform.position.y ? angle : 360 - angle : pos2.y > transform.position.y ? 180 - angle : 180 + angle;

            arrowRender.transform.localScale = new Vector3(transform.localScale.x, 1, 1);

            arrow.transform.rotation = Quaternion.Euler(0, 0, dopangle);

            arrow.transform.position = new Vector3(transform.position.x + (t2 * tt2), transform.position.y + (t1 * tt1), arrow.transform.position.z);
        }

    }
}
