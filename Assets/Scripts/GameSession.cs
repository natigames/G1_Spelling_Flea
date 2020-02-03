using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public AudioSource audioSource;
    static GameSession instance;

    private void Awake()
    {
        // notice the plural for SINGLETON Pattern (only 1)
        int gameStatusCount = FindObjectsOfType<GameSession>().Length;
        if (gameStatusCount > 1)
        {
            //make inactive first (best practice)
            gameObject.SetActive(false);

            //  if one already exists and I'm number two, destroy self...
            Destroy(gameObject);
        }
        else
        {
            // Don't destroy me, I need to prevail (I'm the first)
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        instance = this;
    }
}
