using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 20240828 TCG
    /// 动态程序集
    /// </summary>
    public class DynamicAssembly
    {
        private static byte[] loadFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[(int)fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            fs.Dispose();
            return buffer;
        }

        /// <summary>
        /// 20240828 TCG
        /// 加载dll程序集
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(string filename)
        {
            return Assembly.Load(loadFile(filename));
        }
    }
}
