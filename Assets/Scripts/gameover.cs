using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class gameover : MonoBehaviour
{
    public Text score;

    // Start is called before the first frame update
    void Start()
    {
        score.text = PlayerPrefs.GetFloat("score").ToString();        
    }

    public void quit()
    {
        Application.Quit();
    }

    public void restart()
    {
        PlayerPrefs.SetInt("currentword", 0);
        SceneManager.LoadScene("game");
    }

    public void mainmenu()
    {
        SceneManager.LoadScene("menu");
    }

}
