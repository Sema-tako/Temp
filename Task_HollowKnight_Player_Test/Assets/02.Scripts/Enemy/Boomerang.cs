using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public Animator anim;

    Vector3 nextPos;
    public float moveSpeed = 20f;
    float updownSpeed = 4f;
    float moveTimeX = 0f;
    float moveTimeY = 0f;
    void Awake()
    {
        //moveSpeed = 10f;
        Destroy(this.gameObject, 4f);
    }

    
    void Update()
    {
        //Debug.Log(transform.right);
        moveTimeX += Time.deltaTime * 50f;
        transform.Translate(transform.right * moveSpeed * Mathf.Cos((moveTimeX) * Mathf.Deg2Rad) * Time.deltaTime);
        moveTimeY += Time.deltaTime * 100f;
        transform.Translate(transform.up * updownSpeed * Mathf.Cos((moveTimeY) * Mathf.Deg2Rad) * Time.deltaTime);

    }

    public void Boome()
    {
        anim.SetTrigger("8_Boomerang");
    }
}
