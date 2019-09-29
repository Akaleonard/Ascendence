using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float cameraSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z); // Camera follows the player with specified offset position   
        float overallDistance = Mathf.Sqrt(Mathf.Pow(player.position.x - transform.position.x, 2) + Mathf.Pow(player.position.y - transform.position.y, 2));
        float angle = Mathf.Atan2((player.position.y - transform.position.y), (player.position.x - transform.position.x));
        float cangle = Mathf.Cos(angle);
        float sangle = Mathf.Sin(angle);
        transform.position = new Vector3(transform.position.x + overallDistance / cameraSpeed * cangle, transform.position.y + overallDistance / cameraSpeed * sangle, offset.z);
    }
}
