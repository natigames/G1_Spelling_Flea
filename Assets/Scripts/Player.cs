using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public static Player instance; //singleton to manage pushback

    private Animator anim;
    private SpriteRenderer theSR;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        theSR = GetComponent<SpriteRenderer>();
    }

    public void animCorrect()
    {
        anim.SetBool("isCorrect", true); // set Animator variable to animate jump!
    }

}
