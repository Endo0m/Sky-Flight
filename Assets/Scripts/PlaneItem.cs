using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaneItem : MonoBehaviour
{
    public int planeId;
    public Image planeImage;
    public TextMeshProUGUI priceText;
    public Button actionButton;
    public Sprite notBoughtSprite;
    public Sprite boughtSprite;
    public Sprite usedSprite;
    public TextMeshProUGUI buttonText;

    private int price;
    private bool isBought;
    private bool isSelected;

    public void Initialize(Planes planeData)
    {
        planeImage.sprite = planeData.planeSprite;
        price = planeData.price;
        isBought = planeData.isBought;
        UpdateUI();
    }

    void Start()
    {
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    void OnActionButtonClicked()
    {
        if (!isBought)
        {
            if (GameManager.Instance.CanBuyPlane(price))
            {
                GameManager.Instance.BuyPlane(planeId, price);
                isBought = true;
                UpdateUI();
            }
        }
        else
        {
            GameManager.Instance.SelectPlane(planeId);
        }
    }

    public void UpdateUI()
    {
        if (isBought)
        {
            if (isSelected)
            {
                actionButton.image.sprite = usedSprite;
                buttonText.text = "USED";
            }
            else
            {
                actionButton.image.sprite = boughtSprite;
                buttonText.text = "USE";
            }
            priceText.text = "BOUGHT";
        }
        else
        {
            actionButton.image.sprite = notBoughtSprite;
            buttonText.text = "BUY";
            priceText.text = price.ToString();
        }
    }

    public void Select()
    {
        isSelected = true;
        UpdateUI();
    }

    public void Deselect()
    {
        isSelected = false;
        UpdateUI();
    }
}