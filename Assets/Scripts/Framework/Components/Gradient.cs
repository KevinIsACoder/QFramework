using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect
{
    [SerializeField]
    public Color topColor = Color.white;
    [SerializeField]
    public Color bottomColor = Color.black;

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

    public void Refresh()
    {
        base.graphic.SetVerticesDirty();
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
            float dis = 1f;
            for (int k = 1; k < 6; k++)
            {
                float y = vertexList[k + i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }
            dis = topY - bottomY;
            for (int k = 0; k < 6; k++)
            {
                UIVertex vertText = vertexList[k + i];
                vertText.color = Color.Lerp(bottomColor, topColor, (vertText.position.y - bottomY) / dis);
                vertexList[k + i] = vertText;
            }
            i += 6;
        }
    }
}