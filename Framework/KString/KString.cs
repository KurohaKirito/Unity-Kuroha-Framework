using System;
using System.Collections.Generic;

namespace Kuroha.Framework.KString
{
    public class KString
    {
        #region 静态变量

        /// <summary>
        /// 深拷贝核心缓存
        /// index: 特定字符串长度
        /// </summary>
        private static readonly Queue<KString>[] coreCache;
        
        /// <summary>
        /// 深拷贝次级缓存
        /// key: 特定字符串长度
        /// value: 字符串栈
        /// </summary>
        private static readonly Dictionary<int, Queue<KString>> secondCache;
        
        /// <summary>
        /// 浅拷贝缓存
        /// </summary>
        private static readonly Stack<KString> shallowCache;
        
        /// <summary>
        /// KStringBlock 缓存栈
        /// </summary>
        private static readonly Stack<KStringBlock> allBlocks;
        
        /// <summary>
        /// KString 已经打开的缓存栈
        /// </summary>
        private static readonly Stack<KStringBlock> openedBlocks;
        
        /// <summary>
        /// KString 所在的 block 块
        /// </summary>
        private static KStringBlock currentBlock;

        #endregion
        
        /// <summary>
        /// KStringBlock
        /// </summary>
        private class KStringBlock : IDisposable
        {
            /// <summary>
            /// KStringBlock 块
            /// </summary>
            private readonly Stack<KString> stack;

            /// <summary>
            /// KStringBlock 构造函数
            /// </summary>
            internal KStringBlock(int capacity)
            {
                stack = new Stack<KString>(capacity);
            }

            /// <summary>
            /// KString 入栈
            /// </summary>
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

                allBlocks.Push(this); //将自身push入缓存栈

                //赋值currentBlock
                openedBlocks.Pop();
                currentBlock = openedBlocks.Count > 0 ? openedBlocks.Peek() : null;
            }
        }
        
        /// <summary>
        /// using 块
        /// </summary>
        public static IDisposable Block()
        {
            currentBlock = allBlocks.Count == 0 ? new KStringBlock(INITIAL_BLOCK_CAPACITY) : allBlocks.Pop();
            openedBlocks.Push(currentBlock);
            return currentBlock.Begin();
        }
        
        /// <summary>
        /// Block 块数量  
        /// </summary>
        private const int INITIAL_BLOCK_CAPACITY = 64;
        /// <summary>
        /// 缓存字典容量  128 * 4 = 512 Byte
        /// </summary>
        private const int INITIAL_CACHE_CAPACITY = 128;
        /// <summary>
        /// 缓存字典每个 Stack 默认 KString 容量
        /// </summary>
        private const int INITIAL_STACK_CAPACITY = 48;
        /// <summary>
        /// 初始开启的 Stack 数量
        /// </summary>
        private const int INITIAL_OPEN_CAPACITY = 5;
        /// <summary>
        /// 浅拷贝缓存个数
        /// </summary>
        private const int INITIAL_SHALLOW_CAPACITY = 100;
        
        /// <summary>
        /// 创建新字符串时的默认字符
        /// </summary>
        private const char NEW_ALLOC_CHAR = 'X';
        
        /// <summary>
        /// 平台 Char 类型长度
        /// </summary>
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

        /// <summary>
        /// 是否浅拷贝
        /// </summary>
        private readonly bool isShallow;

        /// <summary>
        /// 当前 KString 代表的 string 值
        /// </summary>
        [NonSerialized]
        private string currentString;

        /// <summary>
        /// Dispose 标识
        /// </summary>
        [NonSerialized]
        private bool disposedFlag;

        /// <summary>
        /// 私有构造: 带默认长度的构造
        /// </summary>
        private KString(int length)
        {
            currentString = new string(NEW_ALLOC_CHAR, length);
        }

        /// <summary>
        /// 私有构造: 浅拷贝专用构造
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
            // 建立核心缓存: 128 个 Queue<KString>
            coreCache = new Queue<KString>[INITIAL_CACHE_CAPACITY];
            for (var length = 0; length < INITIAL_CACHE_CAPACITY; ++length)
            {
                // 每个 Queue 由 48 个相同长度的 KString 组成
                var stack = new Queue<KString>(INITIAL_STACK_CAPACITY);
                for (var counter = 0; counter < INITIAL_STACK_CAPACITY; counter++)
                {
                    stack.Enqueue(new KString(length));
                }
                
                // 128 个 Queue, 分别长度为 0 ~ 127
                coreCache[length] = stack;
            }
            
            // 建立次级缓存: 128 个 Queue<KString>
            secondCache = new Dictionary<int, Queue<KString>>(INITIAL_CACHE_CAPACITY);
            
            // 建立 Block 块: 64 个长度均为 64 的 KStringBlock
            allBlocks = new Stack<KStringBlock>(INITIAL_BLOCK_CAPACITY);
            for (var length = 0; length < INITIAL_BLOCK_CAPACITY; ++length)
            {
                var block = new KStringBlock(INITIAL_BLOCK_CAPACITY);
                allBlocks.Push(block);
            }
            
            // 建立开启的 Block 块: 5 个
            openedBlocks = new Stack<KStringBlock>(INITIAL_OPEN_CAPACITY);
            
            // 建立浅拷贝缓存: 100 个空的 KString
            shallowCache = new Stack<KString>(INITIAL_SHALLOW_CAPACITY);
            for (var index = 0; index < INITIAL_SHALLOW_CAPACITY; ++index)
            {
                shallowCache.Push(new KString(null));
            }
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
            if (currentBlock == null)
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
            
            currentBlock.Push(result);
            
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
            if (currentBlock == null || length <= 0)
            {
                throw new InvalidOperationException("KString 操作必须在一个 KStringBlock 块中");
            }
            
            // 从缓存中取 Stack
            GetStackInCache(length, out var stack);
            var result = stack.Count == 0 ? new KString(length) : stack.Dequeue();
            result.disposedFlag = false;
            
            // KString 推入块所在栈
            currentBlock.Push(result);
            
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
        /// 将 source 指定 length 内容拷入下标偏移了 sourceOffset 的 target
        /// </summary>
        private static unsafe void MemoryCopy(char* target, char* source, int length, int sourceOffset)
        {
            MemoryCopy(target + sourceOffset, source, length);
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
