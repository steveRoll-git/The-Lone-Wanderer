using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : Creature
{
    private const float PI = Mathf.PI;

    [Space(20)]
    public int walkFrames = 4;
    public Sprite[] spritesDown;
    public Sprite[] spritesUp;
    public Sprite[] spritesLeft;
    public Sprite[] spritesRight;
    public float walkAnimSpeed = 1;

    private float currentFrame = 0;

    public float moveSpeedDefault = 10;
    private float moveSpeed;
    private float moveSpeed_attack;
    private float moveSpeed_aim;

    [Space(10)]
    public Sprite aimLaserSprite;
    public float aimLaserStartDistance;
    public float aimLaserLength;
    public float aimLaserHeight;
    public float aimLaserAlpha;
    private GameObject aimLaser;
    private bool aiming = false;

    [Space(10)]
    public Bullet bulletPrefab;
    public float bulletSpeed;
    public float bulletDamage;
    public float bulletKnockback;
    public float bulletSpread;

    [Space(10)]
    public float minWalkAnimSpeed = 100;

    public float stopSpeed = 0.01f;

    [Space(10)]
    public GameObject shootFlarePrefab;

    public Vector3 swordOffset;

    private Vector2 previousPosition;

    private Vector2 movement;
    
    //private GameObject sword;
    private SwingDamage swordSwing;

    private InteractSensor interactSensor;

    private DialogBox dialogBox;

    [Space(10)]
    public Image healthBarBg;
    public Image healthBar;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        swordSwing = GetComponentInChildren<SwingDamage>();
        swordSwing.sender = transform;
        swordSwing.offset = swordOffset;
        swordSwing.gameObject.SetActive(false);

        interactSensor = GetComponentInChildren<InteractSensor>();

        dialogBox = GameObject.FindWithTag("DialogBox").GetComponent<DialogBox>();
        
        aimLaser = new GameObject("aim laser");
        aimLaser.layer = 5;
        SpriteRenderer laserSpr = aimLaser.AddComponent<SpriteRenderer>();
        laserSpr.sprite = aimLaserSprite;
        laserSpr.color = new Color(1, 1, 1, aimLaserAlpha);
        Transform laserTransform = laserSpr.transform;
        laserTransform.parent = transform;
        laserTransform.localScale = new Vector3(aimLaserLength, aimLaserHeight, 1);
        laserTransform.localPosition = swordOffset;
        aimLaser.SetActive(false);

        moveSpeed = moveSpeedDefault;
        moveSpeed_attack = moveSpeedDefault / 2;
        moveSpeed_aim = moveSpeedDefault / 3;
    }

    private Vector2 GetMouseDirection()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + swordOffset);
            
        return new Vector2(Input.mousePosition.x - screenPos.x, Input.mousePosition.y - screenPos.y).normalized;
    }

    private float GetMouseAngle()
    {
        Vector2 direction = GetMouseDirection();
        return Mathf.Atan2(direction.y, direction.x);
    }

    private void Update()
    {
        base.Update();

        if (alive)
        {
            aiming = Input.GetMouseButton(1) && !swordSwing.attacking && !dialogBox.inDialog;
            aimLaser.SetActive(aiming);
            if (!swordSwing.attacking)
            {
                swordSwing.gameObject.SetActive(aiming);
            }
        
            if (aiming)
            {
                float mouseAngle = GetMouseAngle();
                Vector3 startPos = new Vector3(Mathf.Cos(mouseAngle) * aimLaserStartDistance, Mathf.Sin(mouseAngle) * aimLaserStartDistance) + swordOffset;
                aimLaser.transform.localPosition = startPos;
                aimLaser.transform.rotation = Quaternion.Euler(0, 0, mouseAngle * Mathf.Rad2Deg);
            
                swordSwing.UpdateTransform(mouseAngle);

                if (Input.GetMouseButtonDown(0))
                {
                    //FIRE!
                    float angle = mouseAngle + Random.Range(-bulletSpread / 2, bulletSpread / 2);
                    Bullet bullet = Instantiate(bulletPrefab);
                    bullet.transform.position = transform.position + startPos;
                    bullet.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                    bullet.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * bulletSpeed;
                    bullet.damage = bulletDamage;
                    bullet.knockback = bulletKnockback;
                    bullet.sender = transform;

                    Instantiate(shootFlarePrefab, transform.position + startPos, new Quaternion());
                }
            }
        
            else if (Input.GetMouseButtonDown(0) && !swordSwing.attacking && !dialogBox.inDialog)
            {
                swordSwing.attacking = true;

                Vector2 direction = GetMouseDirection();
            
                float angle = Mathf.Atan2(direction.y, direction.x);
            
                swordSwing.StartSwing(angle);

                spriteRenderer.sprite = GetAngleSprite(angle);
            }

            moveSpeed = swordSwing.attacking ? moveSpeed_attack : 
                (aiming ? moveSpeed_aim : moveSpeedDefault);
        }

        if (healthBar && healthBarBg)
            healthBar.rectTransform.sizeDelta = new Vector2(health / maxHealth * healthBarBg.rectTransform.sizeDelta.x, healthBarBg.rectTransform.sizeDelta.y);
        
    }
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.CompareTag("ORB"))
        {
            base.health += 1.0f;

        }

    }

    void FixedUpdate()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {
        if (!dialogBox.inDialog)
        {
            Vector2 newMovement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveSpeed;
        
            rigidbody.AddForce(newMovement * Time.deltaTime);
            
            if (newMovement.x != 0 || newMovement.y != 0)
            {
                movement = newMovement;
            }
        }

        currentFrame = (currentFrame + (rigidbody.position - previousPosition).magnitude * walkAnimSpeed * Time.deltaTime) % walkFrames;

        if (aiming)
        {
            spriteRenderer.sprite = GetDirectionSprite(GetMouseDirection());
        }
        else if (!swordSwing.attacking && (rigidbody.position - previousPosition).magnitude >= stopSpeed)
        {
            spriteRenderer.sprite = GetDirectionSprite(movement);
        }

        previousPosition = rigidbody.position;
        
        interactSensor.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * SnapAngle(Mathf.Atan2(movement.y, movement.x)));
    }

    public Sprite GetAngleSprite(float angle)
    {
        int frame = (rigidbody.position - previousPosition).magnitude <= minWalkAnimSpeed && !aiming ?
                    0 : Mathf.FloorToInt(currentFrame);
        if (angle >= PI/4*3 || angle <= -PI/4*3)
        {
            return spritesLeft[frame];
        }
        else if (angle >= PI/4)
        {
            return spritesUp[frame];
        }
        else if (angle <= -PI/4)
        {
            return spritesDown[frame];
        }
        else
        {
            return spritesRight[frame];
        }
    }

    public Sprite GetDirectionSprite(Vector2 direction)
    {
        return GetAngleSprite(Mathf.Atan2(direction.y, direction.x));
    }

    private float SnapAngle(float angle)
    {
        if (angle >= PI/4*3 || angle <= -PI/4*3)
        {
            return PI;
        }
        else if (angle >= PI/4)
        {
            return PI/2;
        }
        else if (angle <= -PI/4)
        {
            return -PI/2;
        }
        else
        {
            return 0;
        }
    }
}