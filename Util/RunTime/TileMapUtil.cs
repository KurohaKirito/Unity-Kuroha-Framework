using Kuroha.Framework.UI.RunTime.Manager;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Kuroha.Util.RunTime
{
    public static class TileMapUtil
    {
        /// <summary>
        /// 获取当前鼠标所在的瓦片位置
        /// </summary>
        /// <returns></returns>
        public static Vector3Int GetTilePositionOfMouse(Tilemap tileMap)
        {
            // 鼠标坐标系即屏幕坐标系 (0,0) ~ (1920,1080)
            var mouseScreenPosition = UnityEngine.Input.mousePosition;

            // 但是需要的不是屏幕坐标系, 需要的是游戏中的世界坐标系
            var mouseWorldPosition = UIManager.Instance.MainCamera.ScreenToWorldPoint(mouseScreenPosition);

            // 最后还需要将转换后的坐标调整为瓦片地图坐标
            // 由于转换得来的坐标系是基于主摄像机转换的, 因此带有主摄像机的一些特性, 比如 Z 坐标和 主摄像机的一致, 而瓦片地图有自身的 Z 轴坐标, 因此必须单独调整 Z 轴坐标
            // 前景层和背景层的 Z 轴坐标是相同的, 使用哪个都可以, 这里使用背景层的
            var mouseWorldPositionZ = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, tileMap.transform.position.z);

            // 瓦片地图有自身的偏移量, 因此坐标也要进行相应的偏移, WorldToCell 方法会进行自动调整
            return tileMap.WorldToCell(mouseWorldPositionZ);
        }
    }
}
