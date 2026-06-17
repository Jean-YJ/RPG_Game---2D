using System;

using UnityEngine;
using UnityEngine.UI;

public class UI_TreeConnection : MonoBehaviour
{
    [SerializeField] private RectTransform rootPoint;
    [SerializeField] private RectTransform connectionLine;
    [SerializeField] private RectTransform childConnectPoint;


    public void DirectConnect(NodeDirectionType direction, float length, float offset)
    {
        bool isAlive = direction == NodeDirectionType.None ? false : true;
        float actualLenght = isAlive ? length : 0;
        float angle = isAlive ? GetDirectionAngle(direction) : 0;

        this.rootPoint.localRotation = Quaternion.Euler(0, 0, angle + offset);
        this.connectionLine.sizeDelta = new Vector2(actualLenght, this.connectionLine.sizeDelta.y);
    }

    private float GetDirectionAngle(NodeDirectionType type)
    {
        switch (type)
        {
            case NodeDirectionType.UpLeft: return 135f;
            case NodeDirectionType.Up: return 90f;
            case NodeDirectionType.UpRight: return 45f;
            case NodeDirectionType.Left: return 180f;
            case NodeDirectionType.Right: return 0f;
            case NodeDirectionType.DownLeft: return -135f;
            case NodeDirectionType.Down: return -90;
            case NodeDirectionType.DownRight: return -45f;
            default: return 0f;
        }
    }

    // 获取this.childConnectPoint 作为 point子对象的相对位置
    public Vector2 GetPointPosition(RectTransform point)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                point.parent as RectTransform, this.childConnectPoint.position, null,
                out Vector2 localPosition
            );
        return localPosition;
    }

    public Image GetConnectLineImage()
    {
        return this.connectionLine.GetComponent<Image>();
    }
}

public enum NodeDirectionType
{
    None,
    UpLeft,
    Up,
    UpRight,
    Left,
    Right,
    DownLeft,
    Down,
    DownRight
}

[Serializable]
public class UI_TreeConnectionDetail
{
    public UI_TreeConnectHandler childNode;
    public NodeDirectionType nodeDirection;

    [Range(100, 500)]
    public float lineLength = 150f;
    [Range(-30, 30)]
    public float offserAngle = 0;
}