using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogOptionButton : MonoBehaviour,
                                    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject arrow;

    public UnityEvent onClick;
    
    // Start is called before the first frame update
    void Start()
    {
        arrow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        arrow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        arrow.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}
