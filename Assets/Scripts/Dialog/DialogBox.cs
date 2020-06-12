using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public enum DialogPieceType
{
    Text,
    Jump,
    Choice,
    
    Sprite,
}

public class DialogParseException : Exception
{
    public DialogParseException(int line, string details) : base("dialog syntax error line " + (line + 1) + ": " + details)
    {
        
    }
}

public class DialogException : Exception
{
    public DialogException(int line, string details) : base("dialog error line " + (line + 1) + ": " + details)
    {
        
    }
}

public class UndefinedLabelException : DialogParseException
{
    public UndefinedLabelException(int line, string name) : base(line, "undefined label '" + name + "'")
    {
        
    }
}

public struct ChoiceTextPositionPair
{
    public string text;
    public int position;

    public ChoiceTextPositionPair(string text, int position)
    {
        this.text = text;
        this.position = position;
    }
}

public class TextDialogPiece : DialogPiece
{
    public string charName;
    public string text;
    public string expression;

    public TextDialogPiece(int line, string charName, string text, string expression) : base(line)
    {
        type = DialogPieceType.Text;
        
        this.charName = charName;
        this.text = text;
        this.expression = expression;
    }
}

public class ChoiceDialogPiece : DialogPiece
{
    public List<ChoiceTextPositionPair> choices;

    public ChoiceDialogPiece(int line, List<ChoiceTextPositionPair> choices) : base(line)
    {
        type = DialogPieceType.Choice;
        
        this.choices = choices;
    }
}

public class JumpDialogPiece : DialogPiece
{
    public int jumpTo;

    public JumpDialogPiece(int line, int jumpTo) : base(line)
    {
        type = DialogPieceType.Jump;
        
        this.jumpTo = jumpTo;
    }
}

public class SpriteDialogPiece : DialogPiece
{
    public string character;
    public string position;
    public string expression;
    public string filename;

    public SpriteDialogPiece(int line, string character, string position, string expression, string filename) : base(line)
    {
        type = DialogPieceType.Sprite;
        
        this.character = character;
        this.position = position;
        this.expression = expression;
        this.filename = filename;
    }
}

public class DialogPiece
{
    public DialogPieceType type;

    public int line;

    public DialogPiece(int line)
    {
        this.line = line;
    }
    
    /////

    struct LabelReference
    {
        public int line;
        public Action<int> action;

        public LabelReference(int line, Action<int> action)
        {
            this.line = line;
            this.action = action;
        }
    }
    
    /// parses a dialog script into an array of DialogPieces
    public static DialogPiece[] ParseDialog(string dialogText, char newLine = '\n')
    {
        string[] lines = dialogText.Split(newLine);
        
        List<DialogPiece> pieces = new List<DialogPiece>();

        Dictionary<string, int> labels = new Dictionary<string, int>();
        
        // stores references to labels that haven't been defined yet, and what should happen when they're defined 
        Dictionary<string, List<LabelReference>> labelReferences = new Dictionary<string, List<LabelReference>>();

        void LabelRef(string label, int line, Action<int> action)
        {
            //if the label is already defined, execute the action immediately
            //otherwise, store it in references for later
            if (labels.TryGetValue(label, out int position))
            {
                action(position);
            }
            else
            {
                if (!labelReferences.ContainsKey(label))
                {
                    labelReferences.Add(label, new List<LabelReference>());
                }
                labelReferences[label].Add(new LabelReference(line, action));
            }
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string l = lines[i].Trim();
            
            //ignore empty lines
            if (String.IsNullOrWhiteSpace(l)) continue;
            
            if (l[0] == '@')
            {
                //label
                
                string labelName = l.Substring(1);
                if (labels.ContainsKey(labelName))
                    throw new DialogParseException(i, "label '" + labelName + "' is defined more than once");

                int position = pieces.Count;
                
                if (labelReferences.TryGetValue(labelName, out List<LabelReference> references))
                {
                    foreach (LabelReference labelRef in references)
                    {
                        labelRef.action(position);
                    }

                    labelReferences.Remove(labelName);
                }
                
                labels[labelName] = position;
            }
            else if (l[0] == '?')
            {
                //choice

                string[] options;

                if (l == "?")
                {
                    List<string> opList = new List<string>();
                    while (true)
                    {
                        i++;
                        string op = lines[i].Trim();
                        opList.Add(op[op.Length-1] == ',' ? op.Substring(0, op.Length - 1) : op);
                        if (op[op.Length-1] != ',')
                        {
                            break;
                        }
                    }
                    options = opList.ToArray();
                }
                else
                {
                    options = l.Substring(1).Split(',');
                }

                List<ChoiceTextPositionPair> choices = new List<ChoiceTextPositionPair>();
                
                for (int o = 0; o < options.Length; o++)
                {
                    string option = options[o].Trim();

                    string text = option.Substring(0, option.IndexOf(':')).Trim();
                    string label = option.Substring(option.IndexOf(':') + 1).Trim();

                    if (label == "next")
                    {
                        choices.Add(new ChoiceTextPositionPair(text, pieces.Count + 1));
                    }
                    else
                    {
                        LabelRef(label, i, position => choices.Add(new ChoiceTextPositionPair(text, position)));
                    }
                }
                
                ChoiceDialogPiece thePiece = new ChoiceDialogPiece(i, choices);
                
                pieces.Add(thePiece);
            }
            else if (l[0] == '>')
            {
                //jump

                string labelName = l.Substring(1).Trim();
                
                JumpDialogPiece thePiece = new JumpDialogPiece(i, 0);
                
                LabelRef(labelName, i, position => thePiece.jumpTo = position);
                
                pieces.Add(thePiece);
            }
            else if (l[0] == '!')
            {
                //misc command

                string command = l.Substring(1, l.IndexOf(' ') - 1);

                string rest = l.Substring(command.Length + 2).Trim();
                
                if (command == "sprite")
                {
                    int colon = rest.IndexOf(':');
                    int comma = rest.IndexOf(',');
                    //comma = comma == -1 ? rest.Length : comma;
                    string character = rest.Substring(0, colon).Trim().ToLower();
                    string alias = "";
                    if (character.Contains("-"))
                    {
                        int hyphen = character.IndexOf('-');

                        alias = character.Substring(hyphen + 1);
                        character = character.Substring(0, hyphen);
                    }
                    
                    string position = rest.Substr(colon + 1, comma == -1 ? rest.Length : comma).Trim().ToLower();
                    string expression = comma == -1 ? "" : rest.Substring(comma + 1).Trim().ToLower();
                    
                    pieces.Add(new SpriteDialogPiece(i, character, position, expression, alias));
                }
            }
            else
            {
                //normal dialog
                
                if (!l.Contains(":"))
                    throw new DialogParseException(i, "invalid line syntax");

                int colon = l.IndexOf(':');
                string charName = l.Substring(0, colon);
                string expression = "";
                int paren = charName.IndexOf('(');
                if (paren != -1)
                {
                    expression = charName.Substr(paren + 1, charName.IndexOf(')'));
                    charName = charName.Substring(0, paren).Trim();
                }

                string messageText = l.Substring(colon + 1).Trim();
                
                pieces.Add(new TextDialogPiece(i, charName, messageText, expression));
            }
        }

        //if there are any references still left, it means their labels don't exist
        foreach (var kvp in labelReferences)
        {
            throw new DialogParseException(kvp.Value[0].line, "undefined label '" + kvp.Key + "'");
        }

        return pieces.ToArray();
    }
}

public class DialogBox : MonoBehaviour
{
    public GameObject continueArrow;

    [Space(10)]
    
    public GameObject nameBox;
    private Text nameText;
    
    [Space(10)]

    public GameObject choiceBox;
    private Transform choiceContainer;
    public GameObject choiceButtonPrefab;
    public float choiceButtonMargin = 5;
    private float choiceButtonHeight;
    
    /// is the dialog window currently active?
    public bool inDialog = false; 

    /// are letters currently being typed?
    private bool speaking = false;
    
    /// are we in a choice prompt right now?
    private bool inChoice = false;

    private DialogPiece[] currentDialog;
    /// which piece of dialog are we on now?
    private int dialogIndex;
    
    /// the line that's being typed right now
    private string currentLine;
    /// the character we're currently on
    private int letterIndex = 0;
    private float letterTimer = 0;
    [Space(10)]
    public float letterDelay = 0.1f;

    public Text dialogText;
    
    [Serializable]
    public struct SpritePosition
    {
        public string name;
        public Transform position;
    }
    [Space(10)]
    public SpritePosition[] SpritePositionsArray;

    private Dictionary<string, Transform> SpritePositions = new Dictionary<string, Transform>();

    private Dictionary<string, Sprite> dialogSprites = new Dictionary<string, Sprite>();
    
    private Dictionary<string, GameObject> currentDialogSprites = new Dictionary<string, GameObject>();

    private Dictionary<string, string> dialogSpriteFilenames = new Dictionary<string, string>();

    public float transparentSpriteAlpha = 0.7f;
    public float spriteFadeDuration = 0.5f;
    
    private GameObject activeSprite;
    
    private Action onDialogComplete;

    void Awake()
    {
        foreach (Sprite o in Resources.LoadAll("Dialog Portraits", typeof(Sprite)))
        {
            string name = o.name;

            if (name.Substring(0, 7) == "dialog_")
            {
                dialogSprites.Add(name.Substring(7).ToLower(), o);
            }
        }
        
        //convert sprite positions array into a dictionary
        foreach (SpritePosition pos in SpritePositionsArray)
        {
            SpritePositions.Add(pos.name, pos.position);
        }
    }

    void Start()
    {
        gameObject.SetActive(false);

        nameText = nameBox.transform.GetChild(0).GetComponent<Text>();

        choiceContainer = choiceBox.transform.GetChild(0);

        choiceButtonHeight = choiceButtonPrefab.GetComponent<RectTransform>().rect.height;
        
        choiceBox.SetActive(false);
    }

    void Update()
    {
        if (inDialog)
        {
            if (!inChoice && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Jump")))
            {
                if (speaking)
                {
                    //skip dialog
                    letterIndex = currentLine.Length - 1;
                }
                else
                {
                    //advance dialog
                    dialogIndex++;
                    ExecuteDialog();
                }
            }
            if (speaking)
            {
                letterTimer += Time.deltaTime;
                if (letterTimer >= letterDelay)
                {
                    letterTimer -= letterDelay;
                    letterIndex++;
                    dialogText.text = currentLine.Substring(0, letterIndex);
                    if (letterIndex >= currentLine.Length)
                    {
                        speaking = false;
                        continueArrow.SetActive(true);
                    }
                }
            }
        }
    }

    static void SetImageSprite(GameObject obj, Sprite sprite)
    {
        obj.GetComponent<Image>().sprite = sprite;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
        rect.pivot = sprite.pivot / rect.sizeDelta;
    }

    private Sprite GetCharacterSprite(string character, string expression, int line)
    {
        character = character.ToLower();

        string filename = character;

        if (dialogSpriteFilenames.TryGetValue(character, out string fn))
        {
            filename = fn;
        }
        
        if (dialogSprites.TryGetValue(filename + (!String.IsNullOrEmpty(expression) ? ("_" + expression) : ""),
            out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            throw new DialogException(line, "character sprite for '" + character + "' with expression '" + expression + "' doesn't exist");
        }
    }

    /// execute the current DialogPiece <br/> stops at Text and Choice pieces
    private void ExecuteDialog()
    {
        Dictionary<int, bool> jumped = new Dictionary<int, bool>();
        
        again:
        
        if (dialogIndex >= currentDialog.Length)
        {
            // no more dialog
            inDialog = false;
            gameObject.SetActive(false);

            if (onDialogComplete != null)
            {
                onDialogComplete();
            }
            
            //erase all active sprites
            foreach (GameObject image in currentDialogSprites.Values)
            {
                GameObject.Destroy(image);
            }
            currentDialogSprites.Clear();
            
            return;
        }
        
        DialogPiece p = currentDialog[dialogIndex];

        if (p.type == DialogPieceType.Text)
        {
            TextDialogPiece piece = p as TextDialogPiece;
            
            speaking = true;
            currentLine = piece.text;
            letterIndex = 0;
            letterTimer = 0;
            
            dialogText.text = "";
            
            continueArrow.SetActive(false);

            if (piece.charName != "")
            {
                nameBox.SetActive(true);
                nameText.text = piece.charName;

                if (currentDialogSprites.TryGetValue(piece.charName.ToLower(), out GameObject image))
                {
                    if (activeSprite)
                    {
                        activeSprite.GetComponent<Image>().CrossFadeAlpha(transparentSpriteAlpha, spriteFadeDuration, true);
                    }

                    if (!String.IsNullOrEmpty(piece.expression))
                    {
                        SetImageSprite(image, GetCharacterSprite(piece.charName, piece.expression, piece.line));
                    }

                    image.GetComponent<Image>().CrossFadeAlpha(1, spriteFadeDuration, true);
                    activeSprite = image;
                }
                else
                {
                    if (!String.IsNullOrEmpty(piece.expression))
                    {
                        throw new DialogException(piece.line, "character '" + piece.charName + "' doesn't have a sprite");
                    }
                }
            }
            else
            {
                nameBox.SetActive(false);
            }
        }
        else if (p.type == DialogPieceType.Choice)
        {
            ChoiceDialogPiece piece = p as ChoiceDialogPiece;
            
            foreach(Transform oldBtn in choiceContainer.transform)
            {
                GameObject.Destroy(oldBtn.gameObject);
            }

            float totalHeight = 0;

            for (int i=0; i<piece.choices.Count; i++)
            {
                ChoiceTextPositionPair pair = piece.choices[i];
                Transform newButton = Instantiate(choiceButtonPrefab, choiceContainer).transform;
                newButton.localPosition = new Vector3(0, -i * (choiceButtonHeight + choiceButtonMargin), 0);

                newButton.GetChild(0).GetComponent<Text>().text = pair.text;

                newButton.GetComponent<Button>().onClick.AddListener(delegate
                {
                    continueArrow.transform.localScale = new Vector3(1, 1, 1);
                    choiceBox.SetActive(false);

                    inChoice = false;
                    dialogIndex = pair.position;
                    ExecuteDialog();
                });

                totalHeight += choiceButtonHeight + (i < piece.choices.Count - 1 ? choiceButtonMargin : 0);
            }
            
            choiceContainer.transform.localPosition = new Vector3(0, totalHeight/2, 0);

            inChoice = true;
            
            continueArrow.transform.localScale = new Vector3(1, -1, 1);
            
            choiceBox.SetActive(true);
            
        }
        else if (p.type == DialogPieceType.Jump)
        {
            JumpDialogPiece piece = p as JumpDialogPiece;
            
            if (jumped.ContainsKey(piece.jumpTo))
                throw new DialogException(piece.line, "infinite jump loop");
            
            jumped.Add(piece.jumpTo, true);
            dialogIndex = piece.jumpTo;
            
            goto again;
        }
        else if (p.type == DialogPieceType.Sprite)
        {
            SpriteDialogPiece piece = p as SpriteDialogPiece;

            if (piece.filename != "" && !dialogSpriteFilenames.ContainsKey(piece.character.ToLower()))
            {
                dialogSpriteFilenames.Add(piece.character.ToLower(), piece.filename);
            }
            
            Sprite sprite = GetCharacterSprite(piece.character, piece.expression, piece.line);
            
            if (activeSprite)
            {
                activeSprite.GetComponent<Image>().CrossFadeAlpha(transparentSpriteAlpha, spriteFadeDuration, true);
            }
            
            GameObject image;
            if (!currentDialogSprites.ContainsKey(piece.character))
            {
                //create the object if it doesn't exist
                image = new GameObject(piece.character + " sprite");
                Image newImage = image.AddComponent<Image>();
                currentDialogSprites.Add(piece.character, image);
                RectTransform rect = image.GetComponent<RectTransform>();
                rect.SetParent(this.transform);
                rect.SetSiblingIndex(0);

                newImage.canvasRenderer.SetAlpha(0);
            }
            else
            {
                image = currentDialogSprites[piece.character];
            }

            activeSprite = image;
            
            image.GetComponent<Image>().CrossFadeAlpha(1, spriteFadeDuration, true);
            image.transform.localPosition = SpritePositions[piece.position].localPosition;
            SetImageSprite(image, sprite);
            
            dialogIndex++;
            goto again;
        }
    }
    
    public void StartDialog(DialogPiece[] dialog, Action onComplete = null)
    {
        inDialog = true;
        gameObject.SetActive(true);
        
        currentDialog = dialog;
        dialogIndex = 0;

        onDialogComplete = onComplete;

        dialogText.text = "";
        
        dialogSpriteFilenames.Clear();
        
        ExecuteDialog();
    }
}
