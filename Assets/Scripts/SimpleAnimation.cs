using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimation : MonoBehaviour
{
    public Sprite[] frames;
    [NonSerialized]
    public Sprite[] nextFrames;

    public float frameSpeed = 1;
    private float currentFrame = 0;
    
    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetFrames(Sprite[] frames)
    {
        this.frames = frames;
        currentFrame = 0;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        spriteRenderer.sprite = frames[Mathf.FloorToInt(currentFrame)];
    }

    // Update is called once per frame
    void Update()
    {
        float prev = currentFrame;
        currentFrame = (currentFrame + frameSpeed * Time.deltaTime) % frames.Length;
        if (currentFrame < prev && nextFrames != null)
        {
            SetFrames(nextFrames);
            nextFrames = null;
        }
        else if (Mathf.FloorToInt(currentFrame) != Mathf.FloorToInt(prev))
        {
            UpdateSprite();
        }
    }
}
