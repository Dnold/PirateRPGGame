using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System;

public class ConsoleController : MonoBehaviour
{
    public GameObject consoleObject;
    public TMP_InputField consoleInputField;
    public TMP_Text consoleOutputField;



    bool isOpen = false;

    PickUpSystem pickUpSystem;
    private void Start()
    {
        pickUpSystem = GameManager.instance.gameObject.GetComponent<PickUpSystem>();
    }

    public void OpenConsole()
    {
        consoleObject.SetActive(true);
        isOpen = true;
        GameManager.instance.canMove = false;
    }
    public void CloseConsole()
    {
        consoleObject.SetActive(false);
        isOpen = false;
        GameManager.instance.canMove = true;
    }
    public void AcceptInput(string input)
    {
        if (input.StartsWith("give"))
        {
            GivePlayerItem(input);
        }
        else if (input.StartsWith("clear"))
        {
            ClearPlayerInventory();
        }
        else if (input.StartsWith("generate"))
        {
            string[] data = SplitInput(input);
            GenerateNewMap(Convert.ToInt32(data[1]), Convert.ToInt32(data[2]), float.Parse(data[3], System.Globalization.CultureInfo.InvariantCulture), Convert.ToInt32(data[4]), data[5]);
        }
        else if(input == "help")
        {
            PrintOutput("help - Get a List of all Commands");
            PrintOutput("give {itemID} {quantity} - gives player item of ItemId and with quantity");
            PrintOutput("clear - clears players Inventory");
            PrintOutput("generate {chunkSize(!CAREFULL!)} {gridSize(CAREFULL)} {waterFillPercent} {proccessThreshhold} {seed}");
        }
        else
        {
            PrintOutput("This Command doesn't exist");
            return;
        }
        PrintOutput("success");

    }
    public void GenerateNewMap(int chunkSize, int gridSize, float waterFillPercent, int proccesThreshold,string seed)
    {
        GameManager.instance.GenerateMap(chunkSize,gridSize,waterFillPercent,proccesThreshold,seed);
        GameManager.instance.LoadMap() ;
    }
    public string[] SplitInput(string input)
    {
        string[] data = input.Split(new char[] { ' ' });
        return data;
    }
    public void GivePlayerItem(string command)
    {
        string[] data = SplitInput(command);
        pickUpSystem.AddItem(Convert.ToInt32(data[1]), Convert.ToInt32(data[2]));
    }
    public void ClearPlayerInventory()
    {
        pickUpSystem.ClearInventory();

    }

    public void PrintOutput(string output)
    {
        consoleOutputField.text += "\n\n" + output;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isOpen)
        {
            OpenConsole();
        }
        if(Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseConsole();
        }
        if (Input.GetKeyDown(KeyCode.Return) && isOpen)
        {
            AcceptInput(consoleInputField.text.ToLower());
        }
    }
}
