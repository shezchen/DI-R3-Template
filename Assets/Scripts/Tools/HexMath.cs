using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Tools
{
    public static class HexMath
    {
        /// <summary>
        /// 获取指定中心点、指定半径内的所有六边形坐标（立方体坐标系）
        /// </summary>
        /// <param name="center">中心点坐标 (x, y, z)</param>
        /// <param name="range">半径 N (0表示只包含中心点)</param>
        /// <returns>坐标列表</returns>
        public static List<Vector3Int> GetTilesInRange(Vector3Int center, int range)
        {
            List<Vector3Int> results = new List<Vector3Int>();

            // 1. 遍历 x 轴的偏移量 dx
            for (int dx = -range; dx <= range; dx++)
            {
                // 2. 计算 y 轴偏移量 dy 的边界
                int dyMin = Mathf.Max(-range, -dx - range);
                int dyMax = Mathf.Min(range, -dx + range);

                for (int dy = dyMin; dy <= dyMax; dy++)
                {
                    // 3. 根据 x + y + z = 0 计算 dz
                    int dz = -dx - dy;

                    // 4. 加上中心点坐标得到最终坐标
                    results.Add(center + new Vector3Int(dx, dy, dz));
                }
            }

            return results;
        }
        
        /// <summary>
        /// 将 Unity 的 Offset 坐标 (row, col) 转换为 Cube 坐标 (q, r, s)
        /// 当前约定：
        /// - 尖顶六边形（Pointy-Top）
        /// - 使用 Odd-R offset：奇数行向右偏移半格
        /// - unityOffset: x = row, y = col （Swizzle = XYZ）
        /// </summary>
        public static CubeCoordinate OffsetToCube(Vector3Int unityOffset)
        {
            int row = unityOffset.y;
            int col = unityOffset.x;
            
            int q = col - (row - (row & 1)) / 2;
            int r = row;

            int s = -q - r;

            return new CubeCoordinate(q, r, s);
        }

        /// <summary>
        /// 将 Cube 坐标 (q, r, s) 转回 Unity 的 Offset 坐标（返回值已交换x,y）
        /// 反向使用尖顶 Odd-R offset 公式：
        /// row = r
        /// col = q + (row - (row & 1)) / 2
        /// </summary>
        public static Vector3Int CubeToOffset(CubeCoordinate cube)
        {
            int q = cube.Q;
            int r = cube.R;
            int s = cube.S;
            
            int row = r;
            int col = q + (row - (row & 1)) / 2;
            
            return new Vector3Int(col,row, 0);
        }
    }
}