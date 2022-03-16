using System;
using System.Collections.Generic;

namespace Kuroha.Framework.Pool.RunTime
{
    public class GameObjectPool<T> where T : IPoolGameObject, new()
    {
        /// <summary>
        /// 对象池游戏物体的创建方法
        /// </summary>
        private readonly Func<T> createFunc;

        /// <summary>
        /// 指定泛型的对象池 (队列: 先被回收的, 也会被先用)
        /// </summary>
        private Queue<T> objectPool = new Queue<T>();

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="createFunc"></param>
        public GameObjectPool(Func<T> createFunc)
        {
            this.createFunc = createFunc;
        }

        /// <summary>
        /// 预装对象到对象池
        /// </summary>
        /// <param name="reserveCount">需要预装的数量</param>
        /// <param name="create">创建新物体的方法</param>
        public void Reserve(int reserveCount, Func<T> create = null)
        {
            while (objectPool.Count < reserveCount)
            {
                objectPool.Enqueue(create != null ? create() : createFunc());
            }
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        public T GetFromPool(Func<T> create = null)
        {
            T poolObject;

            if (objectPool.Count > 0)
            {
                poolObject = objectPool.Dequeue();
            }
            else
            {
                poolObject = create != null ? create() : createFunc();
            }

            poolObject.Enable();
            return poolObject;
        }

        /// <summary>
        /// 将对象重置, 并归还给对象池
        /// </summary>
        /// <param name="poolObject">要归还的对象</param>
        public void BackToPool(T poolObject)
        {
            poolObject.Disable();
            objectPool.Enqueue(poolObject);
        }

        /// <summary>
        /// 清空整个对象池, 释放内存
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var poolObject in objectPool)
            {
                poolObject.Release();
            }

            objectPool.Clear();
            objectPool = null;
        }
    }
}
