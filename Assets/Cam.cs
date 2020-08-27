using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour {
    public float speed = 0.30f;
	[HideInInspector]public float h, v, Hangle, Vangle;
    public float turnSpeed = 400f;
    CursorLockMode state;
	public bool isStatic;
	public GlobalScaler scaler;

    void Start () {
		scaler = GameObject.Find ("GlobalScaler").GetComponent<GlobalScaler> ();
    }

	void FixedUpdate () {
		speed = .30f * scaler.GlobalScale;
        Vector3 move = Vector3.zero;
		if (!isStatic) {
			h = Input.GetAxis("Horizontal");
			v = Input.GetAxis("Vertical");
			if (Cursor.lockState == CursorLockMode.Locked) {
			Hangle += Mathf.Clamp (Input.GetAxis ("Mouse X"), -1, 1) * turnSpeed * Time.deltaTime;
			Vangle += Mathf.Clamp (Input.GetAxis ("Mouse Y"), -1, 1) * turnSpeed * Time.deltaTime;
			}
		}
        this.transform.rotation = Quaternion.Euler(-Vangle, Hangle, 0);
        if (h != 0 || v != 0)
        {
            move.x = h * speed;
            move.z = v * speed;
            this.transform.Translate(move);
        } 
    }
}
