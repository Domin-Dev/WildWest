using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GamepadScroll : MonoBehaviour
{
    private ScrollRect scrollRect;
    private const float scrollSpeed = 500f; 
    private const float offset = 50f;

    private NewInput input;
    private void Start()
    {
        input = new NewInput();
        input.Enable();
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(CheckButtons);
        RefreshButtons();
    }


    public PlayerInput playerInput;

    private void OnEnable()
    {
        input?.Enable();

    }
    private void OnDisable()
    {
        input.Disable();
    }


    public void RefreshButtons()
    {

        if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content.GetComponent<RectTransform>());
            CheckButtons(scrollRect.normalizedPosition);
        }
    }
    private void CheckButtons(Vector2 value)
    {

        List<CanvasGroup> elements = new List<CanvasGroup>();
        for (int i = 0; i < scrollRect.content.childCount; i++)
            elements.Add(scrollRect.content.GetChild(i).GetComponent<CanvasGroup>());

        int indexSelected = -1;
        bool findSelected = true;
        bool somethingWasVisible = false;

        for (int j = 0; j < elements.Count; j++) 
        { 
            var item = elements[j];
            if (item == null) continue;

            RectTransform buttonRect = item.GetComponent<RectTransform>();
            Vector3 localPos = scrollRect.viewport.InverseTransformPoint(buttonRect.position);
            bool visible = localPos.y < -offset &&
                           localPos.y > -(scrollRect.viewport.rect.height + offset);
            if(visible)
            {
                if (indexSelected != -1)
                {
                    EventSystem.current.SetSelectedGameObject(item.transform.GetChild(indexSelected).gameObject);
                    indexSelected = -1;
                    findSelected = false;
                }
                somethingWasVisible = true;
            }
            else if (findSelected)
            {
                for (int i = 0; i < item.transform.childCount; i++)
                {
                    Button btn = item.transform.GetChild(i).GetComponent<Button>();
                    if (btn != null)
                    {
                        if (EventSystem.current.currentSelectedGameObject == btn.gameObject)
                        {
                            indexSelected = i;
                            if (somethingWasVisible && j > 0)
                            {
                                EventSystem.current.SetSelectedGameObject(elements[j - 1].transform.GetChild(indexSelected).gameObject);
                                findSelected = false;
                            }            
                            break;
                        }
                    }
                }
            }
            item.interactable = visible;
        }

        Debug.Log(EventSystem.current.currentSelectedGameObject);
    }
    private void Update()
    {
        float value = input.UI.ScrollWheel.ReadValue<Vector2>().y;
        if (Mathf.Abs(value) > 0.1f)
        {
            float contentHeight = scrollRect.content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            float scrollableHeight = contentHeight - viewportHeight;

            if (scrollableHeight <= 0) return;

            float normalizedMove = (value * scrollSpeed * Time.deltaTime) / scrollableHeight;

            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + normalizedMove);
        }
    }
}