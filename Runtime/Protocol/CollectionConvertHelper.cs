using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace Pisces.Protocol
{
    /// <summary>
    /// 集合转换辅助类，提供通用的 no-GC 转换方法。
    /// 优化点：缓存 Count、预分配 Capacity、核心逻辑高度抽象以减少重复。
    /// </summary>
    public static class CollectionConvertHelper
    {
        #region List 转换 (Public API)

        /// <summary>
        /// 将 List 序列化为 RepeatedField (ByteString 形式)
        /// </summary>
        /// <typeparam name="T">消息类型，必须实现 IMessage&lt;T&gt;</typeparam>
        /// <param name="source">源列表</param>
        /// <param name="result">目标 RepeatedField，将存储序列化后的 ByteString</param>
        public static void SerializeToList<T>(List<T> source, RepeatedField<ByteString> result) where T : IMessage<T>
        {
            FillRepeatedField(source, result, msg => msg.ToByteString());
        }

        /// <summary>
        /// 将 RepeatedField 转换为 List (无转换)
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">源 RepeatedField</param>
        /// <param name="result">目标列表</param>
        public static void ToList<T>(RepeatedField<T> source, List<T> result)
        {
            FillList(source, result, item => item);
        }

        /// <summary>
        /// 将 RepeatedField 转换为 List (带转换器)
        /// </summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">目标元素类型</typeparam>
        /// <param name="source">源 RepeatedField</param>
        /// <param name="result">目标列表</param>
        /// <param name="converter">转换函数</param>
        public static void ToList<TSource, TResult>(RepeatedField<TSource> source, List<TResult> result, Func<TSource, TResult> converter)
        {
            FillList(source, result, converter);
        }

        /// <summary>
        /// 将 RepeatedField 转换为 List (使用默认反序列化器)
        /// </summary>
        /// <typeparam name="T">消息类型，必须实现 IMessage 并有无参构造函数</typeparam>
        /// <param name="source">包含 ByteString 的源 RepeatedField</param>
        /// <param name="result">目标列表</param>
        public static void DeserializeToList<T>(RepeatedField<ByteString> source, List<T> result) where T : IMessage, new()
        {
            FillList(source, result, ProtoSerializer.Deserialize<T>, true);
        }

        /// <summary>
        /// 将 RepeatedField 转换为 List (使用指定解析器)
        /// </summary>
        /// <typeparam name="T">消息类型，必须实现 IMessage&lt;T&gt;</typeparam>
        /// <param name="source">包含 ByteString 的源 RepeatedField</param>
        /// <param name="result">目标列表</param>
        /// <param name="parser">消息解析器</param>
        public static void DeserializeToList<T>(RepeatedField<ByteString> source, List<T> result, MessageParser<T> parser) where T : IMessage<T>
        {
            FillList(source, result, bs => ProtoSerializer.Deserialize(bs, parser), true);
        }

        #endregion

        #region Dictionary 转换 (Public API)

        /// <summary>
        /// 将 Entry 列表反序列化为 Dictionary
        /// </summary>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型，必须实现 IMessage 并有无参构造函数</typeparam>
        /// <typeparam name="TEntry">Entry 类型</typeparam>
        /// <param name="entries">Entry 列表</param>
        /// <param name="result">目标字典</param>
        /// <param name="keySelector">键选择器函数</param>
        /// <param name="valueSelector">值选择器函数，返回 ByteString</param>
        public static void DeserializeToDictionary<TKey, TValue, TEntry>(
            RepeatedField<TEntry> entries,
            Dictionary<TKey, TValue> result,
            Func<TEntry, TKey> keySelector,
            Func<TEntry, ByteString> valueSelector) where TValue : IMessage, new()
        {
            FillDictionary(entries, result, keySelector, entry => ProtoSerializer.Deserialize<TValue>(valueSelector(entry)));
        }

        /// <summary>
        /// 将 Entry 列表反序列化为 Dictionary (使用 Parser)
        /// </summary>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型，必须实现 IMessage&lt;TValue&gt;</typeparam>
        /// <typeparam name="TEntry">Entry 类型</typeparam>
        /// <param name="entries">Entry 列表</param>
        /// <param name="result">目标字典</param>
        /// <param name="keySelector">键选择器函数</param>
        /// <param name="valueSelector">值选择器函数，返回 ByteString</param>
        /// <param name="parser">消息解析器</param>
        public static void DeserializeToDictionary<TKey, TValue, TEntry>(
            RepeatedField<TEntry> entries,
            Dictionary<TKey, TValue> result,
            Func<TEntry, TKey> keySelector,
            Func<TEntry, ByteString> valueSelector,
            MessageParser<TValue> parser) where TValue : IMessage<TValue>
        {
            FillDictionary(entries, result, keySelector, entry => ProtoSerializer.Deserialize(valueSelector(entry), parser));
        }

        #endregion

        #region 内部核心引擎 (Internal Engines)

        /// <summary>
        /// 核心填充引擎：将任何 IList 转换为 List
        /// </summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">目标元素类型</typeparam>
        /// <param name="source">源 IList</param>
        /// <param name="result">目标 List</param>
        /// <param name="converter">转换函数</param>
        /// <param name="skipPbErrors">是否跳过协议缓冲区错误</param>
        private static void FillList<TSource, TResult>(IList<TSource> source, List<TResult> result, Func<TSource, TResult> converter, bool skipPbErrors = false)
        {
            if (result == null) return;
            result.Clear();
            
            if (source == null) return;
            var count = source.Count; // 缓存 Count，避免循环内重复访问属性
            if (count == 0) return;

            // 预分配容量
            if (result.Capacity < count)
            {
                result.Capacity = count;
            }

            for (var i = 0; i < count; i++)
            {
                if (skipPbErrors)
                {
                    try
                    {
                        TResult item = converter(source[i]);
                        if (item != null) result.Add(item);
                    }
                    catch (InvalidProtocolBufferException) { /* 忽略损坏的数据 */ }
                }
                else
                {
                    var item = converter(source[i]);
                    if (item != null) result.Add(item);
                }
            }
        }

        /// <summary>
        /// 核心填充引擎：针对 RepeatedField 目标的填充
        /// </summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">目标元素类型</typeparam>
        /// <param name="source">源 IList</param>
        /// <param name="result">目标 RepeatedField</param>
        /// <param name="converter">转换函数</param>
        private static void FillRepeatedField<TSource, TResult>(IList<TSource> source, RepeatedField<TResult> result, Func<TSource, TResult> converter)
        {
            if (result == null) return;
            result.Clear();
            
            if (source == null) return;
            var count = source.Count; // 缓存 Count
            if (count == 0) return;

            result.Capacity = count; // RepeatedField 也有 Capacity

            for (var i = 0; i < count; i++)
            {
                TResult item = converter(source[i]);
                if (item != null) result.Add(item);
            }
        }

        /// <summary>
        /// 核心填充引擎：将 RepeatedField 转换为 Dictionary
        /// </summary>
        /// <typeparam name="TEntry">Entry 类型</typeparam>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型</typeparam>
        /// <param name="entries">Entry 列表</param>
        /// <param name="result">目标字典</param>
        /// <param name="keySelector">键选择器函数</param>
        /// <param name="valueConverter">值转换函数</param>
        private static void FillDictionary<TEntry, TKey, TValue>(RepeatedField<TEntry> entries, Dictionary<TKey, TValue> result, Func<TEntry, TKey> keySelector, Func<TEntry, TValue> valueConverter)
        {
            if (result == null) return;
            result.Clear();
            
            if (entries == null) return;
            var count = entries.Count; // 缓存 Count
            if (count == 0) return;

            for (var i = 0; i < count; i++)
            {
                try
                {
                    var entry = entries[i];
                    var key = keySelector(entry);
                    var val = valueConverter(entry);
                    
                    if (val != null)
                    {
                        result[key] = val;
                    }
                }
                catch (InvalidProtocolBufferException) { /* 忽略损坏的数据 */ }
                catch (ArgumentException) { /* 忽略重复的 Key */ }
            }
        }

        #endregion

        #region 扩展方法

        /// <summary>
        /// 扩展方法：批量添加元素到 RepeatedField
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="field">目标 RepeatedField</param>
        /// <param name="items">要添加的元素集合</param>
        public static void AddRange<T>(this RepeatedField<T> field, IEnumerable<T> items)
        {
            if (field == null || items == null) return;
            
            // 如果 items 是列表，尝试优化容量
            if (items is ICollection<T> collection)
            {
                field.Capacity = field.Count + collection.Count;
            }

            foreach (var item in items)
            {
                field.Add(item);
            }
        }

        #endregion
    }
}
