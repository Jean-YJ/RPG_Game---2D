using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TreeConnectHandler : MonoBehaviour
{
    [SerializeField] private UI_TreeConnectionDetail[] connectionDetails;
    [SerializeField] private UI_TreeConnection[] connections;
    private RectTransform myRect => this.GetComponent<RectTransform>();

    private Image connectLineImage;
    private Color originalLineColor;

    // 编辑阶段，Inspector窗口里进行配置修改时会调用
    void OnValidate()
    {

        if (connectionDetails.Length <= 0)
            return;

        if (connectionDetails.Length != connections.Length)
        {
            Debug.Log("Amount of details should be same as amount of connections. - " + gameObject.name);
            return;
        }
        UpdateConnections();
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (this.connectLineImage != null)
            this.originalLineColor = this.connectLineImage.color;
    }

    // 更新和本节点连接的所有子节点的连接情况
    private void UpdateConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            var detail = this.connectionDetails[i];
            var connection = this.connections[i];

            Image lineImg = connection.GetConnectLineImage();
            // 获取connection中的子节点作为myRect子对象的相对位置
            Vector2 childTargetPos = connection.GetPointPosition(this.myRect);

            //设置链接线
            connection.DirectConnect(detail.nodeDirection, detail.lineLength, detail.offserAngle);

            if (detail.childNode == null)
                continue;

            detail.childNode.SetPosition(childTargetPos);
            detail.childNode.SetConnectLineImg(lineImg);
            detail.childNode.transform.SetAsLastSibling(); //将子节点的层级结构置于最后，保证其显示一定高于父节点
        }
    }

    // 更新和本节点连接的所有子节点、以及子节点的子节点的连接情况
    // 本方法避免在OnValidate里调用。如果配置不小心出错了，出现了两个节点互为父子节点的情况，就会导致无限递归
    public void UpdateAllConnection()
    {
        this.UpdateConnections();
        foreach (var node in connectionDetails)
        {
            if (node.childNode == null) continue;
            node.childNode.UpdateConnections();
        }
    }

    // 本节点作为子节点时，根据外部的位置来调整自身位置
    public void SetPosition(Vector2 position) => this.myRect.anchoredPosition = position;

    // 本节点作为子节点时，获取父节点链接到自身的链接线
    public void SetConnectLineImg(Image image) => this.connectLineImage = image;

    public void UpdateUnlockConnectLineImage(bool isUnlock)
    {
        if (this.connectLineImage == null)
            return;

        this.connectLineImage.color = isUnlock ? Color.white : this.originalLineColor;
    }

    public UI_TreeNode[] GetChildNodes()
    {
        List<UI_TreeNode> childNodes = new List<UI_TreeNode>();
        foreach (var item in this.connectionDetails)
        {
            if (item.childNode != null)
                childNodes.Add(item.childNode.GetComponent<UI_TreeNode>());
        }

        return childNodes.ToArray();
    }
}


