using UnityEngine;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public PlaneDataBase planeDatabase;
    public PlaneItem[] planeItems;

    void Start()
    {
        LoadShopData();
        InitializeShop();
    }

    void LoadShopData()
    {
        for (int i = 0; i < planeDatabase.PlanesCount; i++)
        {
            planeDatabase.GetPlanes(i).isBought = PlayerPrefs.GetInt("Plane_" + i + "_Bought", i == 0 ? 1 : 0) == 1;
            planeDatabase.GetPlanes(i).price = PlayerPrefs.GetInt("Plane_" + i + "_Price", planeDatabase.GetPlanes(i).price);
        }
    }

    void SaveShopData()
    {
        for (int i = 0; i < planeDatabase.PlanesCount; i++)
        {
            PlayerPrefs.SetInt("Plane_" + i + "_Bought", planeDatabase.GetPlanes(i).isBought ? 1 : 0);
            PlayerPrefs.SetInt("Plane_" + i + "_Price", planeDatabase.GetPlanes(i).price);
        }
        PlayerPrefs.Save();
    }

    void InitializeShop()
    {
        for (int i = 0; i < planeItems.Length; i++)
        {
            planeItems[i].planeId = i;
            planeItems[i].Initialize(planeDatabase.GetPlanes(i));
        }
        SelectPlane(GameManager.Instance.selectedPlaneId);
    }

    public void BuyPlane(int planeId)
    {
        Planes planeData = planeDatabase.GetPlanes(planeId);
        planeData.isBought = true;
        planeItems[planeId].Initialize(planeData);
        SaveShopData();
    }

    public void SelectPlane(int planeId)
    {
        if (planeDatabase.GetPlanes(planeId).isBought)
        {
            foreach (PlaneItem item in planeItems)
            {
                item.Deselect();
            }
            planeItems[planeId].Select();

            Sprite selectedPlaneSprite = planeDatabase.GetPlanes(planeId).planeSprite;
            if (GameManager.Instance.playerController != null)
            {
                GameManager.Instance.playerController.GetComponent<SpriteRenderer>().sprite = selectedPlaneSprite;
            }
            GameManager.Instance.selectedPlaneId = planeId;
        }
    }
}