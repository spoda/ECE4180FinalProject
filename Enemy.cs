using UnityEngine;
using System.Collections;

namespace Completed
{
    //Enemy is subclass of MovingObject
    public class Enemy : MovingObject
    {
        public int playerDamage;                         
        public AudioClip attackSound1;              
        public AudioClip attackSound2;                     

        private Animator animator;                         
        private Transform target;                         
        private bool skipMove;                              

        //This Start method overrides the virtual Start method of the base class MovingObject
        protected override void Start()
        {
            GameManager.instance.AddEnemyToList(this);
            animator = GetComponent<Animator>();
            target = GameObject.FindGameObjectWithTag("Player").transform;
            base.Start();
        }

        //This method overrides the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            if (skipMove)
            {
                skipMove = false;
                return;
            }
            base.AttemptMove<T>(xDir, yDir);
            skipMove = true;
        }

        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player
        public void MoveEnemy()
        {
            int xDir = 0;
            int yDir = 0;
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
                yDir = target.position.y > transform.position.y ? 1 : -1;
            else
                xDir = target.position.x > transform.position.x ? 1 : -1;
            AttemptMove<Player>(xDir, yDir);
        }

        //OnCantMove is called if Enemy attempts to move into a space occupied by Player 
        protected override void OnCantMove<T>(T component)
        {
            Player hitPlayer = component as Player;
            hitPlayer.LoseFood(playerDamage);
            animator.SetTrigger("enemyAttack");
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }
    }
}