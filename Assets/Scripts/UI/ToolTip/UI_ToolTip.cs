using UnityEngine;

public class UI_ToolTip : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private Vector2 offset;

    protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public virtual void ShowToolTip(bool show, RectTransform target = null)
    {
        if (rect == null) return; // 防止已被销毁时访问

        if (!show)
        {
            // this.rect.position = new Vector3(9999, 9999, 0); // 隐藏到屏幕外
            this.gameObject.SetActive(false); // 比移动到屏幕外更可靠
            return;
        }
        this.gameObject.SetActive(true);

        if (target != null)
        {
            // 强制立即重建 Canvas / Layout，使 rect.size 在本帧可用（避免使用旧尺寸）
            // Canvas.ForceUpdateCanvases(); //所有Canvas中的布局
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rect); //指定rect中的布局

            this.UpdatePosition(target);
        }

    }

    private void UpdatePosition(RectTransform target)
    {
        // Vector2 targetPosition = target.position;
        // 更稳健地将目标世界位置转换为屏幕坐标
        Vector2 targetPosition = RectTransformUtility.WorldToScreenPoint(null, target.position);
        //获取屏幕的水平中心点
        float screenCenterX = Screen.width / 2f;
        //屏幕最高处和最低处的值
        float screenTop = Screen.height;
        float screenBottom = 0f;

        // 目标处于屏幕右半侧，ToolTip就向左偏移
        if (targetPosition.x >= screenCenterX)
            targetPosition.x -= this.offset.x;
        // 目标处于屏幕左半侧，ToolTip就向右偏移
        else
            targetPosition.x += this.offset.x;

        //ToolTip的半高
        // float toolTipHalfHeight = this.rect.sizeDelta.y / 2f;
        // 使用 rect.rect.height 获取实际渲染高度
        float toolTipHalfHeight = this.rect.rect.height / 2f;
        //以target的位置为中心，ToolTip的最高点
        float topOfToolTip = targetPosition.y + toolTipHalfHeight;
        //以target的位置为中心，ToolTip的最低点
        float bottomOfToolTip = targetPosition.y - toolTipHalfHeight;

        // ToolTip的最高点超出了屏幕
        if (topOfToolTip >= screenTop)
            targetPosition.y = screenTop - this.offset.y - toolTipHalfHeight;
        // ToolTip的最低点超出了屏幕
        else if (bottomOfToolTip <= screenBottom)
            targetPosition.y = screenBottom + this.offset.y + toolTipHalfHeight;

        this.rect.position = targetPosition;
    }

    protected string GetColoredText(string colorHex, string text)
    {
        return $"<color={colorHex}>{text}</color>";
    }
}
