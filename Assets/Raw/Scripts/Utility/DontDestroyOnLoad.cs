/*
 * @Author: zhendong liang
 * @Date: 2022-08-11 16:58:52
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-11 16:59:57
 * @Description: 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
