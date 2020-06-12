using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrbColor
{
    Red,
    Green,
    Blue
}

public class ORBBehaviourScript : MonoBehaviour
{

    public int ID;
    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;

    public Sprite sprite_red;
    public Sprite sprite_blue;
    public Sprite sprite_green;

    public OrbColor color;

    private UI_BASKET uI_BASKET;

    void OnValidate()
    {
        GetComponent<SpriteRenderer>().sprite = GetColorSprite(color);
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        uI_BASKET = GameObject.FindGameObjectWithTag("BASKET").GetComponent<UI_BASKET>();
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sprite = GetColorSprite(color); 
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.CompareTag("Player"))
        {
            uI_BASKET.RGB = color;
            uI_BASKET.AddOrb(color);
           
            Destroy(gameObject);

        }

    }


    private Sprite GetColorSprite(OrbColor color)
    {
        if (color == OrbColor.Red)
        {
            return sprite_red;
        }
        else if (color == OrbColor.Green)
        {
            return sprite_green;
        }
        else if (color == OrbColor.Blue)
        {
            return sprite_blue;
        }
        else
        {
            return sprite_red;
        }


    }
}
