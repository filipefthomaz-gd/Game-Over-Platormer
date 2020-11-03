using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ridiculous : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().velocity += Physics2D.gravity * 2 * Time.deltaTime;
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) < 5)
            {
                Debug.Log(GetComponent<Rigidbody2D>().velocity);
                GetComponent<Rigidbody2D>().velocity -= new Vector2(2, 0);
            }
            /*if (Mathf.Abs(rb.velocity.x) >= speed)
                rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);*/
        }
    }
}
