using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SkillObject_DomainExpansion : SkillObject_Base
{
    private Skill_DomainExpansion deManager;

    // private float maxSize = 10;
    private float duration = 5;
    private float expandSpeed = 2;
    private float slowDownPercent = 0.9f;

    private Vector3 targetScale;
    private bool isShrinking;

    public void SetUpDomainExpansion(Skill_DomainExpansion manager)
    {
        this.deManager = manager;

        this.duration = this.deManager.GetDomainDuration();
        float maxSize = this.deManager.maxSize;
        this.slowDownPercent = this.deManager.GetSlowDownPercentage();
        this.expandSpeed = this.deManager.expandSpeed;

        this.targetScale = Vector3.one * maxSize;
        Invoke(nameof(ShrinkDomain), duration);

    }

    void Update()
    {
        HandleScaling();
    }

    //当前物体的scale和目标scale不同时，使当前物体的scale平滑接近目标
    private void HandleScaling()
    {
        float scaleDifference = Mathf.Abs(this.transform.localScale.x - this.targetScale.x);
        bool shouldScale = scaleDifference > 0.1f;

        if (shouldScale)
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, this.targetScale, this.expandSpeed * Time.deltaTime);

        if (this.isShrinking && scaleDifference <= 0.1f)
            TerminateDomain();
    }

    private void TerminateDomain()
    {
        this.deManager.ClearTargets();
        Destroy(this.gameObject);
    }
    private void ShrinkDomain()
    {
        this.targetScale = Vector3.zero;
        this.isShrinking = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy == null)
            return;

        this.deManager.AddTarget(enemy);
        enemy.EntitySlowDown(this.duration, this.slowDownPercent, true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy == null)
            return;

        this.deManager.RemoveTarget(enemy);
        enemy.StopSlowDown();
    }


}
