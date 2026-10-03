

using System;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;




public class TabUI : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        [SerializeField] public Button tabButton;
        [SerializeField] public Transform tab;   

        public event Action OnEnable;
        public event Action OnDisable;

        public void SetUp()
        {
            tabButton.onClick.AddListener(() => ChangeTab());
            OnDisable = null;
            OnEnable = null;      
        }
        public void CleanUp()
        {
            tabButton.onClick.RemoveAllListeners();
        }
        public void TurnButton(bool value)
        {
            tabButton.image.sprite = value ?  UIAssetsManager.instance.goldPressedBackgroundUI : UIAssetsManager.instance.woodBackgroundUI;
            var state =  tabButton.spriteState;
            state.highlightedSprite =  value ? UIAssetsManager.instance.goldPressedBackgroundUI : UIAssetsManager.instance.selectedWoodBackgroundUI;
            tabButton.spriteState = state;
        }
        public void TurnTab(bool value)
        {     
            if(value)
                OnEnable?.Invoke(); 
            else
                OnDisable?.Invoke();

            tab.gameObject.SetActive(value);
            TurnButton(value);
            if(value)
                TabUI.instance?.CloseOpen(this);
        }
        private void ChangeTab()
        {
            Sounds.instance.Click();
            if(this != instance.open)
                TurnTab(true);
        }
    }


    [Header("Tabs")]
    [SerializeField] private Transform tabsMenu;
    [SerializeField] public Tab equipmentTab;
    [SerializeField] public Tab skillsTab; 
    [SerializeField] public Tab mapTab; 
    [SerializeField] public Tab peopleTab;    
    private Tab open;


    private static TabUI instance;
    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void OnEnable()
    {
        equipmentTab.SetUp();
        skillsTab.SetUp();
        mapTab.SetUp();
        peopleTab.SetUp();
        UIManager.instance.TabSetUp();
    }
    public void OnDisable()
    {
        equipmentTab.CleanUp();
        skillsTab.CleanUp();
        mapTab.CleanUp();
        peopleTab.CleanUp();
    }
    public void SetActive(bool value)
    {
        tabsMenu.gameObject.SetActive(value);
        if(value)
            Crosshairs.Hide();
        else
            Crosshairs.Show();

        CloseOpen(null);  
    }
    public void CloseOpen(Tab newTab)
    {
        if(open != null)
            open.TurnTab(false);   
        open = newTab;
    }
    public bool IsOpen()
    {
        return tabsMenu.gameObject.activeSelf;
    }
}