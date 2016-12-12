using UnityEngine;
using System.Collections;

namespace Completed
{
    public abstract class MovingObject : MonoBehaviour
    {
        public float moveTime = 0.1f;           
        public LayerMask blockingLayer;         

        private BoxCollider2D boxCollider;     
        private Rigidbody2D rb2D;              
        private float inverseMoveTime;         

        protected virtual void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            rb2D = GetComponent<Rigidbody2D>();
            inverseMoveTime = 1f / moveTime;
        }

        //Move takes in parameters for x and y directions and a RaycastHit2D to check collision
        //Returns true if object is able to move, else false
        protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            Vector2 start = transform.position;
            Vector2 end = start + new Vector2(xDir, yDir);
            boxCollider.enabled = false;
            hit = Physics2D.Linecast(start, end, blockingLayer);
            boxCollider.enabled = true;
            if (hit.transform == null)
            {
                StartCoroutine(SmoothMovement(end));
                return true;
            }
            return false;
        }

        //Coroutine for moving units from one space to next
        protected IEnumerator SmoothMovement(Vector3 end)
        {
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            while (sqrRemainingDistance > float.Epsilon)
            {
                Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
                rb2D.MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }
        }

        //AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemy, Wall for Player)
        protected virtual void AttemptMove<T>(int xDir, int yDir)
            where T : Component
        {
            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit);
            if (hit.transform == null)
                return;
            T hitComponent = hit.transform.GetComponent<T>();
            if (!canMove && hitComponent != null)
                OnCantMove(hitComponent);
        }

        //If object cannot move
        protected abstract void OnCantMove<T>(T component)
            where T : Component;
    }
}