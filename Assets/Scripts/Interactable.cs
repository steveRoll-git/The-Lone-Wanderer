using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [NonSerialized]
    public bool hideBubble;
    
    public virtual void OnInteract()
    {
        
    }
}
