/*
 * Create: @Lin Zidong [2018.07.12]
 */
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

interface ISingleton<T>
{
    /*
     该方法已作废
     ps.本来是想弄一个支持泛型的单例模式的类，使得继承该接口/类的子类自动成为符合单例模式的model类
        然而，由于这样的做法需要 where T:new(), 而这个要求必须有一个公共的构造方法，这一点直接破坏了单例模式的要求（不允许另外再构造）
        因此放弃该做法，改为还是由子类复制以下代码，并用子类替换T来实现
        另外还有一个做法是通过unity的awake来实现的，在下面可供参考
     */
    #region 单例模式Singleton
    //private T() { }
    //static T() { }
    //private static readonly T _instance = new T();
    //public static T Instance { get { return _instance; } }
    #endregion

    /*利用Unity的awake实现方法如下(该方法只能算伪单例模式)
     * 
    public abstract class Singleton<T> : MonoBehaviour
    where T : MonoBehaviour
    {
        private static T m_instance = null;

        public static T Instance
        {
            get { return m_instance; }
        }

        protected virtual void Awake()
        {
            m_instance = this as T;
        }
    }
    */

    /*补充：这就是用了new()被放弃的实现方式
     * 
    public class Singleton<T> where T : class, new()
    {
        private static object _syncobj = new object();
        private static volatile T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncobj)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
        public Singleton()
        { }
    }
    */
}
