using UnityEngine;
using System.Collections;
using UnityEngine.UI;	
using UnityEngine.SceneManagement;

using System;
using System.IO.Ports;
using System.Linq;

namespace Completed
{
    //Player is subclass of MovingObject
    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f;        
        public int pointsPerFood = 10;              
        public int pointsPerSoda = 20;              
        public int wallDamage = 1;                 
        public Text foodText;                       
        public AudioClip moveSound1;               
        public AudioClip moveSound2;                
        public AudioClip eatSound1;                 
        public AudioClip eatSound2;               
        public AudioClip drinkSound1;              
        public AudioClip drinkSound2;              
        public AudioClip gameOverSound;             
        public SerialPort sp = new SerialPort("COM3", 9600);

        private Animator animator;                  
        private int food;                           

        //This method overrides the Start function of MovingObject
        protected override void Start()
        {
            animator = GetComponent<Animator>();
            food = GameManager.instance.playerFoodPoints;
            foodText.text = "Health: " + food;
            base.Start();
            if (sp.IsOpen)
            {
                sp.Close();
                sp = new SerialPort("COM3", 9600);
            }
            sp.Open();
        }

        //This function is called when behavior becomes disabled or inactive
        private void OnDisable()
        {
            GameManager.instance.playerFoodPoints = food;
        }

        private void Update()
        {
            if (!GameManager.instance.playersTurn) return;

            int horizontal = 0;     
            int vertical = 0;       

            //Check if we are running either in the Unity editor or in a standalone build
            #if UNITY_STANDALONE || UNITY_WEBPLAYER

            //Set horizontal or vertical to 1 or -1 depending on reading from serial port
            try
            {
                char c = (char)sp.ReadChar();
                if (c == 'u')
                {
                    vertical = 1;
                }
                else if (c == 'd')
                {
                    vertical = -1;
                }
                else if (c == 'l')
                {
                    horizontal = -1;
                }
                else if (c == 'r')
                {
                    horizontal = 1;
                }
            }
            catch (TimeoutException e) { }

            //Check if moving horizontally, if so set vertical to zero
            if (horizontal != 0)
            {
                vertical = 0;
            }
            	
            #endif
            //If we have a non-zero value for horizontal or vertical
            if (horizontal != 0 || vertical != 0)
            {
                AttemptMove<Wall>(horizontal, vertical);
            }
        }

        //Try moving
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            food--;
            foodText.text = "Health: " + food;
            base.AttemptMove<T>(xDir, yDir);
            RaycastHit2D hit;
            if (Move(xDir, yDir, out hit))
            {
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }
            CheckIfGameOver();
            GameManager.instance.playersTurn = false;
        }

        //When player meets Wall
        protected override void OnCantMove<T>(T component)
        {
            Wall hitWall = component as Wall;
            hitWall.DamageWall(wallDamage);
            animator.SetTrigger("playerChop");
        }

        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Exit")
            {
                Invoke("Restart", restartLevelDelay);
                enabled = false;
            }
            else if (other.tag == "Food")
            {
                food += pointsPerFood;
                foodText.text = "+" + pointsPerFood + " Health: " + food;
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
                other.gameObject.SetActive(false);
            }
            else if (other.tag == "Soda")
            {
                food += pointsPerSoda;
                foodText.text = "+" + pointsPerSoda + " Health: " + food;
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
                other.gameObject.SetActive(false);
            }
        }

        //Restart reloads the scene
        private void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        //When an enemy attacks the player
        public void LoseFood(int loss)
        {
            animator.SetTrigger("playerHit");
            food -= loss;
            foodText.text = "-" + loss + " Health: " + food;
            CheckIfGameOver();
        }

        //CheckIfGameOver checks if the player is out of health points and if so, ends the game
        private void CheckIfGameOver()
        {
            if (food <= 0)
            {
                SoundManager.instance.PlaySingle(gameOverSound);
                SoundManager.instance.musicSource.Stop();
                GameManager.instance.GameOver();
            }
        }
    }
}