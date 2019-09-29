using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlapPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb2D;
    public float thrust;
    public Transform player;
    void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        transform.position = new Vector3(0.0f, -2.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key was pressed.");
            rb2D.AddForce(transform.up * thrust, ForceMode2D.Impulse);
        }

        if (Mathf.Abs(rb2D.velocity.y) > thrust)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, rb2D.velocity.y > 0 ? thrust : thrust * -1);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Space key was released.");
        }
    }

}

