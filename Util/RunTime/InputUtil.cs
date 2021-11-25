using UnityEngine;

namespace Kuroha.Util.RunTime
{
    public static class InputUtil
    {
        /// <summary>
        /// 检测鼠标左键按下
        /// </summary>
        /// <returns>是否按下</returns>
        public static bool GetMouseDown_Left()
        {
            return UnityEngine.Input.GetMouseButtonDown(0);
        }

        /// <summary>
        /// 检测鼠标右键按下
        /// </summary>
        /// <returns>是否按下</returns>
        public static bool GetMouseDown_Right()
        {
            return UnityEngine.Input.GetMouseButtonDown(1);
        }

        /// <summary>
        /// 检测鼠标中键按下
        /// </summary>
        /// <returns>是否按下</returns>
        public static bool GetMouseDown_Middle()
        {
            return UnityEngine.Input.GetMouseButtonDown(2);
        }

        /// <summary>
        /// 检测鼠标左键松开
        /// </summary>
        /// <returns>是否松开</returns>
        public static bool GetMouseUp_Left()
        {
            return UnityEngine.Input.GetMouseButtonUp(0);
        }

        /// <summary>
        /// 检测鼠键右键松开
        /// </summary>
        /// <returns>是否松开</returns>
        public static bool GetMouseUp_Right()
        {
            return UnityEngine.Input.GetMouseButtonUp(1);
        }

        /// <summary>
        /// 检测鼠标中键松开
        /// </summary>
        /// <returns>是否松开</returns>
        public static bool GetMouseUp_Middle()
        {
            return UnityEngine.Input.GetMouseButtonUp(2);
        }

        /// <summary>
        /// 检测特定键盘上按键的按下
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>是否按下</returns>
        public static bool GetKeyCodeDown(UnityEngine.KeyCode keyCode)
        {
            return UnityEngine.Input.GetKeyDown(keyCode);
        }

        /// <summary>
        /// 检测特定键盘上按键的松开
        /// </summary>
        /// <param name="keyCode">按键代码</param>
        /// <returns>是否松开</returns>
        public static bool GetKeyCodeUp(UnityEngine.KeyCode keyCode)
        {
            return UnityEngine.Input.GetKeyUp(keyCode);
        }

        /// <summary>
        /// 检测鼠标滑轮输入
        /// </summary>
        /// <returns>鼠标滑轮的状态值</returns>
        public static float GetAxisMouseScrollWheel()
        {
            return UnityEngine.Input.GetAxis("Mouse ScrollWheel");
        }

        /// <summary>
        /// 检测任意键
        /// </summary>
        /// <returns></returns>
        public static bool GetAnyClick()
        {
            return UnityEngine.Input.anyKeyDown;
        }

        /// <summary>
        /// 获取鼠标坐标
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }
    }
}
