using System.Collections.Generic;
using System.Linq;

namespace Pisces.Protocol
{
    #region IntValue Extensions

    /// <summary>
    /// IntValue 扩展，提供 int 与 IntValue 之间的隐式转换
    /// </summary>
    public partial class IntValue
    {
        /// <summary>
        /// 从 int 隐式转换为 IntValue
        /// </summary>
        /// <example>
        /// IntValue wrapper = 123;
        /// var request = new SomeRequest { Score = 100 };
        /// </example>
        public static implicit operator IntValue(int value)
        {
            return new IntValue { Value = value };
        }

        /// <summary>
        /// 从 IntValue 隐式转换为 int
        /// </summary>
        /// <example>
        /// int value = response.GetValue&lt;IntValue&gt;();
        /// </example>
        public static implicit operator int(IntValue wrapper)
        {
            return wrapper?.Value ?? 0;
        }
    }

    #endregion

    #region LongValue Extensions

    /// <summary>
    /// LongValue 扩展，提供 long 与 LongValue 之间的隐式转换
    /// </summary>
    public partial class LongValue
    {
        /// <summary>
        /// 从 long 隐式转换为 LongValue
        /// </summary>
        public static implicit operator LongValue(long value)
        {
            return new LongValue { Value = value };
        }

        /// <summary>
        /// 从 LongValue 隐式转换为 long
        /// </summary>
        public static implicit operator long(LongValue wrapper)
        {
            return wrapper?.Value ?? 0L;
        }
    }

    #endregion

    #region StringValue Extensions

    /// <summary>
    /// StringValue 扩展，提供 string 与 StringValue 之间的隐式转换
    /// </summary>
    public partial class StringValue
    {
        /// <summary>
        /// 从 string 隐式转换为 StringValue
        /// </summary>
        public static implicit operator StringValue(string value)
        {
            return new StringValue { Value = value ?? string.Empty };
        }

        /// <summary>
        /// 从 StringValue 隐式转换为 string
        /// </summary>
        public static implicit operator string(StringValue wrapper)
        {
            return wrapper?.Value ?? string.Empty;
        }
    }

    #endregion

    #region BoolValue Extensions

    /// <summary>
    /// BoolValue 扩展，提供 bool 与 BoolValue 之间的隐式转换
    /// </summary>
    public partial class BoolValue
    {
        /// <summary>
        /// 从 bool 隐式转换为 BoolValue
        /// </summary>
        public static implicit operator BoolValue(bool value)
        {
            return new BoolValue { Value = value };
        }

        /// <summary>
        /// 从 BoolValue 隐式转换为 bool
        /// </summary>
        public static implicit operator bool(BoolValue wrapper)
        {
            return wrapper?.Value ?? false;
        }
    }

    #endregion

    #region List Extensions

    /// <summary>
    /// IntValueList 扩展，提供 List&lt;int&gt; 与 IntValueList 之间的隐式转换
    /// </summary>
    public partial class IntValueList
    {
        /// <summary>
        /// 从 List&lt;int&gt; 隐式转换为 IntValueList
        /// </summary>
        public static implicit operator IntValueList(List<int> values)
        {
            var list = new IntValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 int[] 隐式转换为 IntValueList
        /// </summary>
        public static implicit operator IntValueList(int[] values)
        {
            var list = new IntValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 IntValueList 隐式转换为 List&lt;int&gt;
        /// </summary>
        public static implicit operator List<int>(IntValueList wrapper)
        {
            return wrapper?.Values?.ToList() ?? new List<int>();
        }
    }

    /// <summary>
    /// LongValueList 扩展，提供 List&lt;long&gt; 与 LongValueList 之间的隐式转换
    /// </summary>
    public partial class LongValueList
    {
        /// <summary>
        /// 从 List&lt;long&gt; 隐式转换为 LongValueList
        /// </summary>
        public static implicit operator LongValueList(List<long> values)
        {
            var list = new LongValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 long[] 隐式转换为 LongValueList
        /// </summary>
        public static implicit operator LongValueList(long[] values)
        {
            var list = new LongValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 LongValueList 隐式转换为 List&lt;long&gt;
        /// </summary>
        public static implicit operator List<long>(LongValueList wrapper)
        {
            return wrapper?.Values?.ToList() ?? new List<long>();
        }
    }

    /// <summary>
    /// StringValueList 扩展，提供 List&lt;string&gt; 与 StringValueList 之间的隐式转换
    /// </summary>
    public partial class StringValueList
    {
        /// <summary>
        /// 从 List&lt;string&gt; 隐式转换为 StringValueList
        /// </summary>
        public static implicit operator StringValueList(List<string> values)
        {
            var list = new StringValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 string[] 隐式转换为 StringValueList
        /// </summary>
        public static implicit operator StringValueList(string[] values)
        {
            var list = new StringValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 StringValueList 隐式转换为 List&lt;string&gt;
        /// </summary>
        public static implicit operator List<string>(StringValueList wrapper)
        {
            return wrapper?.Values?.ToList() ?? new List<string>();
        }
    }

    /// <summary>
    /// BoolValueList 扩展，提供 List&lt;bool&gt; 与 BoolValueList 之间的隐式转换
    /// </summary>
    public partial class BoolValueList
    {
        /// <summary>
        /// 从 List&lt;bool&gt; 隐式转换为 BoolValueList
        /// </summary>
        public static implicit operator BoolValueList(List<bool> values)
        {
            var list = new BoolValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 bool[] 隐式转换为 BoolValueList
        /// </summary>
        public static implicit operator BoolValueList(bool[] values)
        {
            var list = new BoolValueList();
            if (values != null)
                list.Values.AddRange(values);
            return list;
        }

        /// <summary>
        /// 从 BoolValueList 隐式转换为 List&lt;bool&gt;
        /// </summary>
        public static implicit operator List<bool>(BoolValueList wrapper)
        {
            return wrapper?.Values?.ToList() ?? new List<bool>();
        }
    }

    #endregion

    #region Vector Extensions

    /// <summary>
    /// Vector2 扩展，提供 UnityEngine.Vector2 与 Vector2 之间的隐式转换
    /// </summary>
    public partial class Vector2
    {
        /// <summary>
        /// 从 UnityEngine.Vector2 隐式转换为 Vector2
        /// </summary>
        public static implicit operator Vector2(UnityEngine.Vector2 value)
        {
            return new Vector2 { X = value.x, Y = value.y };
        }

        /// <summary>
        /// 从 Vector2 隐式转换为 UnityEngine.Vector2
        /// </summary>
        public static implicit operator UnityEngine.Vector2(Vector2 wrapper)
        {
            return wrapper != null
                ? new UnityEngine.Vector2(wrapper.X, wrapper.Y)
                : UnityEngine.Vector2.zero;
        }
    }

    /// <summary>
    /// Vector2Int 扩展，提供 UnityEngine.Vector2Int 与 Vector2Int 之间的隐式转换
    /// </summary>
    public partial class Vector2Int
    {
        /// <summary>
        /// 从 UnityEngine.Vector2Int 隐式转换为 Vector2Int
        /// </summary>
        public static implicit operator Vector2Int(UnityEngine.Vector2Int value)
        {
            return new Vector2Int { X = value.x, Y = value.y };
        }

        /// <summary>
        /// 从 Vector2Int 隐式转换为 UnityEngine.Vector2Int
        /// </summary>
        public static implicit operator UnityEngine.Vector2Int(Vector2Int wrapper)
        {
            return wrapper != null
                ? new UnityEngine.Vector2Int(wrapper.X, wrapper.Y)
                : UnityEngine.Vector2Int.zero;
        }
    }

    /// <summary>
    /// Vector3 扩展，提供 UnityEngine.Vector3 与 Vector3 之间的隐式转换
    /// </summary>
    public partial class Vector3
    {
        /// <summary>
        /// 从 UnityEngine.Vector3 隐式转换为 Vector3
        /// </summary>
        public static implicit operator Vector3(UnityEngine.Vector3 value)
        {
            return new Vector3 { X = value.x, Y = value.y, Z = value.z };
        }

        /// <summary>
        /// 从 Vector3 隐式转换为 UnityEngine.Vector3
        /// </summary>
        public static implicit operator UnityEngine.Vector3(Vector3 wrapper)
        {
            return wrapper != null
                ? new UnityEngine.Vector3(wrapper.X, wrapper.Y, wrapper.Z)
                : UnityEngine.Vector3.zero;
        }
    }

    /// <summary>
    /// Vector3Int 扩展，提供 UnityEngine.Vector3Int 与 Vector3Int 之间的隐式转换
    /// </summary>
    public partial class Vector3Int
    {
        /// <summary>
        /// 从 UnityEngine.Vector3Int 隐式转换为 Vector3Int
        /// </summary>
        public static implicit operator Vector3Int(UnityEngine.Vector3Int value)
        {
            return new Vector3Int { X = value.x, Y = value.y, Z = value.z };
        }

        /// <summary>
        /// 从 Vector3Int 隐式转换为 UnityEngine.Vector3Int
        /// </summary>
        public static implicit operator UnityEngine.Vector3Int(Vector3Int wrapper)
        {
            return wrapper != null
                ? new UnityEngine.Vector3Int(wrapper.X, wrapper.Y, wrapper.Z)
                : UnityEngine.Vector3Int.zero;
        }
    }

    /// <summary>
    /// Vector2List 扩展，提供 List&lt;UnityEngine.Vector2&gt; 与 Vector2List 之间的隐式转换
    /// </summary>
    public partial class Vector2List
    {
        /// <summary>
        /// 从 List&lt;UnityEngine.Vector2&gt; 隐式转换为 Vector2List
        /// </summary>
        public static implicit operator Vector2List(List<UnityEngine.Vector2> values)
        {
            var list = new Vector2List();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector2 { X = v.x, Y = v.y });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 UnityEngine.Vector2[] 隐式转换为 Vector2List
        /// </summary>
        public static implicit operator Vector2List(UnityEngine.Vector2[] values)
        {
            var list = new Vector2List();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector2 { X = v.x, Y = v.y });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 Vector2List 隐式转换为 List&lt;UnityEngine.Vector2&gt;
        /// </summary>
        public static implicit operator List<UnityEngine.Vector2>(Vector2List wrapper)
        {
            if (wrapper?.Values == null) return new List<UnityEngine.Vector2>();

            var result = new List<UnityEngine.Vector2>(wrapper.Values.Count);
            foreach (var v in wrapper.Values)
            {
                result.Add(new UnityEngine.Vector2(v.X, v.Y));
            }
            return result;
        }
    }

    /// <summary>
    /// Vector2IntList 扩展，提供 List&lt;UnityEngine.Vector2Int&gt; 与 Vector2IntList 之间的隐式转换
    /// </summary>
    public partial class Vector2IntList
    {
        /// <summary>
        /// 从 List&lt;UnityEngine.Vector2Int&gt; 隐式转换为 Vector2IntList
        /// </summary>
        public static implicit operator Vector2IntList(List<UnityEngine.Vector2Int> values)
        {
            var list = new Vector2IntList();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector2Int { X = v.x, Y = v.y });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 UnityEngine.Vector2Int[] 隐式转换为 Vector2IntList
        /// </summary>
        public static implicit operator Vector2IntList(UnityEngine.Vector2Int[] values)
        {
            var list = new Vector2IntList();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector2Int { X = v.x, Y = v.y });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 Vector2IntList 隐式转换为 List&lt;UnityEngine.Vector2Int&gt;
        /// </summary>
        public static implicit operator List<UnityEngine.Vector2Int>(Vector2IntList wrapper)
        {
            if (wrapper?.Values == null) return new List<UnityEngine.Vector2Int>();

            var result = new List<UnityEngine.Vector2Int>(wrapper.Values.Count);
            foreach (var v in wrapper.Values)
            {
                result.Add(new UnityEngine.Vector2Int(v.X, v.Y));
            }
            return result;
        }
    }

    /// <summary>
    /// Vector3List 扩展，提供 List&lt;UnityEngine.Vector3&gt; 与 Vector3List 之间的隐式转换
    /// </summary>
    public partial class Vector3List
    {
        /// <summary>
        /// 从 List&lt;UnityEngine.Vector3&gt; 隐式转换为 Vector3List
        /// </summary>
        public static implicit operator Vector3List(List<UnityEngine.Vector3> values)
        {
            var list = new Vector3List();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector3 { X = v.x, Y = v.y, Z = v.z });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 UnityEngine.Vector3[] 隐式转换为 Vector3List
        /// </summary>
        public static implicit operator Vector3List(UnityEngine.Vector3[] values)
        {
            var list = new Vector3List();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector3 { X = v.x, Y = v.y, Z = v.z });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 Vector3List 隐式转换为 List&lt;UnityEngine.Vector3&gt;
        /// </summary>
        public static implicit operator List<UnityEngine.Vector3>(Vector3List wrapper)
        {
            if (wrapper?.Values == null) return new List<UnityEngine.Vector3>();

            var result = new List<UnityEngine.Vector3>(wrapper.Values.Count);
            foreach (var v in wrapper.Values)
            {
                result.Add(new UnityEngine.Vector3(v.X, v.Y, v.Z));
            }
            return result;
        }
    }

    /// <summary>
    /// Vector3IntList 扩展，提供 List&lt;UnityEngine.Vector3Int&gt; 与 Vector3IntList 之间的隐式转换
    /// </summary>
    public partial class Vector3IntList
    {
        /// <summary>
        /// 从 List&lt;UnityEngine.Vector3Int&gt; 隐式转换为 Vector3IntList
        /// </summary>
        public static implicit operator Vector3IntList(List<UnityEngine.Vector3Int> values)
        {
            var list = new Vector3IntList();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector3Int { X = v.x, Y = v.y, Z = v.z });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 UnityEngine.Vector3Int[] 隐式转换为 Vector3IntList
        /// </summary>
        public static implicit operator Vector3IntList(UnityEngine.Vector3Int[] values)
        {
            var list = new Vector3IntList();
            if (values != null)
            {
                foreach (var v in values)
                {
                    list.Values.Add(new Vector3Int { X = v.x, Y = v.y, Z = v.z });
                }
            }
            return list;
        }

        /// <summary>
        /// 从 Vector3IntList 隐式转换为 List&lt;UnityEngine.Vector3Int&gt;
        /// </summary>
        public static implicit operator List<UnityEngine.Vector3Int>(Vector3IntList wrapper)
        {
            if (wrapper?.Values == null) return new List<UnityEngine.Vector3Int>();

            var result = new List<UnityEngine.Vector3Int>(wrapper.Values.Count);
            foreach (var v in wrapper.Values)
            {
                result.Add(new UnityEngine.Vector3Int(v.X, v.Y, v.Z));
            }
            return result;
        }
    }

    #endregion
}
