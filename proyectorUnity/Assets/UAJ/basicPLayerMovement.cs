using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicPLayerMovement : MonoBehaviour
{
    [SerializeField]
    float speed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

 

        gameObject.transform.position += new Vector3(horizontalInput, verticalInput) * speed;
    }
}
