using System;
using System.Collections.Generic;

namespace Kuroha.Framework.KString
{
    public class KString
    {
        /// <summary>
        /// Block 块数量  
        /// </summary>
        private const int INITIAL_BLOCK_CAPACITY = 32;
        
        /// <summary>
        /// 缓存字典容量  128 * 4 = 512 Byte
        /// </summary>
        private const int INITIAL_CACHE_CAPACITY = 128;
        
        /// <summary>
        /// 缓存字典每个 Stack 默认 KString 容量
        /// </summary>
        private const int INITIAL_STACK_CAPACITY = 48;

        private const int INITIAL_OPEN_CAPACITY = 5; // 默认打开层数为5
        
        private const int INITIAL_SHALLOW_CAPACITY = 100; // 默认50个浅拷贝用
        
        private const char NEW_ALLOC_CHAR = 'X'; // 默认 char
        
        private const int CHAR_LENGTH_THIS_PLATFORM = sizeof(char);
        
        #region 结构体

        private struct Byte8192
        {
            private Byte4096 a1;
            private Byte4096 a2;
        }

        private struct Byte4096
        {
            private Byte2048 a1;
            private Byte2048 a2;
        }

        private struct Byte2048
        {
            private Byte1024 a1;
            private Byte1024 a2;
        }

        private struct Byte1024
        {
            private Byte512 a1;
            private Byte512 a2;
        }

        private struct Byte512
        {
            private Byte256 a1;
            private Byte256 a2;
        }

        private struct Byte256
        {
            private Byte128 a1;
            private Byte128 a2;
        }

        private struct Byte128
        {
            private Byte64 a1;
            private Byte64 a2;
        }

        private struct Byte64
        {
            private Byte32 a1;
            private Byte32 a2;
        }

        private struct Byte32
        {
            private Byte16 a1;
            private Byte16 a2;
        }

        private struct Byte16
        {
            private Byte8 a1;
            private Byte8 a2;
        }

        private struct Byte8
        {
            private long a1;
        }

        private struct Byte4
        {
            private int a1;
        }

        private struct Byte2
        {
            private short a;
        }

        private struct Byte1
        {
            private byte a;
        }

        #endregion

        // using 语法所用
        // 从 KStringBlock 栈中取出一个 block 并将其置为当前 g_current_block
        // 在代码块 {} 中新生成的 KString 都将push入块内部stack中
        // 当离开块作用域时，调用块的 Dispose 函数，将内栈中所有 KString 填充初始值并放入KString缓存栈
        // 同时将自身放入 block 缓存栈中。
        // （此处有个问题：使用Stack缓存block，当block被dispose放入Stack后g_current_block仍然指向此block，无法记录此block之前的block，这样导致KString.Block()无法嵌套使用）
        public static IDisposable Block()
        {
            g_current_block = g_blocks.Count == 0 ? new KStringBlock(INITIAL_BLOCK_CAPACITY * 2) : g_blocks.Pop();

            g_open_blocks.Push(g_current_block); //新加代码，将此玩意压入open栈
            return g_current_block.Begin();
        }
        
        /// <summary>
        /// KStringBlock
        /// </summary>
        private class KStringBlock : IDisposable
        {
            private readonly Stack<KString> stack;

            internal KStringBlock(int capacity)
            {
                stack = new Stack<KString>(capacity);
            }

            internal void Push(KString str)
            {
                stack.Push(str);
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <returns></returns>
            internal IDisposable Begin()
            {
                return this;
            }

            /// <summary>
            /// 析构函数
            /// </summary>
            void IDisposable.Dispose()
            {
                // 循环调用栈中所有的 Dispose 方法
                while (stack.Count > 0)
                {
                    stack.Pop().Dispose();
                }

                g_blocks.Push(this); //将自身push入缓存栈

                //赋值currentBlock
                g_open_blocks.Pop();
                g_current_block = g_open_blocks.Count > 0 ? g_open_blocks.Peek() : null;
            }
        }

        private static Queue<KString>[] coreCache; // idx 特定字符串长度,深拷贝核心缓存
        private static Dictionary<int, Queue<KString>> secondCache; //key特定字符串长度value字符串栈，深拷贝次级缓存
        private static Stack<KString> shallowCache; //浅拷贝缓存

        private static Stack<KStringBlock> g_blocks; // KStringBlock 缓存栈
        private static Stack<KStringBlock> g_open_blocks; // KString 已经打开的缓存栈
        private static KStringBlock g_current_block; // KString 所在的 block 块
        
        /// <summary>
        /// 是否浅拷贝
        /// </summary>
        private readonly bool isShallow;

        [NonSerialized]
        private string currentString;

        [NonSerialized]
        private bool disposedFlag;

        /// <summary>
        /// 带默认长度的构造
        /// </summary>
        private KString(int length)
        {
            currentString = new string(NEW_ALLOC_CHAR, length);
        }

        /// <summary>
        /// 浅拷贝专用构造
        /// </summary>
        private KString(string value)
        {
            isShallow = true;
            currentString = value;
        }

        /// <summary>
        /// 静态构造
        /// </summary>
        static KString()
        {
            Initialize(INITIAL_CACHE_CAPACITY,
                INITIAL_STACK_CAPACITY,
                INITIAL_BLOCK_CAPACITY,
                INITIAL_OPEN_CAPACITY,
                INITIAL_SHALLOW_CAPACITY);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        private void Dispose()
        {
            if (disposedFlag)
            {
                throw new ObjectDisposedException(this);
            }

            // 浅拷贝
            if (isShallow)
            {
                shallowCache.Push(this);
            }
            // 深拷贝
            else
            {
                var stack = coreCache.Length > Length ? coreCache[Length] : secondCache[Length];
                stack.Enqueue(this);
            }

            disposedFlag = true;
        }
        
       

        //将字符拷贝到dst指定index位置
        
        

        // cache_capacity  缓存栈字典容量
        // stack_capacity  缓存字符串栈容量
        // block_capacity  缓存栈容量
        // intern_capacity 缓存
        // open_capacity   默认打开层数
        private static void Initialize(int cache_capacity, int stack_capacity, int block_capacity, int open_capacity, int shallowCache_capacity)
        {
            coreCache = new Queue<KString>[cache_capacity];
            secondCache = new Dictionary<int, Queue<KString>>(cache_capacity);
            g_blocks = new Stack<KStringBlock>(block_capacity);
            g_open_blocks = new Stack<KStringBlock>(open_capacity);
            shallowCache = new Stack<KString>(shallowCache_capacity);
            
            for (var c = 0; c < cache_capacity; c++)
            {
                var stack = new Queue<KString>(stack_capacity);
                for (var j = 0; j < stack_capacity; j++)
                {
                    stack.Enqueue(new KString(c));
                }
                coreCache[c] = stack;
            }

            for (var i = 0; i < block_capacity; i++)
            {
                var block = new KStringBlock(block_capacity * 2);
                g_blocks.Push(block);
            }

            for (var i = 0; i < shallowCache_capacity; i++)
            {
                shallowCache.Push(new KString(null));
            }
        }

        

        //获取 hashcode
        

        

        // 将 src 指定 length 内容拷入 dst, dst 下标 src_offset 偏移
        private static unsafe void MemoryCopy(char* dst, char* source, int length, int src_offset)
        {
            MemoryCopy(dst + src_offset, source, length);
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        
        
        
        

        


        #region 外部类型转换
        
        /// <summary>
        /// 默认小数精度
        /// </summary>
        private static uint decimalAccuracy = 3;
        
        /// <summary>
        /// long    => KString
        /// </summary>
        public static unsafe implicit operator KString(long value)
        {
            var negativeLength = value < 0 ? 1 : 0;
            if (negativeLength == 1)
            {
                value = -value;
            }
            
            // 得到整数位数
            var digitCount = GetDigitCount(value);
            
            var kStringStack = GetKStringOfLength(digitCount + negativeLength);
            
            fixed (char* ptr = kStringStack.currentString)
            {
                // 插入负号
                if (negativeLength == 1) { *ptr = '-'; }
                
                // 插入数字
                LongCopy(ptr, value, negativeLength, digitCount);
            }

            return kStringStack;
        }

        /// <summary>
        /// int     => KString
        /// </summary>
        public static unsafe implicit operator KString(int value)
        {
            var negativeLength = value < 0 ? 1 : 0;
            if (negativeLength == 1)
            {
                value = -value;
            }
            
            // 得到整数位数
            var digitCount = GetDigitCount(value);
            
            var kStringStack = GetKStringOfLength(digitCount + negativeLength);
            
            fixed (char* ptr = kStringStack.currentString)
            {
                // 插入负号
                if (negativeLength == 1) { *ptr = '-'; }
                
                // 插入数字
                LongCopy(ptr, value, negativeLength, digitCount);
            }

            return kStringStack;
        }
        
        /// <summary>
        /// float   => KString
        /// </summary>
        public static unsafe implicit operator KString(float value)
        {
            var currentDecimalAccuracy = (int) decimalAccuracy;
            
            // 举例: 数字为: -38.8765411, 精度为: 3
            var negativeLength = value < 0 ? 1 : 0;
            if (negativeLength == 1)
            {
                value = -value;
            }
            
            // 计算一个和小数精度相同的整零数, 举例: 1000
            var mul = (long) Math.Pow(10, currentDecimalAccuracy);
            
            // 得到整数, 举例: 38876
            var number = (long) (value * mul);
            
            // 得到小数点左侧的数字, 举例: 38
            var leftNumber = (int) (number / mul);
            
            // 得到小数点右侧的数字, 举例: 876
            var rightNumber = (int)(number % mul);
            
            // 得到左侧数字是几位数, 举例: 2
            var leftDigitCount = GetDigitCount(leftNumber);
            
            // 得到总位数 (小数点也算 1 位), 举例: 6
            var total = leftDigitCount + currentDecimalAccuracy + 1;

            var kStringStack = GetKStringOfLength(total + negativeLength);
            fixed (char* ptr = kStringStack.currentString)
            {
                // 插入负号
                if (negativeLength == 1) { *ptr = '-'; }
                
                // 插入左侧数字
                IntCopy(ptr, leftNumber, negativeLength, leftDigitCount);
                
                // 插入小数点
                *(ptr + negativeLength + leftDigitCount) = '.';
                
                // 插入右侧数字
                IntCopy(ptr, rightNumber, negativeLength + leftDigitCount + 1, currentDecimalAccuracy);
            }
            
            return kStringStack;
        }
        
        /// <summary>
        /// float   => KString
        /// </summary>
        public static KString ToKString(float value, uint accuracy)
        {
            var oldValue = decimalAccuracy;
            decimalAccuracy = accuracy;
            
            KString target = value;
            
            decimalAccuracy = oldValue;
            return target;
        }
        
        /// <summary>
        /// bool    => KString
        /// </summary>
        public static implicit operator KString(bool value)
        {
            return ToKString(value ? "true" : "false");
        }
        
        /// <summary>
        /// KString => string
        /// </summary>
        public static implicit operator string(KString value)
        {
            return value.currentString;
        }
        
        /// <summary>
        /// string  => KString
        /// </summary>
        public static implicit operator KString(string value)
        {
            return ToKStringShallow(value);
        }
        
        #endregion
        
        #region 外部基础操作
        
        // ReSharper disable once UnusedMember.Global
        public char this[int i]
        {
            get => currentString[i];
            set => MemoryCopy(this, value, i);
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public override string ToString()
        {
            return currentString;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public int Length => currentString.Length;
        // ReSharper disable once UnusedMember.Global
        public unsafe bool EndsWith(string postfix)
        {
            if (postfix == null)
            {
                throw new ArgumentNullException(nameof(postfix));
            }

            if (Length < postfix.Length)
            {
                return false;
            }

            fixed (char* ptr_this = currentString)
            {
                fixed (char* ptr_postfix = postfix)
                {
                    for (int i = currentString.Length - 1, j = postfix.Length - 1; j >= 0; i--, j--)
                        if (ptr_this[i] != ptr_postfix[j])
                            return false;
                }
            }

            return true;
        }
        // ReSharper disable once UnusedMember.Global
        public unsafe bool StartsWith(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (Length < prefix.Length)
            {
                return false;
            }

            fixed (char* ptr_this = currentString)
            {
                fixed (char* ptr_prefix = prefix)
                {
                    for (var i = 0; i < prefix.Length; i++)
                        if (ptr_this[i] != ptr_prefix[i])
                            return false;
                }
            }

            return true;
        }
        // ReSharper disable once UnusedMember.Global
        public static KString operator +(KString left, KString right)
        {
            return InternalConcat(left, right);
        }
        // ReSharper disable once UnusedMember.Global
        public static bool operator ==(KString left, KString right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            if (ReferenceEquals(right, null))
            {
                return false;
            }

            return left.currentString == right.currentString;
        }
        // ReSharper disable once UnusedMember.Global
        public static bool operator !=(KString left, KString right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }
            
            if (ReferenceEquals(right, null))
            {
                return false;
            }
            
            return left.currentString != right.currentString;
        }
        // ReSharper disable once UnusedMember.Global
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return ReferenceEquals(this, null);
            }

            var targetK = obj as KString;
            if (targetK != null)
            {
                return targetK.currentString == currentString;
            }

            if (obj is string targetS)
            {
                return targetS == currentString;
            }

            return false;
        }
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return currentString.GetHashCode();
        }

        #endregion
        
        #region 内部操作
        
        /// <summary>
        /// 复制数字到字符串中
        /// </summary>
        private static unsafe void LongCopy(char* dst, long value, int start, int count)
        {
            // 数字 0 的 ASCII 编码为: 48
            var end = start + count;
            for (var i = end - 1; i >= start; i--, value /= 10)
            {
                *(dst + i) = (char)(value % 10 + 48);
            }
        }

        /// <summary>
        /// 复制数字到字符串中
        /// </summary>
        private static unsafe void IntCopy(char* dst, int value, int start, int count)
        {
            // 数字 0 的 ASCII 编码为: 48
            var end = start + count;
            for (var i = end - 1; i >= start; i--, value /= 10)
            {
                *(dst + i) = (char) (value % 10 + 48);
            }
        }
        
        /// <summary>
        /// 计算数字是几位数
        /// </summary>
        private static int GetDigitCount(long value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) { }

            return cnt;
        }

        /// <summary>
        /// 计算数字是几位数
        /// </summary>
        private static int GetDigitCount(int value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) { }

            return cnt;
        }
        
        /// <summary>
        /// string 转换为 KString (浅拷贝)
        /// </summary>
        private static KString ToKStringShallow(string value)
        {
            if (g_current_block == null)
            {
                throw new InvalidOperationException("KString 操作必须在一个 KStringBlock 块中");
            }

            KString result;
            if (shallowCache.Count == 0)
            {
                result = new KString(value);
            }
            else
            {
                result = shallowCache.Pop();
                result.currentString = value;
            }

            result.disposedFlag = false;
            
            g_current_block.Push(result);
            
            return result;
        }
        
        /// <summary>
        /// string 转换为 KString (深拷贝)
        /// </summary>
        private static KString ToKString(string value)
        {
            if (value == null)
            {
                return null;
            }

            var result = GetKStringOfLength(value.Length);
            
            MemoryCopy(target: result, source: value);
            
            return result;
        }
        
        /// <summary>
        /// 得到一个指定长度的 KString
        /// </summary>
        private static KString GetKStringOfLength(int length)
        {
            if (g_current_block == null || length <= 0)
            {
                throw new InvalidOperationException("KString 操作必须在一个 KStringBlock 块中");
            }
            
            // 从缓存中取 Stack
            GetStackInCache(length, out var stack);
            var result = stack.Count == 0 ? new KString(length) : stack.Dequeue();
            result.disposedFlag = false;
            
            // KString 推入块所在栈
            g_current_block.Push(result);
            
            return result;
        }
        
        /// <summary>
        /// 尝试从缓存中取出 KString
        /// </summary>
        private static void GetStackInCache(int needLength, out Queue<KString> outStack)
        {
            var coreLength = coreCache.Length;
            if (coreLength > needLength)
            {
                outStack = coreCache[needLength];
            }
            else
            {
                if (!secondCache.TryGetValue(needLength, out outStack))
                {
                    outStack = new Queue<KString>(INITIAL_STACK_CAPACITY);
                    secondCache[needLength] = outStack;
                }
            }
        }
        
        /// <summary>
        /// 字符串拼接
        /// </summary>
        private static unsafe KString InternalConcat(string left, string right)
        {
            var totalLength = left.Length + right.Length;
            var result = GetKStringOfLength(totalLength);

            fixed (char* ptr_result = result.currentString)
            {
                fixed (char* ptr_left = left)
                {
                    fixed (char* ptr_right = right)
                    {
                        MemoryCopy(ptr_result, ptr_left, left.Length, 0);
                        MemoryCopy(ptr_result, ptr_right, right.Length, left.Length);
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// 将相同长度的 source 内容拷入 target
        /// </summary>
        private static void MemoryCopy(KString target, string source)
        {
            MemoryCopy(target.currentString, source);
        }
        
        /// <summary>
        /// 指定字符插入到指定位置
        /// </summary>
        private static unsafe void MemoryCopy(string target, char source, int index)
        {
            fixed (char* ptr = target)
            {
                ptr[index] = source;
            }
        }
        
        /// <summary>
        /// 将相同长度的 source 内容拷入 target
        /// </summary>
        private static unsafe void MemoryCopy(string target, string source)
        {
            if (target.Length != source.Length)
            {
                throw new InvalidOperationException("两个字符串参数长度不一致。");
            }
            
            fixed (char* dst_ptr = target)
            {
                fixed (char* src_ptr = source)
                {
                    MemoryCopy(dst_ptr, src_ptr, target.Length);
                }
            }
        }
        
        /// <summary>
        /// 将相同长度的 source 内容拷入 target
        /// </summary>
        private static unsafe void MemoryCopy(char* target, char* source, int count)
        {
            ByteCopy( (byte*)target, (byte*)source, count * CHAR_LENGTH_THIS_PLATFORM);
        }
        
        /// <summary>
        /// 字节拷贝
        /// </summary>
        private static unsafe void ByteCopy(byte* dest, byte* src, int byteCount)
        {
            if (byteCount < 128)
            {
                goto g64;
            } 
            
            if (byteCount < 2048)
            {
                goto g1024;
            }

            while (byteCount >= 8192)
            {
                ((Byte8192*)dest)[0] = ((Byte8192*)src)[0];
                dest += 8192;
                src += 8192;
                byteCount -= 8192;
            }

            if (byteCount >= 4096)
            {
                ((Byte4096*)dest)[0] = ((Byte4096*)src)[0];
                dest += 4096;
                src += 4096;
                byteCount -= 4096;
            }

            if (byteCount >= 2048)
            {
                ((Byte2048*)dest)[0] = ((Byte2048*)src)[0];
                dest += 2048;
                src += 2048;
                byteCount -= 2048;
            }

            g1024:
            if (byteCount >= 1024)
            {
                ((Byte1024*)dest)[0] = ((Byte1024*)src)[0];
                dest += 1024;
                src += 1024;
                byteCount -= 1024;
            }

            if (byteCount >= 512)
            {
                ((Byte512*)dest)[0] = ((Byte512*)src)[0];
                dest += 512;
                src += 512;
                byteCount -= 512;
            }

            if (byteCount >= 256)
            {
                ((Byte256*)dest)[0] = ((Byte256*)src)[0];
                dest += 256;
                src += 256;
                byteCount -= 256;
            }

            if (byteCount >= 128)
            {
                ((Byte128*)dest)[0] = ((Byte128*)src)[0];
                dest += 128;
                src += 128;
                byteCount -= 128;
            }

            g64:
            if (byteCount >= 64)
            {
                ((Byte64*)dest)[0] = ((Byte64*)src)[0];
                dest += 64;
                src += 64;
                byteCount -= 64;
            }

            if (byteCount >= 32)
            {
                ((Byte32*)dest)[0] = ((Byte32*)src)[0];
                dest += 32;
                src += 32;
                byteCount -= 32;
            }

            if (byteCount >= 16)
            {
                ((Byte16*)dest)[0] = ((Byte16*)src)[0];
                dest += 16;
                src += 16;
                byteCount -= 16;
            }

            if (byteCount >= 8)
            {
                ((Byte8*)dest)[0] = ((Byte8*)src)[0];
                dest += 8;
                src += 8;
                byteCount -= 8;
            }

            if (byteCount >= 4)
            {
                ((Byte4*)dest)[0] = ((Byte4*)src)[0];
                dest += 4;
                src += 4;
                byteCount -= 4;
            }

            if (byteCount >= 2)
            {
                ((Byte2*)dest)[0] = ((Byte2*)src)[0];
                dest += 2;
                src += 2;
                byteCount -= 2;
            }

            if (byteCount >= 1)
            {
                ((Byte1*)dest)[0] = ((Byte1*)src)[0];
                // dest += 1;
                // src += 1;
                // byteCount -= 1;
            }
        }

        #endregion
    }
}