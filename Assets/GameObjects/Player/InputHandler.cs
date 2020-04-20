using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    InputObject inputObject;
    TCPClient tcpClient;

    public string mode;

    void Start()
    {   
        inputObject = new InputObject();
        tcpClient = GameObject.Find("Main Camera").GetComponent<TCPClient>();
    }
    
    public InputObject GetInputObject() {
        return inputObject;
    }

    public void setInputObject(InputObject inputObject) {
        this.inputObject = inputObject;
        Debug.Log(transform.position);
        transform.position = new Vector3(this.inputObject.posX, this.inputObject.posY, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == "local") {
            inputObject.jumpHeld = Input.GetButton("Jump");
            inputObject.jumpPressed = Input.GetButtonDown("Jump");
            inputObject.dashPressed = Input.GetButtonDown("Dash");
            inputObject.inputX = Input.GetAxis("Horizontal");
            inputObject.inputY = Input.GetAxis("Vertical");
            inputObject.posX = transform.position.x;
            inputObject.posY = transform.position.y;
            if (tcpClient != null)
                tcpClient.netUpdate(inputObject);
        }
    }
}

[System.Serializable]
public class InputObject {
    public bool jumpPressed;
    public bool jumpHeld;

    public bool dashPressed;
    public float inputX;
    public float inputY;
    public float posX;
    public float posY;
    public InputObject() {
        dashPressed = false;
        jumpPressed = false;
        inputX = 0;
        inputY = 0;
        posX = 0;
        posY = 0;
    }
}