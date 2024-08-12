using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlaneDataBase", menuName = "ScriptableObjects/PlaneDataBase", order = 1)]
public class PlaneDataBase : ScriptableObject
{
    public Planes[] planes;

    public int PlanesCount
    {
        get { return planes.Length; }
    }

    public Planes GetPlanes(int index)
    {
        return planes[index];
    }

    public void SetPlanePrices(int[] prices)
    {
        for (int i = 0; i < planes.Length && i < prices.Length; i++)
        {
            planes[i].price = prices[i];
        }
    }
}