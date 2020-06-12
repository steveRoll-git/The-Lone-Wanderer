using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaging : MonoBehaviour
{
    public float damage;

    public float knockback;

    [NonSerialized]
    public Transform sender;

    [NonSerialized]
    public bool attacking;

    public virtual Vector2 GetHitNormal(Transform other)
    {
        return (other.position - transform.position).normalized;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (attacking && !other.transform.IsChildOf(sender) && other.TryGetComponent(out Creature creature))
        {
            creature.Hurt(damage, GetHitNormal(creature.transform) * knockback);
        }
    }
}
