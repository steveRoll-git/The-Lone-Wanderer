using System;
using UnityEngine;

public class Follower : Creature
{
    [Space(10)]
    public float moveSpeed;

    public float attackDistance;

    public float attackInterval = 0.5f;
    private float attackTimer = 0;
    
    private bool moving = false;

    private Animator animator;

    private SwingDamage swing;

    private GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();

        swing = GetComponentInChildren<SwingDamage>();
        swing.sender = transform;
        
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (alive && player)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
        
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10,
                ~LayerMask.GetMask("Enemy"));

            if (hit && hit.transform.gameObject == player)
            {
                rigidbody.AddForce(direction * (moveSpeed * Time.deltaTime));

                moving = true;
                animator.SetBool("move", true);
            }
            else
            {
                moving = false;
                animator.SetBool("move", false);
            }

            float prev = attackTimer;
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0 && moving && !swing.attacking && Vector2.Distance(transform.position, player.transform.position) <= attackDistance)
            {
                swing.StartSwing(Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x));
            
                animator.SetTrigger("attack");

                attackTimer = attackInterval;
            }
        }
    }
}
