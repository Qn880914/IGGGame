using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

//禁用通过菜单创建的Img 和 Txt 的鼠标事件 (优化)

public class UIRayCasterEnable {

    [MenuItem("GameObject/UI/Image", false, 10)]
    static void CreateImage(MenuCommand menuCommand)
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Image", typeof(Image));
                go.GetComponent<Image>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localPosition = new Vector3(0, 0, 0);
                go.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    [MenuItem("GameObject/UI/Text", false, 10)]
    static void CreateText(MenuCommand menuCommand)
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Text", typeof(Text));
                go.GetComponent<Text>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localPosition = new Vector3(0, 0, 0);
                go.transform.localScale = new Vector3(1, 1, 1);
                RectTransform rc = go.transform as RectTransform;
                rc.sizeDelta = new Vector2(100, 30);
            }
        }
    }
}
