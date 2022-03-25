using System;
using System.Collections.Generic;

namespace Kuroha.Framework.ZString
{
    public class KString
    {
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

        private static Queue<KString>[] g_cache; //idx特定字符串长度,深拷贝核心缓存
        private static Dictionary<int, Queue<KString>> g_secCache; //key特定字符串长度value字符串栈，深拷贝次级缓存
        private static Stack<KString> g_shallowCache; //浅拷贝缓存

        private static Stack<KStringBlock> g_blocks; // KStringBlock 缓存栈
        private static Stack<KStringBlock> g_open_blocks; // KString 已经打开的缓存栈      
        private static Dictionary<int, string> g_intern_table; // 字符串 intern 表
        private static KStringBlock g_current_block; // KString 所在的 block 块
        private static readonly List<int> g_finds; // 字符串 replace 功能记录子串位置
        private static readonly KString[] g_format_args; // 存储格式化字符串值

        private const int INITIAL_BLOCK_CAPACITY = 32; // gBlock 块数量  
        private const int INITIAL_CACHE_CAPACITY = 128; // cache 缓存字典容量  128*4Byte 500 多 Byte
        private const int INITIAL_STACK_CAPACITY = 48; // cache 字典每个stack默认 nString 容量
        private const int INITIAL_INTERN_CAPACITY = 256; // Intern容量
        private const int INITIAL_OPEN_CAPACITY = 5; // 默认打开层数为5
        private const int INITIAL_SHALLOW_CAPACITY = 100; // 默认50个浅拷贝用
        private const char NEW_ALLOC_CHAR = 'X'; // 填充char

        private readonly bool isShallow; //是否浅拷贝

        [NonSerialized]
        private string currentString;

        [NonSerialized]
        private bool disposedFlag;

        // 带默认长度的构造
        private KString(int length)
        {
            currentString = new string(NEW_ALLOC_CHAR, length);
        }

        // 浅拷贝专用构造
        private KString(string value)
        {
            isShallow = true;
            currentString = value;
        }

        static KString()
        {
            Initialize(INITIAL_CACHE_CAPACITY, INITIAL_STACK_CAPACITY, INITIAL_BLOCK_CAPACITY, INITIAL_INTERN_CAPACITY, INITIAL_OPEN_CAPACITY, INITIAL_SHALLOW_CAPACITY);

            g_finds = new List<int>(10);
            g_format_args = new KString[10];
        }

        //析构
        private void dispose()
        {
            if (disposedFlag)
                throw new ObjectDisposedException(this);

            if (isShallow) //深浅拷贝走不同缓存
            {
                g_shallowCache.Push(this);
            } else
            {
                Queue<KString> stack;
                if (g_cache.Length > Length)
                {
                    stack = g_cache[Length]; //取出valuelength长度的栈，将自身push进去
                } else
                {
                    stack = g_secCache[Length];
                }

                stack.Enqueue(this);
            }

            //memcpy(_value, NEW_ALLOC_CHAR);//内存拷贝至value
            disposedFlag = true;
        }

        //由string获取相同内容ZString，深拷贝
        private static KString Get(string value)
        {
            if (value == null)
                return null;
#if DBG
            if (log != null)
                log("Getting: " + value);
#endif
            var result = Get(value.Length);
            MemoryCopy(dst: result, src: value); //内存拷贝
            return result;
        }

        //由string浅拷贝入ZString
        private static KString getShallow(string value)
        {
            if (g_current_block == null)
            {
                throw new InvalidOperationException("nstring 操作必须在一个nstring_block块中。");
            }

            KString result;
            if (g_shallowCache.Count == 0)
            {
                result = new KString(value);
            } else
            {
                result = g_shallowCache.Pop();
                result.currentString = value;
            }

            result.disposedFlag = false;
            g_current_block.Push(result); //ZString推入块所在栈
            return result;
        }

        //将string加入intern表中
        private static string __intern(string value)
        {
            int hash = value.GetHashCode();
            if (g_intern_table.ContainsKey(hash))
            {
                return g_intern_table[hash];
            } else
            {
                string interned = new string(NEW_ALLOC_CHAR, value.Length);
                MemoryCopy(interned, value);
                g_intern_table.Add(hash, interned);
                return interned;
            }
        }

        
        
        //value是10的次方数
        private static int get_digit_count(long value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) { }

            return cnt;
        }

        //value是10的次方数
        private static uint get_digit_count(uint value)
        {
            uint cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) { }

            return cnt;
        }

        //value是10的次方数
        private static int get_digit_count(int value)
        {
            int cnt;
            for (cnt = 1; (value /= 10) > 0; cnt++) { }

            return cnt;
        }

        //获取char在input中start起往后的下标
        private static int internal_index_of(string input, char value, int start)
        {
            return internal_index_of(input, value, start, input.Length - start);
        }

        //获取string在input中起始0的下标
        private static int internal_index_of(string input, string value)
        {
            return internal_index_of(input, value, 0, input.Length);
        }

        //获取string在input中自0起始下标
        private static int internal_index_of(string input, string value, int start)
        {
            return internal_index_of(input, value, start, input.Length - start);
        }

        //获取格式化字符串
        private static unsafe KString internal_format(string input, int num_args)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            //新字符串长度
            var new_len = input.Length;
            for (var i = -3;;)
            {
                i = internal_index_of(input, '{', i + 3);
                if (i == -1)
                {
                    break;
                }

                new_len -= 3;
                var arg_idx = input[i + 1] - '0';
                var arg = g_format_args[arg_idx];
                new_len += arg.Length;
            }

            var result = Get(new_len);
            var res_value = result.currentString;

            var next_output_idx = 0;
            var next_input_idx = 0;
            var brace_idx = -3;
            for (int i = 0, j = 0;;) // x < num_args
            {
                brace_idx = internal_index_of(input, '{', brace_idx + 3);
                if (brace_idx == -1)
                {
                    break;
                }

                next_input_idx = brace_idx;
                var arg_idx = input[brace_idx + 1] - '0';
                var arg = g_format_args[arg_idx].currentString;
                if (brace_idx + 2 >= input.Length || input[brace_idx + 2] != '}')
                {
                    throw new InvalidOperationException("没有发现大括号} for argument " + arg);
                }

                fixed (char* ptr_input = input)
                {
                    fixed (char* ptr_result = res_value)
                    {
                        for (var k = 0; i < new_len;)
                        {
                            if (j < brace_idx)
                            {
                                ptr_result[i++] = ptr_input[j++];
                                ++next_output_idx;
                            } else
                            {
                                ptr_result[i++] = arg[k++];
                                ++next_output_idx;
                                if (k == arg.Length)
                                {
                                    j += 3;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            next_input_idx += 3;
            for (int i = next_output_idx, j = 0; i < new_len; i++, j++)
            {
                fixed (char* ptr_input = input)
                {
                    fixed (char* ptr_result = res_value)
                    {
                        ptr_result[i] = ptr_input[next_input_idx + j];
                    }
                }
            }

            return result;
        }

        //获取char在字符串中start开始的下标
        private static unsafe int internal_index_of(string input, char value, int start, int count)
        {
            if (start < 0 || start >= input.Length)
                // throw new ArgumentOutOfRangeException("start");
                return -1;

            if (start + count > input.Length)
                return -1;
            // throw new ArgumentOutOfRangeException("count=" + count + " start+count=" + start + count);

            fixed (char* ptr_this = input)
            {
                int end = start + count;
                for (int i = start; i < end; i++)
                    if (ptr_this[i] == value)
                        return i;
                return -1;
            }
        }

        //获取value在input中自start起始下标
        private static unsafe int internal_index_of(string input, string value, int start, int count)
        {
            int input_len = input.Length;

            if (start < 0 || start >= input_len)
                throw new ArgumentOutOfRangeException("start");

            if (count < 0 || start + count > input_len)
                throw new ArgumentOutOfRangeException("count=" + count + " start+count=" + (start + count));

            if (count == 0)
                return -1;

            fixed (char* ptr_input = input)
            {
                fixed (char* ptr_value = value)
                {
                    int found = 0;
                    int end = start + count;
                    for (int i = start; i < end; i++)
                    {
                        for (int j = 0; j < value.Length && i + j < input_len; j++)
                        {
                            if (ptr_input[i + j] == ptr_value[j])
                            {
                                found++;
                                if (found == value.Length)
                                    return i;
                                continue;
                            }

                            if (found > 0)
                                break;
                        }
                    }

                    return -1;
                }
            }
        }

        //移除string中自start起始count长度子串
        private static KString internal_remove(string input, int start, int count)
        {
            if (start < 0 || start >= input.Length)
                throw new ArgumentOutOfRangeException("start=" + start + " Length=" + input.Length);

            if (count < 0 || start + count > input.Length)
                throw new ArgumentOutOfRangeException("count=" + count + " start+count=" + (start + count) + " Length=" + input.Length);

            if (count == 0)
                return input;

            KString result = Get(input.Length - count);
            internal_remove(result, input, start, count);
            return result;
        }

        //将src中自start起count长度子串复制入dst
        private static unsafe void internal_remove(string dst, string src, int start, int count)
        {
            fixed (char* src_ptr = src)
            {
                fixed (char* dst_ptr = dst)
                {
                    for (int i = 0, j = 0; i < dst.Length; i++)
                    {
                        if (i >= start && i < start + count) // within removal range
                        {
                            continue;
                        }

                        dst_ptr[j++] = src_ptr[i];
                    }
                }
            }
        }

        //字符串replace，原字符串，需替换子串，替换的新子串
        private static unsafe KString internal_replace(string value, string old_value, string new_value)
        {
            // "Hello, World. There World" | World->Jon =
            // "000000000000000000000" (len = orig - 2 * (world-jon) = orig - 4
            // "Hello, 00000000000000"
            // "Hello, Jon00000000000"
            // "Hello, Jon. There 000"
            // "Hello, Jon. There Jon"

            // "Hello, World. There World" | World->Alexander =
            // "000000000000000000000000000000000" (len = orig + 2 * (alexander-world) = orig + 8
            // "Hello, 00000000000000000000000000"
            // "Hello, Alexander00000000000000000"
            // "Hello, Alexander. There 000000000"
            // "Hello, Alexander. There Alexander"

            if (old_value == null)
                throw new ArgumentNullException("old_value");

            if (new_value == null)
                throw new ArgumentNullException("new_value");

            int idx = internal_index_of(value, old_value);
            if (idx == -1)
                return value;

            g_finds.Clear();
            g_finds.Add(idx);

            // 记录所有需要替换的idx点
            while (idx + old_value.Length < value.Length)
            {
                idx = internal_index_of(value, old_value, idx + old_value.Length);
                if (idx == -1)
                    break;
                g_finds.Add(idx);
            }

            // calc the right new total length
            int new_len;
            int dif = old_value.Length - new_value.Length;
            if (dif > 0)
                new_len = value.Length - (g_finds.Count * dif);
            else
                new_len = value.Length + (g_finds.Count * -dif);

            KString result = Get(new_len);
            fixed (char* ptr_this = value)
            {
                fixed (char* ptr_result = result.currentString)
                {
                    for (int i = 0, x = 0, j = 0; i < new_len;)
                    {
                        if (x == g_finds.Count || g_finds[x] != j)
                        {
                            ptr_result[i++] = ptr_this[j++];
                        } else
                        {
                            for (int n = 0; n < new_value.Length; n++)
                                ptr_result[i + n] = new_value[n];

                            x++;
                            i += new_value.Length;
                            j += old_value.Length;
                        }
                    }
                }
            }

            return result;
        }

        //向字符串value中自start位置插入count长度的to_insertChar
        private static unsafe KString internal_insert(string value, char to_insert, int start, int count)
        {
            // "HelloWorld" (to_insert=x, start=5, count=3) -> "HelloxxxWorld"

            if (start < 0 || start >= value.Length)
                throw new ArgumentOutOfRangeException("start=" + start + " Length=" + value.Length);

            if (count < 0)
                throw new ArgumentOutOfRangeException("count=" + count);

            if (count == 0)
                return Get(value);

            int new_len = value.Length + count;
            KString result = Get(new_len);
            fixed (char* ptr_value = value)
            {
                fixed (char* ptr_result = result.currentString)
                {
                    for (int i = 0, j = 0; i < new_len; i++)
                    {
                        if (i >= start && i < start + count)
                            ptr_result[i] = to_insert;
                        else
                            ptr_result[i] = ptr_value[j++];
                    }
                }
            }

            return result;
        }

        //向input字符串中插入to_insert串，位置为start
        private static KString internal_insert(string input, string to_insert, int start)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (to_insert == null)
            {
                throw new ArgumentNullException(nameof(to_insert));
            }

            if (start < 0 || start >= input.Length)
            {
                throw new ArgumentOutOfRangeException("start=" + start + " Length=" + input.Length);
            }

            if (to_insert.Length == 0)
            {
                return Get(input);
            }

            var new_len = input.Length + to_insert.Length;
            var result = Get(new_len);
            internal_insert(result, input, to_insert, start);
            return result;
        }


        //将to_insert串插入src的start位置，内容写入dst
        private static unsafe void internal_insert(string dst, string src, string to_insert, int start)
        {
            fixed (char* ptr_src = src)
            {
                fixed (char* ptr_dst = dst)
                {
                    fixed (char* ptr_to_insert = to_insert)
                    {
                        for (int i = 0, j = 0, k = 0; i < dst.Length; i++)
                        {
                            if (i >= start && i < start + to_insert.Length)
                                ptr_dst[i] = ptr_to_insert[k++];
                            else
                                ptr_dst[i] = ptr_src[j++];
                        }
                    }
                }
            }
        }

        //将长度为count的数字插入dst中，起始位置为start，dst的长度需大于start+count
        private static unsafe void longcpy(char* dst, long value, int start, int count)
        {
            int end = start + count;
            for (int i = end - 1; i >= start; i--, value /= 10)
                *(dst + i) = (char)(value % 10 + 48);
        }

        //将长度为count的数字插入dst中，起始位置为start，dst的长度需大于start+count
        private static unsafe void intcpy(char* dst, int value, int start, int count)
        {
            int end = start + count;
            for (int i = end - 1; i >= start; i--, value /= 10)
                *(dst + i) = (char)(value % 10 + 48);
        }

        private static unsafe void _memcpy4(byte* dest, byte* src, int size)
        {
            /*while (size >= 32) {
                // using long is better than int and slower than double
                // FIXME: enable this only on correct alignment or on platforms
                // that can tolerate unaligned reads/writes of doubles
                ((double*)dest) [0] = ((double*)src) [0];
                ((double*)dest) [1] = ((double*)src) [1];
                ((double*)dest) [2] = ((double*)src) [2];
                ((double*)dest) [3] = ((double*)src) [3];
                dest += 32;
                src += 32;
                size -= 32;
            }*/
            while (size >= 16)
            {
                ((int*)dest)[0] = ((int*)src)[0];
                ((int*)dest)[1] = ((int*)src)[1];
                ((int*)dest)[2] = ((int*)src)[2];
                ((int*)dest)[3] = ((int*)src)[3];
                dest += 16;
                src += 16;
                size -= 16;
            }

            while (size >= 4)
            {
                ((int*)dest)[0] = ((int*)src)[0];
                dest += 4;
                src += 4;
                size -= 4;
            }

            while (size > 0)
            {
                dest[0] = src[0];
                dest += 1;
                src += 1;
                --size;
            }
        }

        private static unsafe void _memcpy2(byte* dest, byte* src, int size)
        {
            while (size >= 8)
            {
                ((short*)dest)[0] = ((short*)src)[0];
                ((short*)dest)[1] = ((short*)src)[1];
                ((short*)dest)[2] = ((short*)src)[2];
                ((short*)dest)[3] = ((short*)src)[3];
                dest += 8;
                src += 8;
                size -= 8;
            }

            while (size >= 2)
            {
                ((short*)dest)[0] = ((short*)src)[0];
                dest += 2;
                src += 2;
                size -= 2;
            }

            if (size > 0)
            {
                dest[0] = src[0];
            }
        }

        //从src，0位置起始拷贝count长度字符串src到dst中
        //private static unsafe void memcpy(char* dest, char* src, int count)
        //{
        //    // Same rules as for memcpy, but with the premise that 
        //    // chars can only be aligned to even addresses if their
        //    // enclosing types are correctly aligned

        //    superMemcpy(dest, src, count);
        //    //if ((((int)(byte*)dest | (int)(byte*)src) & 3) != 0)//转换为byte指针
        //    //{
        //    //    if (((int)(byte*)dest & 2) != 0 && ((int)(byte*)src & 2) != 0 && count > 0)
        //    //    {
        //    //        ((short*)dest)[0] = ((short*)src)[0];
        //    //        dest++;
        //    //        src++;
        //    //        count--;
        //    //    }
        //    //    if ((((int)(byte*)dest | (int)(byte*)src) & 2) != 0)
        //    //    {
        //    //        _memcpy2((byte*)dest, (byte*)src, count * 2);//转换为short*指针一次两个字节拷贝
        //    //        return;
        //    //    }
        //    //}
        //    //_memcpy4((byte*)dest, (byte*)src, count * 2);//转换为int*指针一次四个字节拷贝
        //}
        //--------------------------------------手敲memcpy-------------------------------------//

        
        //-----------------------------------------------------------------------------------------//

        //将字符串dst用字符src填充
        private static unsafe void MemoryCopy(string dst, char src)
        {
            fixed (char* ptr_dst = dst)
            {
                int len = dst.Length;
                for (int i = 0; i < len; i++)
                    ptr_dst[i] = src;
            }
        }

        //将字符拷贝到dst指定index位置
        private static unsafe void MemoryCopy(string dst, char src, int index)
        {
            fixed (char* ptr = dst)
                ptr[index] = src;
        }

        //将相同长度的src内容拷入dst
        private static unsafe void MemoryCopy(string dst, string src)
        {
            if (dst.Length != src.Length)
                throw new InvalidOperationException("两个字符串参数长度不一致。");
            fixed (char* dst_ptr = dst)
            {
                fixed (char* src_ptr = src)
                {
                    MemoryCopy(dst_ptr, src_ptr, dst.Length);
                }
            }
        }

        

        private static unsafe void MemoryCopy(string dst, string src, int length, int src_offset)
        {
            fixed (char* ptr_dst = dst)
            {
                fixed (char* ptr_src = src)
                {
                    MemoryCopy(ptr_dst + src_offset, ptr_src, length);
                }
            }
        }

        public class KStringBlock : IDisposable
        {
            readonly Stack<KString> stack;

            internal KStringBlock(int capacity)
            {
                stack = new Stack<KString>(capacity);
            }

            internal void Push(KString str)
            {
                stack.Push(str);
            }

            internal IDisposable begin() //构造函数
            {
#if DBG
                if (log != null)
                    log("Began block");
#endif
                return this;
            }

            void IDisposable.Dispose() //析构函数
            {
#if DBG
                if (log != null)
                    log("Disposing block");
#endif
                while (stack.Count > 0)
                {
                    var str = stack.Pop();
                    str.dispose(); //循环调用栈中ZString的Dispose方法
                }

                KString.g_blocks.Push(this); //将自身push入缓存栈

                //赋值currentBlock
                g_open_blocks.Pop();
                if (g_open_blocks.Count > 0)
                {
                    KString.g_current_block = g_open_blocks.Peek();
                } else
                {
                    KString.g_current_block = null;
                }
            }
        }

        // Public API

        #region

        public static Action<string> Log = null;

        public static uint DecimalAccuracy = 3; // 小数点后精度位数

        //获取字符串长度
        public int Length
        {
            get { return currentString.Length; }
        }

        //类构造：cache_capacity缓存栈字典容量，stack_capacity缓存字符串栈容量，block_capacity缓存栈容量，intern_capacity缓存,open_capacity默认打开层数
        public static void Initialize(int cache_capacity, int stack_capacity, int block_capacity, int intern_capacity, int open_capacity, int shallowCache_capacity)
        {
            g_cache = new Queue<KString>[cache_capacity];
            g_secCache = new Dictionary<int, Queue<KString>>(cache_capacity);
            g_blocks = new Stack<KStringBlock>(block_capacity);
            g_intern_table = new Dictionary<int, string>(intern_capacity);
            g_open_blocks = new Stack<KStringBlock>(open_capacity);
            g_shallowCache = new Stack<KString>(shallowCache_capacity);
            for (int c = 0; c < cache_capacity; c++)
            {
                var stack = new Queue<KString>(stack_capacity);
                for (int j = 0; j < stack_capacity; j++)
                    stack.Enqueue(new KString(c));
                g_cache[c] = stack;
            }

            for (int i = 0; i < block_capacity; i++)
            {
                var block = new KStringBlock(block_capacity * 2);
                g_blocks.Push(block);
            }

            for (int i = 0; i < shallowCache_capacity; i++)
            {
                g_shallowCache.Push(new KString(null));
            }
        }

        //using语法所用。从KStringBlock栈中取出一个block并将其置为当前g_current_block，在代码块{}中新生成的ZString都将push入块内部stack中。当离开块作用域时，调用块的Dispose函数，将内栈中所有ZString填充初始值并放入ZString缓存栈。同时将自身放入block缓存栈中。（此处有个问题：使用Stack缓存block，当block被dispose放入Stack后g_current_block仍然指向此block，无法记录此block之前的block，这样导致ZString.Block()无法嵌套使用）
        public static IDisposable Block()
        {
            if (g_blocks.Count == 0)
                g_current_block = new KStringBlock(INITIAL_BLOCK_CAPACITY * 2);
            else
                g_current_block = g_blocks.Pop();

            g_open_blocks.Push(g_current_block); //新加代码，将此玩意压入open栈
            return g_current_block.begin();
        }

        //将ZString value放入intern缓存表中以供外部使用
        public string Intern()
        {
            //string interned = new string(NEW_ALLOC_CHAR, _value.Length);
            //memcpy(interned, _value);
            //return interned;
            return __intern(currentString);
        }

        //将string放入ZString intern缓存表中以供外部使用
        public static string Intern(string value)
        {
            return __intern(value);
        }

        public static void Intern(string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                __intern(values[i]);
        }

        //下标取值函数
        public char this[int i]
        {
            get { return currentString[i]; }
            set { MemoryCopy(this, value, i); }
        }

        //获取 hashcode
        public override int GetHashCode()
        {
            return currentString.GetHashCode();
        }

        //字面值比较
        public override bool Equals(object obj)
        {
            if (obj == null)
                return ReferenceEquals(this, null);

            var gstr = obj as KString;
            if (gstr != null)
                return gstr.currentString == this.currentString;

            var str = obj as string;
            if (str != null)
                return str == this.currentString;

            return false;
        }

        //转化为string
        public override string ToString()
        {
            return currentString;
        }

        //bool->ZString转换
        public static implicit operator KString(bool value)
        {
            return Get(value? "True" : "False");
        }

        // long - >ZString转换
        public static unsafe implicit operator KString(long value)
        {
            // e.g. 125
            // first pass: count the number of digits
            // then: get a KString with length = num digits
            // finally: iterate again, get the char of each digit, memcpy char to result
            bool negative = value < 0;
            value = Math.Abs(value);
            int num_digits = get_digit_count(value);
            KString result;
            if (negative)
            {
                result = Get(num_digits + 1);
                fixed (char* ptr = result.currentString)
                {
                    *ptr = '-';
                    longcpy(ptr, value, 1, num_digits);
                }
            } else
            {
                result = Get(num_digits);
                fixed (char* ptr = result.currentString)
                    longcpy(ptr, value, 0, num_digits);
            }

            return result;
        }

        //int->ZString转换
        public static unsafe implicit operator KString(int value)
        {
            // e.g. 125
            // first pass: count the number of digits
            // then: get a KString with length = num digits
            // finally: iterate again, get the char of each digit, memcpy char to result
            bool negative = value < 0;
            value = Math.Abs(value);
            int num_digits = get_digit_count(value);
            KString result;
            if (negative)
            {
                result = Get(num_digits + 1);
                fixed (char* ptr = result.currentString)
                {
                    *ptr = '-';
                    intcpy(ptr, value, 1, num_digits);
                }
            } else
            {
                result = Get(num_digits);
                fixed (char* ptr = result.currentString)
                    intcpy(ptr, value, 0, num_digits);
            }

            return result;
        }

        //float->ZString转换
        public static unsafe implicit operator KString(float value)
        {
            // e.g. 3.148
            bool negative = value < 0;
            if (negative)
                value = -value;
            long mul = (long)Math.Pow(10, DecimalAccuracy);
            long number = (long)(value * mul); // gets the number as a whole, e.g. 3148
            int left_num = (int)(number / mul); // left part of the decimal point, e.g. 3
            int right_num = (int)(number % mul); // right part of the decimal pnt, e.g. 148
            int left_digit_count = get_digit_count(left_num); // e.g. 1
            int right_digit_count = get_digit_count(right_num); // e.g. 3
            //int total = left_digit_count + right_digit_count + 1; // +1 for '.'
            int total = left_digit_count + (int)DecimalAccuracy + 1; // +1 for '.'

            KString result;
            if (negative)
            {
                result = Get(total + 1); // +1 for '-'
                fixed (char* ptr = result.currentString)
                {
                    *ptr = '-';
                    intcpy(ptr, left_num, 1, left_digit_count);
                    *(ptr + left_digit_count + 1) = '.';
                    var offset = (int)DecimalAccuracy - right_digit_count;
                    for (var i = 0; i < offset; i++)
                    {
                        *(ptr + left_digit_count + i + 1) = '0';
                    }

                    intcpy(ptr, right_num, left_digit_count + 2 + offset, right_digit_count);
                }
            } else
            {
                result = Get(total);
                fixed (char* ptr = result.currentString)
                {
                    intcpy(ptr, left_num, 0, left_digit_count);
                    *(ptr + left_digit_count) = '.';
                    var offset = (int)DecimalAccuracy - right_digit_count;
                    for (var i = 0; i < offset; i++)
                    {
                        *(ptr + left_digit_count + i + 1) = '0';
                    }

                    intcpy(ptr, right_num, left_digit_count + 1 + offset, right_digit_count);
                }
            }

            return result;
        }

        //string->ZString转换
        public static implicit operator KString(string value)
        {
            //return get(value);
            return getShallow(value);
        }

        //string->ZString转换
        public static KString shallow(string value)
        {
            return getShallow(value);
        }

        //KString->string转换
        public static implicit operator string(KString value)
        {
            return value.currentString;
        }


        //转换为大写
        public unsafe KString ToUpper()
        {
            var result = Get(Length);
            fixed (char* ptr_this = this.currentString)
            {
                fixed (char* ptr_result = result.currentString)
                {
                    for (int i = 0; i < currentString.Length; i++)
                    {
                        var ch = ptr_this[i];
                        if (char.IsLower(ch))
                            ptr_result[i] = char.ToUpper(ch);
                        else
                            ptr_result[i] = ptr_this[i];
                    }
                }
            }

            return result;
        }

        //转换为小写
        public unsafe KString ToLower()
        {
            var result = Get(Length);
            fixed (char* ptr_this = this.currentString)
            {
                fixed (char* ptr_result = result.currentString)
                {
                    for (int i = 0; i < currentString.Length; i++)
                    {
                        var ch = ptr_this[i];
                        if (char.IsUpper(ch))
                            ptr_result[i] = char.ToLower(ch);
                        else
                            ptr_result[i] = ptr_this[i];
                    }
                }
            }

            return result;
        }

        //移除剪切
        public KString Remove(int start)
        {
            return Remove(start, Length - start);
        }

        //移除剪切
        private KString Remove(int start, int count)
        {
            return internal_remove(this.currentString, start, count);
        }

        //插入start起count长度字符
        public KString Insert(char value, int start, int count)
        {
            return internal_insert(this.currentString, value, start, count);
        }

        //插入start起字符串
        public KString Insert(string value, int start)
        {
            return internal_insert(this.currentString, value, start);
        }

        //子字符替换
        public unsafe KString Replace(char old_value, char new_value)
        {
            KString result = Get(Length);
            fixed (char* ptr_this = this.currentString)
            {
                fixed (char* ptr_result = result.currentString)
                {
                    for (int i = 0; i < Length; i++)
                    {
                        ptr_result[i] = ptr_this[i] == old_value? new_value : ptr_this[i];
                    }
                }
            }

            return result;
        }

        //子字符串替换
        public KString Replace(string old_value, string new_value)
        {
            return internal_replace(this.currentString, old_value, new_value);
        }

        //剪切start位置起后续子串
        public KString Substring(int start)
        {
            return Substring(start, Length - start);
        }

        //剪切start起count长度的子串
        private unsafe KString Substring(int start, int count)
        {
            if (start < 0 || start >= Length)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (count > Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            KString result = Get(count);
            fixed (char* src = this.currentString)
            fixed (char* dst = result.currentString)
                MemoryCopy(dst, src + start, count);

            return result;
        }

        //子串包含判断
        public bool Contains(string value)
        {
            return IndexOf(value) != -1;
        }

        //字符包含判断
        public bool Contains(char value)
        {
            return IndexOf(value) != -1;
        }

        //子串第一次出现位置
        public int LastIndexOf(string value)
        {
            int idx = -1;
            int last_find = -1;
            while (true)
            {
                idx = internal_index_of(this.currentString, value, idx + value.Length);
                last_find = idx;
                if (idx == -1 || idx + value.Length >= this.currentString.Length)
                    break;
            }

            return last_find;
        }

        //字符第一次出现位置
        public int LastIndexOf(char value)
        {
            int idx = -1;
            int last_find = -1;
            while (true)
            {
                idx = internal_index_of(this.currentString, value, idx + 1);
                last_find = idx;
                if (idx == -1 || idx + 1 >= this.currentString.Length)
                    break;
            }

            return last_find;
        }

        //字符第一次出现位置
        private int IndexOf(char value)
        {
            return IndexOf(value, 0, Length);
        }

        //字符自start起第一次出现位置
        public int IndexOf(char value, int start)
        {
            return internal_index_of(this.currentString, value, start);
        }

        //字符自start起count长度内，
        private int IndexOf(char value, int start, int count)
        {
            return internal_index_of(this.currentString, value, start, count);
        }

        // 子串第一次出现位置
        private int IndexOf(string value)
        {
            return IndexOf(value, 0, Length);
        }

        // 子串自 start 位置起，第一次出现位置
        public int IndexOf(string value, int start)
        {
            return IndexOf(value, start, Length - start);
        }

        //子串自start位置起，count长度内第一次出现位置
        private int IndexOf(string value, int start, int count)
        {
            return internal_index_of(this.currentString, value, start, count);
        }

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        private static void GetStackInCache(int index, out Queue<KString> outStack)
        {
            var length = g_cache.Length;
            
            // 核心缓存
            if (length > index)
            {
                outStack = g_cache[index];
            }
            // 次级缓存
            else
            {
                if (!g_secCache.TryGetValue(index, out outStack))
                {
                    outStack = new Queue<KString>(INITIAL_STACK_CAPACITY);
                    g_secCache[index] = outStack;
                }
            }
        }
        
        private static KString Get(int length)
        {
            if (g_current_block == null || length <= 0)
            {
                throw new InvalidOperationException("KString 操作必须在一个 KStringBlock 块中");
            }

            GetStackInCache(length, out var stack);
            
            // 从缓存中取 Stack
            var result = stack.Count == 0 ? new KString(length) : stack.Dequeue();
            result.disposedFlag = false;
            
            // ZString 推入块所在栈
            g_current_block.Push(result);
            
            return result;
        }
        
        private static unsafe KString InternalConcat(string left, string right)
        {
            var totalLength = left.Length + right.Length;
            var result = Get(totalLength);

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

        private const int charLengthThisPlatform = sizeof(char);

        // 将 src 指定 length 内容拷入 dst, dst 下标 src_offset 偏移
        private static unsafe void MemoryCopy(char* dst, char* src, int length, int src_offset)
        {
            MemoryCopy(dst + src_offset, src, length);
        }
        
        private static unsafe void MemoryCopy(char* dest, char* src, int count)
        {
            ByteCopy( (byte*)dest, (byte*)src, count * charLengthThisPlatform);
        }
        
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
                throw new ArgumentNullException(nameof(prefix));

            if (this.Length < prefix.Length)
                return false;

            fixed (char* ptr_this = this.currentString)
            {
                fixed (char* ptr_prefix = prefix)
                {
                    for (int i = 0; i < prefix.Length; i++)
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
            return right is { } && left is { } && left.currentString != right.currentString;
        }

        
        
        
        
        
        
        
        // 普通的float->string是隐式转换，小数点后只保留三位有效数字
        // 对于更高精确度需求，隐式转换，可以修改静态变量DecimalAccuracy
        // 显式转换使用此方法即可，函数结束DecimalAccuracy值和之前的一样
        public static KString FloatToZString(float value, uint decimalAccuracy)
        {
            var oldValue = DecimalAccuracy;
            DecimalAccuracy = decimalAccuracy;
            KString target = value;
            DecimalAccuracy = oldValue;
            return target;
        }



        #endregion
    }
}