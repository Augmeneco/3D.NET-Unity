using System.Collections;
using System.Collections.Generic;
using System; 
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using ThreeDNet.Engine;

public class Console : MonoBehaviour
{
    public InputField ConsoleInput;
    public InputField ConsoleOutput;
    public Canvas ConsoleCanvas;
    CanvasGroup ConsoleCanvasGroup;
    bool ConsoleEnabled;

    // Start is called before the first frame update
    void Start()
    {
        ConsoleInput.onEndEdit.AddListener(OnEndInput);
        ConsoleCanvasGroup = ConsoleCanvas.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
            if (!ConsoleEnabled)
            {
                //ConsoleInput.DeactivateInputField();
                //ConsoleCanvas.enabled = false;
                //UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                ConsoleEnabled = true;
                ConsoleCanvasGroup.alpha = 1;
                ConsoleCanvasGroup.interactable = true;
                ConsoleCanvasGroup.blocksRaycasts = true;
                ConsoleInput.ActivateInputField();
                ConsoleInput.Select();
            }
            else
            {
                ConsoleEnabled = false;
                ConsoleCanvasGroup.alpha = 0;
                ConsoleCanvasGroup.interactable = false;
                ConsoleCanvasGroup.blocksRaycasts = false;
                ConsoleInput.DeactivateInputField();
            }
    }

    void OnEndInput(string text)
    {

        ConsoleOutput.text += "> "+text+'\n';
        ConsoleInput.text = "";
        ConsoleInput.ActivateInputField();
        ConsoleInput.Select();

        ProcessCommand(text);
    }

    void ProcessCommand(string cmd)
    {
        string[] cmdSplit = cmd.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        if (cmdSplit.Length > 0)
            switch (cmdSplit[0])
            {
                case "tpchunk":
                    Debug.Log(cmdSplit[0]);
                    Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
                    player.NetCoords = new Vector2Int(int.Parse(cmdSplit[1], CultureInfo.InvariantCulture.NumberFormat), 
                                                    int.Parse(cmdSplit[2], CultureInfo.InvariantCulture.NumberFormat));
                    World world = World.getInstance();
                    world.ChangeActiveScene(player.NetCoords);
                    break;
                //default:
            }
    }
}
