using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinueArrowBounce : MonoBehaviour
{
    private RectTransform rectTransform;
    private float origY;
    private float timer = 0;

    public float bounceRange = 5;
    public float speed = 4;
    
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        origY = rectTransform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        timer = (timer + Time.deltaTime * speed) % (Mathf.PI * 2);
        rectTransform.position = new Vector3(rectTransform.position.x, origY + Mathf.Sin(timer) * bounceRange, rectTransform.position.z);
    }
}
