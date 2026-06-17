using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 用于存储所有要用到的UI物体上的UI组件，用里式替换原则 父类装子类
    /// </summary>
    protected Dictionary<string,UIBehaviour> uiComponentDic = new Dictionary<string,UIBehaviour>();

    /// <summary>
    /// 控件默认名字 如果得到的控件名字存在于这个容器 意味着我们不会通过代码去使用它 它只会是起到显示作用的控件
    /// </summary>
    private static List<string> defaultNameList = new List<string>() { "Image","Text (TMP)","RawImage","Background",
        "Checkmark","Label","Text (Legacy)","Arrow","Placeholder","Fill","Handle","Viewport","Scrollbar Horizontal",
        "Scrollbar Vertical"};


    protected virtual void Awake()
    {
        //为了避免 某一个对象上存在两种控件的情况
        //我们应该优先查找重要的组件
        FindChildrenComponent<Button>();
        FindChildrenComponent<Toggle>();
        FindChildrenComponent<Slider>();
        FindChildrenComponent<InputField>();
        FindChildrenComponent<ScrollRect>();
        FindChildrenComponent<Dropdown>();
        //即使对象上挂在了多个组件 只要优先找到了重要组件
        //之后也可以通过重要组件得到身上其他挂载的内容
        FindChildrenComponent<Text>();
        FindChildrenComponent<TextMeshProUGUI>();
        FindChildrenComponent<Image>();
    }

    /// <summary>
    /// 面板显示时会调用的逻辑
    /// </summary>
    public abstract void Show();
    /// <summary>
    /// 面板隐藏时会调用的逻辑
    /// </summary>
    public abstract void Hide();

    private void FindChildrenComponent<T>() where T : UIBehaviour
    {
        T[] components = this.GetComponentsInChildren<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            //临时变量解决后续的闭包问题
            string controlName = components[i].gameObject.name;
            if (!uiComponentDic.ContainsKey(controlName))
            {
                if(!defaultNameList.Contains(controlName))
                {
                    uiComponentDic.Add(controlName, components[i]);

                    if (components[i] is Button)
                    {
                        //存在闭包
                        (components[i] as Button).onClick.AddListener(() =>
                        {
                            ClickBtn(controlName);
                        });
                    }
                    if (components[i] is Toggle)
                    {
                        (components[i] as Toggle).onValueChanged.AddListener((value) =>
                        {
                            ToggleValueChange(controlName, value);
                        });
                    }
                    if (components[i] is Slider)
                    {
                        (components[i] as Slider).onValueChanged.AddListener((value) =>
                        {
                            SliderValueChange(controlName, value);
                        });
                    }
                }

            }
        }
    }
    protected virtual void ClickBtn(string btnName)
    {

    }
    protected virtual void SliderValueChange(string sliderName, float value)
    {

    }
    protected virtual void ToggleValueChange(string toggleName, bool value)
    {

    }

    public T GetUIComponent<T>(string name)where T : UIBehaviour
    {
        if(uiComponentDic.ContainsKey(name))
        {
            T com = uiComponentDic[name] as T;
            if(com == null)
            {
                Debug.LogError($"不存在名称为{name}且类型为{typeof(T).Name}的组件");
            }
            return com;
        }
        else
        {
            Debug.LogError($"不存在名称为{name}的组件");
            return null;
        }
    }
}
