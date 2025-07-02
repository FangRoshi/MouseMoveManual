using System;
using System.Collections.Generic;
using UnityEngine;

namespace MouseMoveManual
{
    public static class AnimationUI
    {
        //private static bool showDropdown = false;
        private static int selectedIndex = 0;
        
        private static Vector2 scrollPosition;     // 滚动视图位置
        private static float[] itemPositions; // 存储每一项的Y位置
        private static bool needRecalculatePositions = true;
        private static int buttonHeight = 20;
        private static void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(400f));
            {
                // 左侧滚动选择区域 (占屏幕宽度30%)
                //DrawSelectionPanel();
                // 右侧详细信息区域 (占屏幕宽度70%)
                DrawDetailPanel();;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        public static void DrawSelectionPanel()
        {
            if (MouseMoveManual.bean != null)
            {
                selectedIndex = Array.FindIndex(MouseMoveManual.options, x => x == MouseMoveManual.bean.nameAnimation);
            }
            if(selectedIndex < 0) 
            { 
                selectedIndex = 0;
            }
            //Debug.Log("MouseMoveManual.options=========>" + MouseMoveManual.options[1]);
            //Debug.Log("2MouseMoveManual.bean.nameAnimation=========>" + MouseMoveManual.bean.nameAnimation);
            //Debug.Log("selectedIndex=========>" + selectedIndex);
            
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200f));
            {
                GUILayout.Label("动作选择", GUILayout.Height(30));

                //计算滚动区域高度（屏幕高度减去可能的边距）
                float scrollViewHeight = window.height - 20;
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                Debug.Log("needRecalculatePositions=========>" + needRecalculatePositions);
                // 计算每一项的位置（仅首次或窗口大小变化时计算）
                if (needRecalculatePositions)
                {
                    CalculateItemPositions();
                    float targetY = itemPositions[selectedIndex] - scrollViewHeight / 2 + buttonHeight; // 20是按钮高度的一半
                    Debug.Log("targetY=========>" + targetY);
                    scrollPosition.y = Mathf.Clamp(targetY, 0, GetTotalContentHeight() - scrollViewHeight);
                    Debug.Log("scrollPosition.y=========>" + scrollPosition.y);
                    needRecalculatePositions = false;
                }

                {
                    for (int i = 0; i < MouseMoveManual.options.Length; i++)
                    {
                        // 使用不同样式显示选中项
                        GUIStyle style = (i == selectedIndex) ?
                            new GUIStyle(GUI.skin.button) { normal = { textColor = new Color(0.8f, 0.2f, 0.2f, 1.0f) }, fontStyle = FontStyle.Bold } :
                            GUI.skin.button;

                        if (GUILayout.Button(MouseMoveManual.options[i], style, GUILayout.Height(buttonHeight)))
                        {
                            selectedIndex = i;
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        public static void DrawDetailPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(400f));
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label("nowAnimation:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                GUILayout.Label(MouseMoveManual.bean.nameAnimation);
                if (GUILayout.Button("Save This", Array.Empty<GUILayoutOption>()))
                {
                    MouseMoveManual.saveBean(MouseMoveManual.bean);
                    Tools.SaveListToXml(MouseMoveManual.beanList, MouseMoveManual.savePath);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("feelStep:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                GUILayout.Label(MouseMoveManual.bean.feelStep.ToString());
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("isFaintness:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                GUILayout.Label(MouseMoveManual.bean.isFaintness.ToString());
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("StartProgress:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                }) ;
               // GUILayout.Label(minValue.ToString());
                MouseMoveManual.bean.StartProgress = GUILayout.HorizontalSlider(MouseMoveManual.bean.StartProgress, minValue, maxValue, GUILayout.Width(160));
                // GUILayout.Label(maxValue.ToString());
                GUILayout.Label(MouseMoveManual.bean.StartProgress.ToString("F2"));
                GUILayout.EndHorizontal();
                

                GUILayout.BeginHorizontal();
                GUILayout.Label("MidProgress:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                MouseMoveManual.bean.MidProgress = GUILayout.HorizontalSlider(MouseMoveManual.bean.MidProgress, 0, 3, GUILayout.Width(160));
                GUILayout.Label(MouseMoveManual.bean.MidProgress.ToString("F2"));
                GUILayout.EndHorizontal();
                

                GUILayout.BeginHorizontal();
                GUILayout.Label("TProgress:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                MouseMoveManual.bean.TProgress = GUILayout.HorizontalSlider(MouseMoveManual.bean.TProgress, 0, 3, GUILayout.Width(160));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("ForwardProgress:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                MouseMoveManual.bean.ForwardProgress = GUILayout.HorizontalSlider(MouseMoveManual.bean.ForwardProgress, 0, 3, GUILayout.Width(160));
                GUILayout.Label(MouseMoveManual.bean.ForwardProgress.ToString("F2"));
                GUILayout.EndHorizontal();
                

                
                //GUILayout.Label("TProgress's value: " + MouseMoveManual.bean.TProgress.ToString("F2"));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Multiple:", new GUILayoutOption[]{
                    GUILayout.Width(128f)
                });
                MouseMoveManual.bean.Multiple = GUILayout.HorizontalSlider(MouseMoveManual.bean.Multiple, 0, 4, GUILayout.Width(160));
                GUILayout.Label(MouseMoveManual.bean.Multiple.ToString("F2"));
                GUILayout.EndHorizontal();
                
            }
            GUILayout.EndVertical();
        }

            // 计算每一项的累积Y位置
            static void CalculateItemPositions()
        {
            itemPositions = new float[MouseMoveManual.options.Length];
            float currentY = 0;

            for (int i = 0; i < MouseMoveManual.options.Length; i++)
            {
                itemPositions[i] = currentY;
                currentY += buttonHeight; // 按钮高度 + 可能的间距
            }
        }

        // 计算滚动区域总高度
        static float GetTotalContentHeight()
        {
            return MouseMoveManual.options.Length * buttonHeight; // 所有按钮的总高度
        }

        public static void DrawAnimationUI()
        {
            AnimationUI.window = GUILayout.Window(789456155, AnimationUI.window, new GUI.WindowFunction(AnimationUI.DrawWindow), "Animation Selection UI", new GUILayoutOption[]
            {
                GUILayout.Width(400f),
                GUILayout.Height(180f)
            });
        }

        private static Rect window = new Rect(0f, 300f, 400, 180);
        private static float minValue = 0f;
        private static float maxValue = 4f;

    }
}
