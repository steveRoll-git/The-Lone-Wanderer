using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    public TextAsset dialogText;

    private DialogPiece[] dialog;

    private DialogBox dialogBox;

    public NPC()
    {
        hideBubble = true;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        dialog = DialogPiece.ParseDialog(dialogText.text);

        GameObject dialogBoxObj = GameObject.FindWithTag("DialogBox");
        dialogBox = dialogBoxObj.GetComponent<DialogBox>();
    }

    public override void OnInteract()
    {
        dialogBox.StartDialog(dialog);
    }
}
