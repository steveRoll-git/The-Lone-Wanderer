using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public DialogBox dialogBox;

    public TextAsset dialogFile;

    private DialogPiece[] theDialog;

    private Button button;
    
    private void Start()
    {
        button = GetComponent<Button>();
        
        theDialog = DialogPiece.ParseDialog(dialogFile.text);
    }

    public void onButtonClick(){
        dialogBox.StartDialog(theDialog, delegate { button.interactable = true; });
        button.interactable = false;
    }
}
