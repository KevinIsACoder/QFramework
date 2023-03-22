/*
 *                        .::::.
 *                      .::::::::.
 *                     :::::::::::
 *                  ..:::::::::::'
 *               '::::::::::::'
 *                 .::::::::::
 *            '::::::::::::::..
 *                 ..::::::::::::.
 *               ``::::::::::::::::
 *                ::::``:::::::::'        .:::.
 *               ::::'   ':::::'       .::::::::.
 *             .::::'      ::::     .:::::::'::::.
 *            .:::'       :::::  .:::::::::' ':::::.
 *           .::'        :::::.:::::::::'      ':::::.
 *          .::'         ::::::::::::::'         ``::::.
 *      ...:::           ::::::::::::'              ``::.
 *     ````':.          ':::::::::'                  ::::..
 *                        '.:::::'                    ':'````..
 * 
 * @Author: zhendong liang
 * @Date: 2022-08-09 14:21:41
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-25 14:30:32
 * @Description: 游戏中的一些常量
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AppConsts
{
    public const string version = "1.0"; //版本号
    public static string persistentDataPath = Application.persistentDataPath;
    public static string dataPath = persistentDataPath + "/data";
    public static string assetPath = Application.dataPath;
    public static string staticAssetsPath = Application.streamingAssetsPath + "/StaticAssets";
}
