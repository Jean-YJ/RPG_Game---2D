using UnityEngine;

public class Skill_SwordToss : Skill_Base
{
    private SkillObject_SwordRegular currentSwordObj;
    private float currentTossPower;
    [Header("Regular Sword")]
    [SerializeField] private GameObject swordRegularPrefab;
    [Range(0, 10)]
    [SerializeField] private float regularTossPower = 5f;

    [Header("Peirce Sword Upgrade")]
    [SerializeField] private GameObject swordPeircePrefab;
    public int amountToPeirce = 2;
    [Range(0, 10)]
    [SerializeField] private float peirceTossPower = 5f;

    [Header("Spin Sword Upgrade")]
    [SerializeField] private GameObject swordSpinPrefab;
    public float maxSpinDistance = 5;
    public int attackAmountPersecond = 6;
    public float maxSpinDuration = 3.0f;
    [Range(0, 10)]
    [SerializeField] private float spinTossPower = 5f;

    [Header("Bounce Sword Upgrade")]
    [SerializeField] private GameObject swordBouncePrefab;
    public float bounceSpeed = 15.0f;
    public int bounceCount = 5;
    [Range(0, 10)]
    [SerializeField] private float bounceTossPower = 5f;

    [Header("Predict Trajectory")]
    [SerializeField] private float swordGravityScale;
    [SerializeField] private GameObject predictionDotPrefab;
    [SerializeField] private int numberOfDots = 20;
    [SerializeField] private float spaceBetweenDots = .05f;
    private Transform[] dots;
    private Vector2 confirmedDirection;

    protected override void Awake()
    {
        base.Awake();
        this.swordGravityScale = this.swordRegularPrefab.GetComponent<Rigidbody2D>().gravityScale;
        this.dots = GenerateTrajectoryDots();
    }

    public override bool CanUseSkill()
    {
        UpdateTossPower();
        //一次一把剑，未回收时禁用
        if (this.currentSwordObj != null)
        {
            this.currentSwordObj.GetSwordBackToPlayer(); // 回收
            return false;
        }

        return base.CanUseSkill();
    }

    public void TossSword()
    {
        // Debug.Log("Toss Sword!");
        GameObject newSword = Instantiate(this.GetSwordPrefab(), this.dots[0].position, Quaternion.identity);
        this.currentSwordObj = newSword.GetComponent<SkillObject_SwordRegular>();
        this.currentSwordObj.SetUpSword(this, this.confirmedDirection * (this.currentTossPower * 10));

        SetSkillOnCoolDown();
    }

    /// <summary>
    /// 以Dir为方向，生成运动轨迹
    /// </summary>
    /// <param name="dir">初始方向</param>
    public void PredictTrajectoryPoint(Vector2 dir)
    {
        for (int i = 0; i < this.dots.Length; i++)
        {
            this.dots[i].position = GetTrajectoryPoint(dir, i * this.spaceBetweenDots);
        }
    }

    // 以dir为投掷方向，在time时间后的运动轨迹位置
    private Vector2 GetTrajectoryPoint(Vector2 dir, float time)
    {
        float scaledTossPower = this.currentTossPower * 10;

        //投掷后的初始速度 方向*力量
        Vector2 initialVelocity = dir * scaledTossPower;
        // 物理公式，计算time时间内重力造成的位移 （1/2）* G * t^2
        Vector2 gravityEffect = 0.5f * Physics2D.gravity * this.swordGravityScale * (time * time);
        // 通过 把在time时间内把速度的位移和重力造成的位移加起来的计算 可以得到time后的运动情况
        Vector2 trajectoryPoint = (initialVelocity * time) + gravityEffect;
        Vector2 playerPosition = this.transform.root.position; // 最上级根对象的位置，此处即是Player

        return playerPosition + trajectoryPoint;
    }
    // 获取投掷的初始方向
    public void ConfirmTrajectoryDir(Vector2 direction) => this.confirmedDirection = direction;

    // 是否显示预测点
    public void SetDotEnable(bool status)
    {
        foreach (Transform t in this.dots)
            t.gameObject.SetActive(status);
    }

    // 根据numberOfDots生成轨迹预测点
    private Transform[] GenerateTrajectoryDots()
    {
        Transform[] newDots = new Transform[this.numberOfDots];

        for (int i = 0; i < this.numberOfDots; i++)
        {
            newDots[i] = Instantiate(this.predictionDotPrefab, this.transform.position,
                                        Quaternion.identity, this.transform).transform;
            newDots[i].gameObject.SetActive(false); // 默认不显示
        }

        return newDots;
    }

    public GameObject GetSwordPrefab()
    {
        if (IsUpgradeUnLocked(SkillUpgradeType.SwordThrow))
            return this.swordRegularPrefab;

        if (IsUpgradeUnLocked(SkillUpgradeType.SwordThrow_Pierce))
            return this.swordPeircePrefab;

        if (IsUpgradeUnLocked(SkillUpgradeType.SwordThrow_Spin))
            return this.swordSpinPrefab;

        if (IsUpgradeUnLocked(SkillUpgradeType.SwordThrow_Bounce))
            return this.swordBouncePrefab;

        Debug.Log("No valied sword upgrade selected!");
        return null;
    }

    private void UpdateTossPower()
    {
        switch (this.skillUpgradeType)
        {
            case SkillUpgradeType.SwordThrow:
                currentTossPower = regularTossPower;
                break;
            case SkillUpgradeType.SwordThrow_Pierce:
                currentTossPower = peirceTossPower;
                break;
            case SkillUpgradeType.SwordThrow_Spin:
                currentTossPower = spinTossPower;
                break;
            case SkillUpgradeType.SwordThrow_Bounce:
                currentTossPower = bounceTossPower;
                break;
        }
    }
}
