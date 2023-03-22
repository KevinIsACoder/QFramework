using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

//##左上角到右下角的渐变
//##by zwj
[AddComponentMenu("UI/Effects/Gradient2")]
public class Gradient2 : BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;
    [SerializeField]
    private Color32 bottomColor = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!this.IsActive())
            return;

        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);
        ModifyVertices(vertexList);

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }

    public void ModifyVertices(List<UIVertex> vertexList)
    {
        if (!IsActive() || vertexList.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < vertexList.Count;)
        {
            float bottomY = vertexList[i].position.y;
            float topY = bottomY;
            for (int k = 1; k < 6; k++)
            {
                float y = vertexList[k + i].position.y;
                if (y > topY)
                {
                    topY = y;
                    break;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                    break;
                }
            }

            float leftX = vertexList[i].position.x;
            float rightX = leftX;
            for (int k = 1; k < 6; k++)
            {
                float x = vertexList[k + i].position.x;
                if( x > rightX)
                {
                    rightX = x;
                    break;
                }else if (x < leftX)
                {
                    leftX = x;
                    break;
                }
            }

            // int leftTop = i;
            // int rightBottom = leftTop;

            // for (int k = 1; k < 6; k++)
            // {
            //     float x = vertexList[k + i].position.x;
            //     float y = vertexList[k + i].position.y;

            //     if(x == leftX && y == topY)
            //     {
            //         leftTop = k + i ;
            //     }
            //     else if (x == rightX && y == bottomY)
            //     {
            //         rightBottom = k + i ;
            //     }
            // }


            double maxDis = Math.Pow((bottomY - topY),2) + Math.Pow((leftX - rightX),2);
            maxDis = Math.Sqrt(maxDis);
           for (int k = 0; k < 6; k++)
            {
                UIVertex vertText = vertexList[k + i];
                double dis = Math.Pow((vertText.position.y - topY),2) + Math.Pow((vertText.position.x - leftX),2);
                dis = Math.Sqrt(dis);
                float part = float.Parse((dis/maxDis).ToString());
                vertText.color = Color32.Lerp(topColor, bottomColor, part);
                vertexList[k + i] = vertText;
            }
            i += 6;
        }
    }
}