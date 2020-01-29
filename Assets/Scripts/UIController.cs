using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public Text score;
    public Text level;
    public Text feedback;

    //Singleton
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        updateScore(0);
        updateLevel(1);
        showFeedback("");
    }


    public void updateScore(int points)
    {
        int oldscore = int.Parse(score.text);
        int newscore = oldscore + points;
        score.text = newscore.ToString();
    }

    public void updateLevel(int current)
    {
        level.text = "Level " + current.ToString();
    }

    public void showFeedback(string word)
    {
        feedback.text = word;
    }

}