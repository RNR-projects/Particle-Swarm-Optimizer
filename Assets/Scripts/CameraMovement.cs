using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    private float baseSpeed = 0.06f;
    private float speed;
	private float horizontalAngle, verticalAngle;
    private float turnSpeed = 400f;

    void Update()
    {
        this.speed = this.baseSpeed * GlobalScaler.Instance().GetGlobalScale();
        Vector3 move = Vector3.zero;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            move.x = Input.GetAxis("Horizontal") * this.speed;
            move.z = Input.GetAxis("Vertical") * this.speed;
            this.transform.Translate(move);

            this.horizontalAngle += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * this.turnSpeed * Time.deltaTime;
            this.verticalAngle += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * this.turnSpeed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(-this.verticalAngle, this.horizontalAngle, 0);
        }
    }

    public void SetHorizontalAngle(float value)
    {
        this.horizontalAngle = value;
    }

    public void SetVerticalAngle(float value)
    {
        this.verticalAngle = value;
    }
}
