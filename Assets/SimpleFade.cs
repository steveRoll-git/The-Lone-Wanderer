using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFade : MonoBehaviour
{
    public float initialAlpha = 0.7f;
    
    public float fadeTime = 0.25f;

    private float time;

    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, initialAlpha);

        time = fadeTime;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, time / fadeTime * initialAlpha);
        if (time <= 0)
        {
            Destroy(gameObject);
        }
    }
}
