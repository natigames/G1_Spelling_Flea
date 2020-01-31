using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;

public class BGScroller : MonoBehaviour
{
    public static BGScroller instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.DOMoveX(1, 60);
    }

    public void moveRight()
    {
        transform.Translate(new Vector3(-500, 0, 0) * 2.0f * Time.deltaTime);
        if (transform.position.x < -1225)
        {
            transform.position = new Vector3(1225, transform.position.y, 0);
        }
    }


}
