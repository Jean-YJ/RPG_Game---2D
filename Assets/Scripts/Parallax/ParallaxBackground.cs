using System.Runtime.CompilerServices;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastPositionXOfCamera;
    private float currentPositionXOfCamera;
    [SerializeField] private ParallaxLayer[] backgroundLayers;
    private float cameraHalfWidth;

    void Awake()
    {
        this.mainCamera = Camera.main;
        this.cameraHalfWidth = this.mainCamera.orthographicSize * this.mainCamera.aspect;
        this.InitializeLayer();
    }

    // 使用FixedUpdate的原因：背景Sky跟随Camera移动，camera由CinemachineCamera控制跟随Player
    // 会存在微小的位置数值抖动，若每帧都更新的话会导致背景Sky也出现抖动
    // 注意：使用FixedUpdate后还需要调整maincamera上的CinemachineBrain组件上的UpdateMethod，SmartUpdate -> FixedUpdate
    // Player刚体组件上Inertpolate设为None
    void FixedUpdate()
    {
        this.currentPositionXOfCamera = this.mainCamera.transform.position.x;
        float distance = this.currentPositionXOfCamera - this.lastPositionXOfCamera;
        this.lastPositionXOfCamera = this.currentPositionXOfCamera;

        float cameraLeftEdge = this.currentPositionXOfCamera - this.cameraHalfWidth;
        float cameraRightEdge = this.currentPositionXOfCamera + this.cameraHalfWidth;

        foreach (ParallaxLayer layer in this.backgroundLayers)
        {
            layer.Move(distance);
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
        }
    }

    private void InitializeLayer()
    {
        foreach (ParallaxLayer layer in this.backgroundLayers)
        {
            layer.CalculateImageWidth();

        }
    }
}
