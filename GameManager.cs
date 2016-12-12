using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;        
    using UnityEngine.UI;                   

    public class GameManager : MonoBehaviour
    {
        public float levelStartDelay = 2f;                      
        public float turnDelay = 0.1f;                          
        public int playerFoodPoints = 100;                      
        public static GameManager instance = null;           
        [HideInInspector]
        public bool playersTurn = true;    

        private Text levelText;                                
        private GameObject levelImage;                       
        private BoardManager boardScript;                       
        private int level = 1;                                 
        private List<Enemy> enemies;                            
        private bool enemiesMoving;                            
        private bool doingSetup = true;                     

        //Awake is always called before any Start function
        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
            enemies = new List<Enemy>();
            boardScript = GetComponent<BoardManager>();
            InitGame();
        }

        //This method is called only once, and the parameter tells it to be called only after the scene was loaded
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }

        //Initializes each level
        void InitGame()
        {
            doingSetup = true;
            levelImage = GameObject.Find("LevelImage");
            levelText = GameObject.Find("LevelText").GetComponent<Text>();
            levelText.text = "Level " + level;
            levelImage.SetActive(true);
            Invoke("HideLevelImage", levelStartDelay);
            enemies.Clear();
            boardScript.SetupScene(level);
        }

        //Hide black image used between levels
        void HideLevelImage()
        {
            levelImage.SetActive(false);
            doingSetup = false;
        }

        //Update is called every frame
        void Update()
        {
            if (playersTurn || enemiesMoving || doingSetup)
                return;
            StartCoroutine(MoveEnemies());
        }

        //Add the passed in Enemy to the List of Enemy objects
        public void AddEnemyToList(Enemy script)
        {
            enemies.Add(script);
        }

        //GameOver is called when the player reaches 0 health points
        public void GameOver()
        {
            levelText.text = "You completed " + (level - 1) + " levels";
            levelImage.SetActive(true);
            enabled = false;
        }

        //Coroutine to move enemies in sequence
        IEnumerator MoveEnemies()
        {
            enemiesMoving = true;
            yield return new WaitForSeconds(turnDelay);
            if (enemies.Count == 0)
            {
                yield return new WaitForSeconds(turnDelay);
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].MoveEnemy();
                yield return new WaitForSeconds(enemies[i].moveTime);
            }
            playersTurn = true;
            enemiesMoving = false;
        }
    }
}