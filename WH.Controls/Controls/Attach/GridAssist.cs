using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WH.Controls.Controls.Attach
{
    static class GridAssist
    {
        #region AutoRowColumn

        public static string GetAutoRowColumn(DependencyObject obj)
        {
            return (string)obj.GetValue(AutoRowColumnProperty);
        }

        public static void SetAutoRowColumn(DependencyObject obj, string value)
        {
            obj.SetValue(AutoRowColumnProperty, value);
        }

        /// <summary>
        /// 自动排列 Grid 容器中的所有控件
        /// </summary>
        /// <remarks>
        /// 值为一个逗号隔开的字符串，形如：<br/>
        /// - 2,3（2 行 3 列，列宽默认为 1*，行高为 Auto）<br/>
        /// - _,3（3 列，行数根据子控件而定）<br/>
        /// - 2,3,Auto（列的宽度为 Auto）<br/>
        /// Grid 中的所有控件将自动按照从左到右、从上到下的顺序进行排列<br/>
        /// 控件各自的 RowSpan 以及 ColumnSpan 也会被考虑
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <Window xmlns:ap="clr-namespace:NemoDemo.AttachedProperties">
        ///     <Grid ap:GridHelper.AutoRowColumn="2,3">
        ///         <Button />                     // 1,1
        ///         <Label />                      // 1,2
        ///         <TextBox />                    // 1,3
        ///         <Button />                     // 2,1
        ///         <Label Grid.ColumnSpan="2" />  // 2,2-3
        ///     </Grid>
        /// </Window>
        /// ]]>
        /// </code>
        /// </example>
        public static readonly DependencyProperty AutoRowColumnProperty = DependencyProperty.RegisterAttached(
            "AutoRowColumn",
            typeof(string),
            typeof(GridAssist),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsMeasure, OnAutoRowColumnChanged)
        );

        private static void OnAutoRowColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Grid grid))
                return;

            var value = e.NewValue as string;
            if (string.IsNullOrEmpty(value))
            {
                grid.RowDefinitions.Clear();
                grid.ColumnDefinitions.Clear();

                grid.Loaded -= OnGridLoaded;
                return;
            }

            grid.Loaded += OnGridLoaded;

            if (grid.IsLoaded)
                OnGridLoaded(grid, null);
        }

        private static void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;

            // 列宽，默认为 Star，即平均分布
            var width = new GridLength(1.0, GridUnitType.Star);
            var split = GridAssist.GetAutoRowColumn(grid).Split(',');
            var r = split[0] != "_" ? int.Parse(split[0]) : 1;
            var c = int.Parse(split[1]);
            // 如果有第三个参数且值为 auto，则宽度为 Auto
            if (split.Length == 3 && split[2].Equals("auto", StringComparison.OrdinalIgnoreCase))
                width = GridLength.Auto;
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < r; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < c; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var cols = grid.ColumnDefinitions.Count;

            var map = new List<bool[]>();

            int x = 0,
                y = 0;
            foreach (UIElement item in grid.Children)
            {
                var rowSpan = Grid.GetRowSpan(item);
                var colSpan = Grid.GetColumnSpan(item);

                // 默认从上到下，从左到右，即任何时候控件的下方和右方都是空的
                // 可能会出现中途有一个 RowSpan > 1 导致其左下方的右侧不为空的情况，暂不处理
                // 同时默认当前的 (x, y) 位置是一个可用位置

                // 当前控件占据的格子
                for (int i = 0; i < rowSpan; i++)
                {
                    // 如果 RowDefinition 不够用，则自动添加
                    if (map.Count <= y + i)
                    {
                        while (map.Count <= y + i)
                        {
                            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            map.Add(new bool[cols]);
                        }
                    }

                    map[y + i][x] = true;
                }

                for (int i = 0; i < colSpan; i++)
                {
                    if (x + i >= cols)
                        break;
                    map[y][x + i] = true;
                }

                Grid.SetRow(item, y);
                Grid.SetColumn(item, x);

                // 寻找下一个可用的格子
                while (map[y][x])
                {
                    x++;
                    if (x >= cols)
                    {
                        x = 0;
                        y++;
                        if (y >= map.Count)
                        {
                            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            map.Add(new bool[cols]);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
