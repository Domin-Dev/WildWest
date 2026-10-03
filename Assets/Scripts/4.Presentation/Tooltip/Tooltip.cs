using UnityEngine.UI;
using TMPro;
using UnityEngine;


public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI headerField;
    [SerializeField] private RectTransform headerParent;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private TextMeshProUGUI contentField;

    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit;
    [SerializeField] private RectTransform rectTransform;

    bool isShow = false;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void SetColorHeader(Color color, Sprite sprite)
    {
        var spriteRe = headerParent.GetComponent<Image>();
        spriteRe.sprite = sprite;
        spriteRe.material.SetColor("_Color", color);
      
    }
    public void SetText(string content, string header = "", Color? headerColor = null)
    {
        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
            headerParent.gameObject.SetActive(false);
            verticalLayoutGroup.padding.top = verticalLayoutGroup.padding.left;
        }
        else
        {
            layoutElement.enabled = true;
            headerField.text = header;
            verticalLayoutGroup.padding.top = (int)headerParent.sizeDelta.y + 15;
            verticalLayoutGroup.CalculateLayoutInputVertical();
            headerField.gameObject.SetActive(true);
            headerParent.gameObject.SetActive(true);

            if (headerColor == null)
                SetColorHeader(Color.white, UIAssetsManager.instance.yellowHeader);
            else
                SetColorHeader(headerColor.Value, UIAssetsManager.instance.whiteHeader);


        }



        contentField.text = content;
        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        if (string.IsNullOrEmpty(header))
            layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;
        Show();
        LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup.GetComponent<RectTransform>());
        UpdatePosition();

    }

    public void Show()
    {
        isShow = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        isShow = false; 
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(isShow)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        Vector2 position = Input.mousePosition;
        float pivotX = 0, pivotY = 0;
        if (Screen.width - position.x < rectTransform.rect.width) pivotX = 1;
        if (Screen.height - position.y < rectTransform.rect.height) pivotY = 1;
        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = position;
    }
}

