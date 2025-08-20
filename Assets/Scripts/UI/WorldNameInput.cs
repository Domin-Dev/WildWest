using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class WorldNameInput : MonoBehaviour
{


    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI errorMessage;

    private string setValue = string.Empty;
    public void SetUp()
    {
        inputField.text = GetDefaultWorldName();
        CheckErrors(inputField.text);
        inputField.onValueChanged.AddListener((string name) => {
            ValidateInput(name);
            CheckErrors(name);
        });
    }
    public void SetUp(string name)
    {
        setValue = name;
        inputField.text = name;
        CheckErrors(inputField.text);
        inputField.onValueChanged.AddListener((string name) => {
            ValidateInput(name);
            CheckErrors(name);
        });
    }

    public string GetWorldName()
    {
        if (CheckWorldName())
            return inputField.text.Trim().ToLower();
        else
            return string.Empty;
    }
    private string GetDefaultWorldName()
    {
        string name = "wild west";
        int i = 0;
        while (true)
        {
            string newName = $"{name}{(i > 0 ? " " + i : "")}";
            if (WorldManager.WorldExist(newName))
                i++;
            else
                return newName;
        }
    }
    private bool CheckWorldName()
    {
        string name = inputField.text.Trim();
        return name.Length > 0 && !WorldManager.WorldExist(name.ToLower());
    }
    private void ValidateInput(string input)
    {
        string valid = Regex.Replace(input, @"[^a-zA-Z0-9 ]", "");
        if (valid != input)
        {
            this.inputField.text = valid;
        }
    }
    private void CheckErrors(string name)
    {
        name = name.Trim().ToLower();
        if (name.Length == 0)
        {
            errorMessage.text = "The world name cannot be empty.";
        }
        else if(WorldManager.WorldExist(name))
        {
            if(setValue != string.Empty && setValue == name)
                errorMessage.text = "";
            else
                errorMessage.text = "A world with this name already exists.";
        }
        else
        {
            errorMessage.text = "";
        }
    }
}
