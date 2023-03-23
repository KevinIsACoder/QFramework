using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSort : MonoBehaviour
{
    private int[] _arrays = new int[8]{5, 3, 2, 6, 1, 12, 10, 8};

    // Start is called before the first frame update
    void Start()
    {
        Sort(_arrays, 0, _arrays.Length - 1);
        foreach (var VARIABLE in _arrays)
        {
            Debug.Log("值： " + VARIABLE);   
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Sort(int[] array, int left, int right)
    {
        int i = left;
        int j = right;
        int key = array[i]; //基准数
        
        while (i < j)
        {
            while (i < j && array[j] > key)
            {
                j--;
            }
            
            while (i < j && array[i] < key)
            {
                i++;
            }
            Debug.LogFormat("i {0} j {1}", i, j);
            SwaP(ref array[i], ref array[j]);
        }
        // /Debug.LogFormat("i {0} j {1}", i, j);
        if (left < i - 1)
       {
           Sort(array, 0, i - 1);   
       }

       if (i + 1 < array.Length - 1)
       {
           Sort(array, i + 1, array.Length - 1);
       }
    }

    void SwaP(ref int a, ref int b)
    {
        int tmp = a;
        a = b;
        b = tmp;
    }
}
