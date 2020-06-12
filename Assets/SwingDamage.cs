using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingDamage : Damaging
{
    private const float PI = Mathf.PI;
    
    [Space(10)]
    public float attackRange = Mathf.PI / 2;
    public float attackSpeed = 0.1f;
    public float swordDistance = 0.3f;
    
    private float attackAngle;
    private float attackAngleMax;

    public Vector3 offset;
    
    [Space(10)]
    public GameObject trailPrefab;

    public float trailInterval = 0.05f;
    private float trailTime = 0;
    
    private BoxCollider2D collider;
    private SpriteRenderer spriteRenderer;

    private float trailPPU;

    void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        trailPPU = trailPrefab.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
    }

    public void UpdateTransform(float angle)
    {
        transform.localPosition = new Vector3(Mathf.Cos(angle) * swordDistance, Mathf.Sin(angle) * swordDistance, transform.localPosition.z) + offset;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle);

        if (spriteRenderer)
            spriteRenderer.flipY = angle >= PI / 2 || angle <= -PI / 2;
    }

    public void StartSwing(float angle)
    {
        attacking = true;
            
        attackAngle = angle - attackRange / 2;
        attackAngleMax = angle + attackRange / 2;
        
        UpdateTransform(attackAngle);
            
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (attacking)
        {
            attackAngle += attackSpeed * Time.deltaTime;

            UpdateTransform(attackAngle);
            
            trailTime += Time.deltaTime;
            if (trailTime >= trailInterval)
            {
                trailTime -= trailInterval;
                GameObject trail = Instantiate(trailPrefab,
                    transform.TransformPoint(collider.offset) + new Vector3(0,0, 0.1f),
                    transform.rotation);

                trail.transform.localScale = collider.size * trailPPU;
            }

            if (attackAngle >= attackAngleMax)
            {
                attacking = false;
                gameObject.SetActive(false);
            }
        }
    }
}
