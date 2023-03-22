using UnityEngine;

// [disa]
public class UILocaleConfig : MonoBehaviour {
    [Tooltip("忽略当前节点以及子节点的多语言配置")]
    public bool ignore;

    [Tooltip("勾选后翻转，不勾选不翻转。（一般只需要标记不翻转的）")]
    public bool flip;

    [Tooltip("多语言的主KEY")]
    public string mainTerm;

    [Tooltip("多语言第二个KEY")]
    public string secondaryTerm;
}
