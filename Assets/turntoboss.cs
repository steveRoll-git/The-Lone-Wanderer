using UnityEngine;
using System.Collections;

public class turntoboss : MonoBehaviour
{
    



    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject.CompareTag("Player"))
        {
            Application.LoadLevel(2);

        }

    }
}

