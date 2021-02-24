using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Camera mainCamera;
    private KeyCode leftKey = KeyCode.Q;
    private KeyCode rightKey = KeyCode.D;
    private KeyCode frontKey = KeyCode.Z;
    private KeyCode backKey = KeyCode.S;
    private float speed = 3f;
    private float sensitivity = 3000000f;
    private float cameraPitch = 0f;

    void Awake(){
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        mainCamera = GetComponentInChildren<Camera>();
    }

    void Update(){
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement(){
        Vector3 movement = Vector3.zero;
        if(Input.GetKey(rightKey))
            movement.x += 1;
        if(Input.GetKey(leftKey))
            movement.x -= 1;
        if(Input.GetKey(frontKey))
            movement.z += 1;
        if(Input.GetKey(backKey))
            movement.z -= 1;
        movement = Vector3.ClampMagnitude(movement,1) * speed * Time.deltaTime;
        movement = transform.rotation * movement;
        controller.Move(movement);
    }

    private void HandleRotation(){
        float yawAngle = (Input.GetAxis("Mouse X") / Screen.width) * sensitivity * Time.deltaTime;
        float pitchAngle = (Input.GetAxis("Mouse Y") / Screen.height) * sensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * yawAngle);

        cameraPitch -= pitchAngle;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        mainCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    public void Teleport(Vector3 newPos){
        controller.enabled = false;
        transform.position = newPos;
        controller.enabled = true;
    }
}
