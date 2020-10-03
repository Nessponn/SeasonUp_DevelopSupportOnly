using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDetectorPallarel : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D Hit)
    {
        if (Hit.GetComponent<Rigidbody2D>() != null)
        {
            transform.parent.GetComponent<WaterPallarel>().Splash(transform.position.x, 
                Mathf.Abs(Hit.GetComponent<Rigidbody2D>().velocity.x * Hit.GetComponent<Rigidbody2D>().gravityScale - Hit.GetComponent<Rigidbody2D>().velocity.x * Hit.GetComponent<Rigidbody2D>().mass) / 40f,
                Mathf.Abs(Hit.GetComponent<Rigidbody2D>().velocity.y * Hit.GetComponent<Rigidbody2D>().gravityScale - Hit.GetComponent<Rigidbody2D>().velocity.y * Hit.GetComponent<Rigidbody2D>().mass) / 40f);
            //Debug.Log("反応あり、JobSystem版");
        }
    }

    private void OnTriggerExit2D(Collider2D Hit)
    {
        //Hit.GetComponent<Rigidbody2D>().velocity = new Vector2(Hit.GetComponent<Rigidbody2D>().velocity.x / 8, Hit.GetComponent<Rigidbody2D>().velocity.y/2 );
        //Debug.Log("沼！！！");
        if (Hit.GetComponent<Rigidbody2D>() != null)
        {
            transform.parent.GetComponent<WaterPallarel>().Splash(transform.position.x, 1 + Mathf.Abs(Hit.GetComponent<Rigidbody2D>().velocity.x * Hit.GetComponent<Rigidbody2D>().gravityScale - Hit.GetComponent<Rigidbody2D>().velocity.x * Hit.GetComponent<Rigidbody2D>().mass) / 40f,
                Mathf.Abs(Hit.GetComponent<Rigidbody2D>().velocity.y * Hit.GetComponent<Rigidbody2D>().gravityScale - Hit.GetComponent<Rigidbody2D>().velocity.y * Hit.GetComponent<Rigidbody2D>().mass) / 40f);
            //Debug.Log("反応あり、JobSystem版");
        }
    }
}
