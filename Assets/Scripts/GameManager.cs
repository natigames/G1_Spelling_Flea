using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    // SINGLETON TO MAINTAIN VARIABLES
    private void Awake()
    {
        int gameStatusCount = FindObjectsOfType<GameManager>().Length;
        if (gameStatusCount > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void endGame()
    {
        SceneManager.LoadScene("GameOver");
    }


}
