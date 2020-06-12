using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 velocity;

    public float damage;
    public float knockback;
    
    public Transform sender;

    private float life = 0;
    public float lifetime = 5;

    public GameObject bloodSpurtPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        life += Time.deltaTime;
        if (life >= lifetime)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity.normalized, velocity.magnitude * Time.deltaTime);
            if (hit && !hit.transform.IsChildOf(sender))
            {
                if (hit.transform.gameObject.TryGetComponent(out Creature creature) && creature.hurtCooldown <= 0)
                {
                    creature.Hurt(damage, velocity.normalized * knockback);

                    if (creature.alive)
                    {
                        Instantiate(bloodSpurtPrefab, hit.transform.position, transform.rotation);
                    }
                }
            
                Destroy(gameObject);
            }
        }
    }
}
