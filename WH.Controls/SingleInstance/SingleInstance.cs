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
    /// 创建单例窗体
    /// </summary>
    public class SingleInstance
    {
        static Hashtable s_typeList = new Hashtable();

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
    }
}
