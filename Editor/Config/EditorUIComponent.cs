using System;
using UnityEngine;

namespace IGG.EditorTools.UI
{
    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.14
    /// Desc    编辑器文字组件
    /// </summary>
    public class Label
    {
        public Rect Rect;
        public string Text;

        public void Draw()
        {
            GUI.Label(Rect,Text);
        }
    }
    
    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.14
    /// Desc    编辑器文字框组件
    /// </summary>
    public class TextField
    {
        public Rect Rect;
        public string Text;

        public TextField(string text = "")
        {
            Text = text;
            Rect = new Rect();
        }

        public void Draw()
        {
            Text = GUI.TextField(Rect, Text);
        }
    }

    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.25
    /// Desc    编辑器输入框组件
    /// </summary>
    public class Input
    {
        public Rect Rect;
        public string Text = "";

        /// <summary>
        /// 输入的内容是否为空
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Text);
            }
        }

        public string Label
        {
            get { return m_label; }
            set
            {
                m_label = value;
                m_labelIsNull = string.IsNullOrEmpty(value);
                UpdateLabelWidth();
            }
        }

        public void RectChange()
        {
            m_rectChange = true;
        }

        public void Draw()
        {
            GUI.Label(Rect, m_label);
            if (m_rectChange)
            {
                UpdateLabelWidth();
            }
            Text = GUI.TextField(m_txtRect, Text);
        }

        private void UpdateLabelWidth()
        {
            m_txtRect = Rect;
            if (m_labelIsNull)
            {
                return;
            }

            var size = GUI.skin.label.CalcSize(new GUIContent(m_label));
            m_txtRect.xMin += size.x;
            m_rectChange = false;
        }

        private Rect m_txtRect;
        private bool m_rectChange = true;
        private string m_label;
        private bool m_labelIsNull = true;
    }

    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.14
    /// Desc    编辑器按钮组件
    /// </summary>
    public class Button
    {
        public Rect Rect;
        public string Label;
        public bool Select;
        public Action<Button> OnClick;

        public void Draw()
        {
            if (GUI.Button(Rect, Label))
            {
                if (OnClick != null)
                {
                    OnClick(this);
                }
            }
        }
    }

    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.14
    /// Desc    复选框组件
    /// </summary>
    public class Toggle
    {
        public Rect Rect;
        public string Label;
        public bool Select;
        public Action<Toggle> OnChange;

        /// <summary>
        /// 附加的数据
        /// </summary>
        public object Data;

        public void Draw()
        {
            bool newSelect = GUI.Toggle(Rect, Select, Label);
            bool hasChange = newSelect != Select;
            Select = newSelect;
            if (OnChange != null && hasChange)
            {
                OnChange(this);
            }
        }
    }

    /// <summary>
    /// Author  gaofan
    /// Date    2017.12.14
    /// Desc    滚动组件
    /// </summary>
    public class ScrollView
    {
        public Vector2 ScrollPoint = Vector2.zero;
        public Rect Position;
        public Rect ViewRect;

        public void Begin()
        {
            ScrollPoint = GUI.BeginScrollView(Position, ScrollPoint, ViewRect);
        }

        public void End()
        {
            GUI.EndScrollView();
        }
    }
}