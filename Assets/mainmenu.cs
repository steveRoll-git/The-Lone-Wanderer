using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainmenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGame()
    {
        Application.LoadLevel(1);

    }

    public void QuitGame()
    {
        Application.Quit();

    }
}
