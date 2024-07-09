using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WH.Controls.SingleInstance
{
    /// <summary>
    /// 20240701 TCG
    /// 创建单例窗体
    /// </summary>
    public class SingleInstance
    {
        static Hashtable s_typeList = new Hashtable();
        /// <summary>
        /// 20240701 TCG
        /// 系统配置等窗口 全局唯一窗口单例，要求无参构造
        /// IOC容器中注册使用，懒加载模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <returns></returns>
        public static T Create<T,W>() where T : Lazy<W>,new() where W : Window,new()
        {
            if (s_typeList.ContainsKey(typeof(T)))
            {
                if(s_typeList[typeof(T)] is not null)
                    return (T)s_typeList[typeof(T)];
                else
                {
                    T t = new T();
                    
                    s_typeList[typeof(T)] = t;
                    t.Value.Closed += (s, e) => s_typeList[typeof(T)] = null;
                    return t;
                }
            }
            else
            {
                T t = new T();
               
                s_typeList.Add(typeof(T), t);
                t.Value.Closed += (s, e) => s_typeList[typeof(T)] = null;
                return t;
            }
        }
        /// <summary>
        /// 20240709 TCG
        /// 单例窗体容器 不要求无参构造
        /// 存在且不为空则返回现有实例，否则添加到容器并返回当前实例
        /// </summary>
        public static T Add<T>(T window, string key) where T : Window
        {
            if (s_typeList.ContainsKey(key))
            {
                if (s_typeList[key] is not null)
                    return (T)s_typeList[key];
                else
                {
                   
                    s_typeList[key] = window;
                    window.Closed += (s, e) => s_typeList[key] = null;
                    return window;
                }
            }
            else
            {
                
                s_typeList.Add(key, window);
                window.Closed += (s, e) => s_typeList[key] = null;
                return window;
            }
        }
    }
    
   
}
