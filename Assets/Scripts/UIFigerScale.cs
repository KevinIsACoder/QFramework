using UnityEngine;
public class UIFigerScale : MonoBehaviour
{
    public Transform target; //目标
    bool isInit;
    Vector3 touch1, touch2, oriPos, pos;
    float scale, disX, disY, oriScale;
    public float scaleSpeed = 1;

    public bool isLimitScale;
    public float min, max;

    private void Update()
    {
        //不是双指就关闭
        if (Input.touchCount != 2)
        {
            isInit = false;
        }

        //初始化
        if (Input.touchCount == 2 && !isInit)
        {
            //两指点位
            touch1 = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            touch2 = Camera.main.ScreenToWorldPoint(Input.GetTouch(1).position);

            //目标初始点位
            oriPos = new Vector3(transform.position.x, transform.position.y, 0);

            //两指中点
            pos = new Vector3((Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).x + Camera.main.ScreenToWorldPoint(Input.GetTouch(1).position).x) / 2, (Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).y + Camera.main.ScreenToWorldPoint(Input.GetTouch(1).position).y) / 2, 0);

            //两指中点和目标距离
            disX = pos.x - oriPos.x;
            disY = pos.y - oriPos.y;
            oriScale = transform.localScale.x;


            isInit = true;
        }

        if (Input.touchCount == 2)
        {
            //两指缩放比例
            scale = Vector3.Distance(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Camera.main.ScreenToWorldPoint(Input.GetTouch(1).position)) / Vector3.Distance(touch1, touch2);

            //利用scaleSpeed控制缩放速度
            scale = (scale - 1) * scaleSpeed;
            Debug.Log("FigerScale++++++++++ " + scale + "TargetLocal Scale+++++ " + target.localScale);
            //给缩放比例加限制
            if (isLimitScale && target.localScale.x <= min && scale < 0)
                return;
            if (isLimitScale && target.localScale.x >= max && scale > 0)
                return;

            //缩放目标大小
            target.localScale = new Vector3(oriScale + scale, oriScale + scale, oriScale + scale);
            //改变目标位置，让位置保持不变
            transform.position = new Vector3(oriPos.x - ((target.localScale.x - oriScale) * disX), oriPos.y - ((target.localScale.y - oriScale) * disY), 0);
        }
    }
}

