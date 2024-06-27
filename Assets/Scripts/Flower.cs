using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour, IItem
{
    public static event Action<int> OnFlowerCollect;
    public int worth = 5;
    public void Collect()
    {
        OnFlowerCollect.Invoke(worth);
        SEManager.Play("Flower");
        Destroy(gameObject);
    }

   
}
