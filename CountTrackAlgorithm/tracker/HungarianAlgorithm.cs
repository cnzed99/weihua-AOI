using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloDeployPlatform.tracker
{
    public static class HungarianAlgorithm
    {
        public static (int[] rows, int[] cols) FindAssignments(float[,] costs, bool maximize)
        {
            // 如果需要最大化收益，则转换为最小化问题
            if (maximize)
            {
                var max = costs.Cast<float>().Max();
                costs = costs.Clone() as float[,];
                for (int i = 0; i < costs.GetLength(0); i++)
                {
                    for (int j = 0; j < costs.GetLength(1); j++)
                    {
                        costs[i, j] = max - costs[i, j];
                    }
                }
            }

            int n = costs.GetLength(0);
            int m = costs.GetLength(1);

            // 初始化标记矩阵
            var marks = new int[n, m];

            // 步骤1: 行归约
            for (int i = 0; i < n; i++)
            {
                float min = float.MaxValue;
                for (int j = 0; j < m; j++)
                {
                    if (costs[i, j] < min)
                    {
                        min = costs[i, j];
                    }
                }

                for (int j = 0; j < m; j++)
                {
                    costs[i, j] -= min;
                }
            }

            // 步骤2: 列归约
            for (int j = 0; j < m; j++)
            {
                float min = float.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    if (costs[i, j] < min)
                    {
                        min = costs[i, j];
                    }
                }

                for (int i = 0; i < n; i++)
                {
                    costs[i, j] -= min;
                }
            }

            // 步骤3: 寻找独立零元素
            var rowCover = new bool[n];
            var colCover = new bool[m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (Math.Abs(costs[i, j]) < float.Epsilon && !rowCover[i] && !colCover[j])
                    {
                        marks[i, j] = 1;
                        rowCover[i] = true;
                        colCover[j] = true;
                    }
                }
            }

            // 转换为结果格式
            var rows = new List<int>();
            var cols = new List<int>();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (marks[i, j] == 1)
                    {
                        rows.Add(i);
                        cols.Add(j);
                        break;
                    }
                }
            }

            return (rows.ToArray(), cols.ToArray());
        }
    }
}