using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleChase : MonoBehaviour
{
    public float moveSpeed;

    private Rigidbody2D rigidbody;
    private Transform player;

    private Creature creature;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;

        creature = GetComponent<Creature>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!creature || creature.hurtCooldown <= 0)
        {
            rigidbody.AddForce((player.position - transform.position).normalized * (moveSpeed * Time.deltaTime));
        }
    }
}
