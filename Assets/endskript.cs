﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endskript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.CompareTag("Player"))
        {
            Application.LoadLevel(0);

        }

    }
}
