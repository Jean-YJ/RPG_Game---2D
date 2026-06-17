using System;
using UnityEngine;

[Serializable]
public class ParallaxLayer
{
    [SerializeField] private Transform background;
    [SerializeField] private float parallaxMultipler;
    private float imageFullWidth;
    private float imageHalfWidth;
    private float offset = 10.0f;

    public void Move(float moveDistance)
    {
        this.background.position += Vector3.right * moveDistance * parallaxMultipler;
    }
    public void CalculateImageWidth()
    {
        this.imageFullWidth = this.background.GetComponent<SpriteRenderer>().bounds.size.x;
        this.imageHalfWidth = this.imageFullWidth / 2;
    }
    public void LoopBackground(float cameraLeftEdge, float cameraRightEdge)
    {
        float imageRightEdge = (this.background.position.x + this.imageHalfWidth) - this.offset;
        float imageLeftEdge = (this.background.position.x - this.imageHalfWidth) + this.offset;
        if (cameraLeftEdge > imageRightEdge)
            this.background.position += Vector3.right * this.imageFullWidth;
        else if (cameraRightEdge < imageLeftEdge)
            this.background.position += Vector3.right * -this.imageFullWidth;
    }
}
