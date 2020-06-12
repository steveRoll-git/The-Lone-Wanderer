using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_BASKET : MonoBehaviour
{

    public OrbColor RGB;
    public GameObject uiorb;
    private Image image;

    public Sprite sprite_red;
    public Sprite sprite_blue;
    public Sprite sprite_green;

    public GameObject ORB;
    // private RectTransform transform;

    private List<GameObject> orbs = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        
        image.color = new Color(1,1,1,0);
    }

    private void Show()
    {
        image.color = Color.white;
        foreach (GameObject orb in orbs)
        {
            orb.GetComponent<Image>().color = Color.white;
        }
    }

    private void Hide()
    {
        image.color = Color.clear;
        foreach (GameObject orb in orbs)
        {
            orb.GetComponent<Image>().color = Color.clear;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Show();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            Hide();
        }
    }

    public void AddOrb(OrbColor color)
    {
        StartCoroutine(Wait(color));
    }
    IEnumerator Wait(OrbColor color)
    {
        Show();
        
        GameObject newOrb = Instantiate(uiorb, GetComponent<RectTransform>());
        newOrb.GetComponent<Image>().sprite = GetColorSprite(color);
        orbs.Add(newOrb);
        
        yield return new WaitForSeconds(2);
        
        Hide();
    }

    private Sprite GetColorSprite(OrbColor color)
    {
        if (color == OrbColor.Red)
        {
            return sprite_red;
        }
        else if (color == OrbColor.Green)
        {
            return sprite_green;
        }
        else if (color == OrbColor.Blue)
        {
            return sprite_blue;
        }
        else
        {
            return sprite_red;
        }


    }

}
