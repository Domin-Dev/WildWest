
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;


public class WorldSetup : MonoBehaviour 
{
    [SerializeField] TMP_InputField inputFieldSeed;
    [SerializeField] TMP_InputField inputFieldWorldName;
    [SerializeField] Button generateSeed;
    [SerializeField] ListSwitch difficultylevelSwitch;
    [SerializeField] Button next;
    [SerializeField] TextMeshProUGUI errorMessage;

    string[] tab = {"Easy","Normal","Hard"};
    public void Awake()
    {
        GameInfo.instance.SetDefaultSettings();
        var random = new System.Random();
        int seed = random.Next();
        SetSeed(seed);

        generateSeed.onClick.AddListener(() => 
        {
           SetSeed(MyTools.NextFullInt(random)); 
        });
        inputFieldSeed.onEndEdit.AddListener((string hex) => { 
            SetSeed(MyTools.HexToInt(hex)); 
        });


        inputFieldWorldName.text = GetDefaultWorldName();
        SetWorldName(inputFieldWorldName.text);
        inputFieldWorldName.onValueChanged.AddListener((string name) => {
            ValidateInput(name);
            SetWorldName(name);
        });


        difficultylevelSwitch.SetUpSwitch(tab,1);
        
        difficultylevelSwitch.OnChangedValue += (object s,int x) => 
        { 
            GameInfo.instance.difficultyLevel = (Difficulty)x;
        };

        next.onClick.AddListener(() => 
        {
            if (CheckWorldName())
            {
                GameInfo.instance.creationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                GameInfo.instance.worldName = inputFieldWorldName.text.Trim();
                GenerateWorld();
            }
        });
    }

    private string GetDefaultWorldName()
    {
        string name = "Wild West";
        int i = 0;
        while (true)
        {
            string newName = $"{name}{(i > 0 ? " " + i : "")}";
            if (LoadSystem.WorldExist(newName))
                i++;
            else
                return newName;
        }
    }
    private bool CheckWorldName()
    {
        string name = inputFieldWorldName.text.Trim();
        return name.Length > 0 && !LoadSystem.WorldExist(name);
    }

    void ValidateInput(string input)
    {
        string valid = Regex.Replace(input, @"[^a-zA-Z0-9 ]", "");        
        if (valid != input)
        {
            inputFieldWorldName.text = valid;
        }
    }
    private void GenerateWorld()
    {
        GameInfo.LoadScene(2, 1);
        ClientServerBootstrap.ServerWorld.EntityManager.CreateEntity(typeof(GenerateMap));
    }
    private void SetSeed(int seed)
    {
        GameInfo.instance.seed = seed;
        inputFieldSeed.text = seed.ToString("x");
    }
    private void SetWorldName(string name)
    {
        name = name.Trim();
        if (name.Length == 0)
        {
            errorMessage.text = "The world name cannot be empty.";
        }
        else if(LoadSystem.WorldExist(name))
        {
            errorMessage.text = "A world with this name already exists.";
        }
        else
        {
            errorMessage.text = "";
        }
    }
    public static string Generate(int length)
    {
        var random = new System.Random();
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int category = random.Next(3); 
            char c;

            if (category == 0)
                c = (char)('0' + random.Next(10));      
            else if (category == 1)
                c = (char)('A' + random.Next(26));       
            else
                c = (char)('a' + random.Next(26));    

            sb.Append(c);
        }

        return sb.ToString();
    }
}

