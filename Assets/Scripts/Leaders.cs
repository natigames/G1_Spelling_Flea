using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Leaders : MonoBehaviour
{
    public Text name1;
    public Text name2;
    public Text name3;
    public Text name4;
    public Text name5;
    public Text name6;
    public Text name7;
    public Text name8;
    public Text name9;
    public Text name10;
    public Text yourname;
    public Text score1;
    public Text score2;
    public Text score3;
    public Text score4;
    public Text score5;
    public Text score6;
    public Text score7;
    public Text score8;
    public Text score9;
    public Text score10;
    public Text yourscore;
    public Text games1;
    public Text games2;
    public Text games3;
    public Text games4;
    public Text games5;
    public Text games6;
    public Text games7;
    public Text games8;
    public Text games9;
    public Text games10;
    public Text yourgames;
    public Text level;
    public Text pack;

    public leaderboard leaderboard;

    public void Exit()
    {
        SceneManager.LoadScene("menu");
    }

    // Fetch words and store them
    public IEnumerator getLeaders()
    {
        WWWForm form = new WWWForm();
        form.AddField("method", "getTop10");
        form.AddField("userid", PlayerPrefs.GetInt("userid"));
        form.AddField("pack", PlayerPrefs.GetInt("pack"));
        form.AddField("level", PlayerPrefs.GetString("level"));
        using (var w = UnityWebRequest.Post("http://nati.games/apis/spellingflea.cfc", form))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            { print(w.error); }
            else
            {
                var myString = w.downloadHandler.text;
                leaderboard = JsonUtility.FromJson<leaderboard>(myString);
                name1.text = leaderboard.name1;
                name2.text = leaderboard.name2;
                name3.text = leaderboard.name3;
                name4.text = leaderboard.name4;
                name5.text = leaderboard.name5;
                name6.text = leaderboard.name6;
                name7.text = leaderboard.name7;
                name8.text = leaderboard.name8;
                name9.text = leaderboard.name9;
                name10.text = leaderboard.name10;
                yourname.text = leaderboard.nameyou;
                score1.text = leaderboard.score1;
                score2.text = leaderboard.score2;
                score3.text = leaderboard.score3;
                score4.text = leaderboard.score4;
                score5.text = leaderboard.score5;
                score6.text = leaderboard.score6;
                score7.text = leaderboard.score7;
                score8.text = leaderboard.score8;
                score9.text = leaderboard.score9;
                score10.text = leaderboard.score10;
                yourscore.text = leaderboard.scoreyou;
                games1.text = leaderboard.total1;
                games2.text = leaderboard.total2;
                games3.text = leaderboard.total3;
                games4.text = leaderboard.total4;
                games5.text = leaderboard.total5;
                games6.text = leaderboard.total6;
                games7.text = leaderboard.total7;
                games8.text = leaderboard.total8;
                games9.text = leaderboard.total9;
                games10.text = leaderboard.total10;
                yourgames.text = leaderboard.totalyou;
                level.text = leaderboard.level;
                pack.text = leaderboard.packname;
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(getLeaders());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
