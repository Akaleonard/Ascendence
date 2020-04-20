using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passthrough : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D (Collider2D jumper) {
        //set jumper's layer to something that can pass through the platform
        Debug.Log ("ping");
        if (jumper.gameObject.GetComponent<Rigidbody2D>().velocity.y > 0) {
            gameObject.layer = 9;
        }
    }
     
    void OnTriggerExit2D (Collider2D jumper) {
        //reset jumper's layer to something that the platform collides with
        Debug.Log ("pong");
        gameObject.layer = 8;
    }

}
