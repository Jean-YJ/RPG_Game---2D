using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI_CanvasRoot canvasRoot;
    private Image img_SkillIcon;
    private Button btn_SkillSlot;
    private RectTransform rect; //用于ToolTip传参

    private SkillData_SO skillData;
    public SkillType skillType;

    [SerializeField] private Image img_CoolDownMask;
    [SerializeField] private string inputKeyName;
    [SerializeField] private TextMeshProUGUI inputKeyText;
    [SerializeField] private GameObject conflictSlot;

    private Coroutine coolDownCoroutine;

    void Awake()
    {
        this.canvasRoot = GetComponentInParent<UI_CanvasRoot>();
        this.img_SkillIcon = GetComponent<Image>();
        this.btn_SkillSlot = GetComponent<Button>();
        this.rect = GetComponent<RectTransform>();
    }
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    void OnValidate()
    {
        this.gameObject.name = "btn_SkillSlot - " + this.skillType.ToString();
    }

    public void SetSkillSlot(SkillData_SO selectSkill)
    {
        this.skillData = selectSkill;
        this.img_SkillIcon.sprite = selectSkill.icon;
        this.inputKeyText.text = this.inputKeyName;

        Color coolDownMaskColor = Color.black;
        coolDownMaskColor.a = 0.6f; // 设置半透明
        this.img_CoolDownMask.color = coolDownMaskColor;

        if (this.conflictSlot != null)
        {
            this.conflictSlot.SetActive(false);
        }
    }

    public void StartCoolDown(float coolDownTime)
    {
        if (this.coolDownCoroutine != null)
        {
            StopCoroutine(this.coolDownCoroutine);
        }

        this.img_CoolDownMask.fillAmount = 1f;
        this.coolDownCoroutine = StartCoroutine(CoolDownCoroutine(coolDownTime));
    }
    public void ResetCoolDown() => this.img_CoolDownMask.fillAmount = 0f;

    private IEnumerator CoolDownCoroutine(float coolDownTime)
    {
        float timePassed = 0f;
        while (timePassed < coolDownTime)
        {
            timePassed += Time.deltaTime;
            this.img_CoolDownMask.fillAmount = 1f - (timePassed / coolDownTime);
            yield return null;
        }
        this.img_CoolDownMask.fillAmount = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.skillData == null)
            return;

        this.canvasRoot.skillToolTip.ShowToolTip(null, this.skillData, true, this.rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.canvasRoot.skillToolTip.ShowToolTip(false);
    }
}
