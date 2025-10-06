
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
    [SerializeField] WorldNameInput worldNameInput;
    [SerializeField] Button generateSeed;
    [SerializeField] ListSwitch difficultylevelSwitch;
    [SerializeField] Button next;

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



        difficultylevelSwitch.SetUpSwitch(tab,1);
        
        difficultylevelSwitch.OnChangedValue += (int x) => 
        { 
            GameInfo.instance.difficultyLevel = (Difficulty)x;
        };

        worldNameInput.SetUp();

        next.onClick.AddListener(() => 
        {
            string worldName = worldNameInput.GetWorldName(); 
            if (worldName != string.Empty)
            {
                GameInfo.instance.creationTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                GameInfo.instance.worldName = worldName;
                GenerateWorld();
            }
        });
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

