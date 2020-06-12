using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Creature : MonoBehaviour
{
    

    [NonSerialized]
    public bool alive = true;
    
    public float maxHealth;
    protected float health;

    [NonSerialized]
    public float hurtCooldown = 0;

    public float hurtCooldownLength;
    public float hurtAlpha = 0.7f;

    public float fadeLength = 1;

    private bool fading = false;
    private float fadeTime = 1;

    public Color deathColor;

    private Color origColor;
    
    protected Rigidbody2D rigidbody;
    protected SpriteRenderer spriteRenderer;

    private float defaultDrag;
    private float deathDrag;

    public GameObject explosionPrefab;
    
    // Start is called before the first frame update
    protected void Start()
    {
        health = maxHealth;
        
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        origColor = spriteRenderer.color;

        defaultDrag = rigidbody.drag;
        deathDrag = defaultDrag / 2;
    }

    public void Hurt(float hit, Vector2 push = new Vector2())
    {
        if (alive && hurtCooldown <= 0)
        {
            health = health - hit;
            if (health <= 0)
            {
                alive = false;
                fading = true;
                fadeTime = fadeLength;
                rigidbody.drag = deathDrag;
                GetComponent<Collider2D>().enabled = false;
            }
            else
            {
                hurtCooldown = hurtCooldownLength;
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, hurtAlpha);
            }
            
            rigidbody.AddForce(push);
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        if (hurtCooldown > 0)
        {
            hurtCooldown -= Time.deltaTime;
            if (hurtCooldown <= 0)
            {
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 1);
            }
        }
        if (fading)
        {
            fadeTime -= Time.deltaTime;

            spriteRenderer.color = Color.Lerp(deathColor, origColor, fadeTime / fadeLength);

            if (fadeTime <= 0)
            {
                Instantiate(explosionPrefab, GetComponent<Renderer>().bounds.center, new Quaternion());
                Destroy(gameObject);
            }
        }
    }
}
