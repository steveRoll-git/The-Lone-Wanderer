using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractSensor : MonoBehaviour
{
    public GameObject interactBubblePrefab;

    private GameObject interactBubble;

    public float floatAbove = 0.2f;
    
    private Interactable touching;

    private DialogBox dialogBox;
    
    // Start is called before the first frame update
    void Start()
    {
        interactBubble = Instantiate(interactBubblePrefab);
        interactBubble.name = "interact bubble";
        interactBubble.SetActive(false);

        dialogBox = GameObject.FindWithTag("DialogBox").GetComponent<DialogBox>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interact = other.gameObject.GetComponent<Interactable>();

        if (interact != null)
        {
            touching = interact;

            Bounds sprBounds = touching.GetComponent<Renderer>().bounds;
            interactBubble.transform.position = sprBounds.center + new Vector3(0, sprBounds.extents.y + floatAbove, 0);
            interactBubble.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (touching && other.gameObject == touching.gameObject)
        {
            touching = null;
            interactBubble.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && touching && !dialogBox.inDialog)
        {
            touching.OnInteract();

            if (touching.hideBubble)
            {
                interactBubble.SetActive(false);
            }
        }
    }
}
