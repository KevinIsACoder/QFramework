//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Spark
{
	/****************************************************************\
	| See http://www.JSON.org/js.html                                |
	|****************************************************************|
	| JSON.stringify(value [, replacer [, space]])                   |
	| JSON.parse(text [, reviver])                                   |
	| JSON.undefined                                                 |
	\****************************************************************/
	sealed public class JSON
	{
		class Undefined
		{
			public override string ToString()
			{
				return "undefined";
			}
		}

		/// JSON.undefined
		static public readonly object undefined = new Undefined();

		static public string Stringify(object value)
		{
			return Stringify(value, null, null, null);
		}
		static public string Stringify(object value, int space)
		{
			return Stringify(value, null, null, space);
		}
		static public string Stringify(object value, string space)
		{
			return Stringify(value, null, null, space);
		}
		static public string Stringify(object value, Func<string, object, object> replacer)
		{
			return Stringify(value, replacer, null, null);
		}
		static public string Stringify(object value, Func<string, object, object> replacer, int space)
		{
			return Stringify(value, replacer, null, space);
		}
		static public string Stringify(object value, Func<string, object, object> replacer, string space)
		{
			return Stringify(value, replacer, null, space);
		}
		static public string Stringify(object value, string[] replacer)
		{
			return Stringify(value, null, replacer, null);
		}
		static public string Stringify(object value, string[] replacer, int space)
		{
			return Stringify(value, null, replacer, space);
		}
		static public string Stringify(object value, string[] replacer, string space)
		{
			return Stringify(value, null, replacer, space);
		}

		static private string Stringify(object value, Func<string, object, object> replacer, string[] filter, int space)
		{
			return Encoder.Encode(value, replacer, filter, new string(' ', Math.Max(Math.Min(space, 10), 0)));
		}
		static private string Stringify(object value, Func<string, object, object> replacer, string[] filter, string space)
		{
			if (string.IsNullOrEmpty(space)) {
				space = "";
			} else if (space.Length > 10) {
				space = space.Substring(0, 10);
			}
			return Encoder.Encode(value, replacer, filter, space);
		}

		/// JSON.Parse

		static public object Parse(string text)
		{
			return Parse(text, null);
		}
		static public object Parse(string text, Func<string, object, object> reviver)
		{
			return Decoder.Decode(text, reviver);
		}

		#region Encoder
		static class Encoder
		{
			static private Dictionary<char, string> characters = new Dictionary<char, string>()
			{
				{ '"', "\\\"" },
				{ '\b', "\\b" },
				{ '\f', "\\f" },
				{ '\n', "\\n" },
				{ '\r', "\\r" },
				{ '\t', "\\t" },
				{ '\\', "\\\\" },
			};

			static public string Encode(object value, Func<string, object, object> replacer, string[] filter, string space)
			{
				value = SerializeValue("", value, replacer, filter, space, "");
				if (value == undefined) {
					return "null";
				}
				return (string)value;
			}

			static private object SerializeValue(string key, object value, Func<string, object, object> replacer, string[] filter, string space, string gap)
			{
				if (replacer != null) {
					value = replacer(key, value);
				}
				if (value == undefined)
					return value;

				if (value is string)
					return SerializeString((string)value);

				if (value is char)
					return '"' + SerializeChar((char)value) + '"';

				if (value is byte || value is uint8
					|| value is sbyte || value is int8
					|| value is short || value is int16
					|| value is ushort || value is uint16
					|| value is int || value is int32
					|| value is uint || value is uint32
					|| value is long || value is int64
					|| value is ulong || value is uint64
					|| value is float || value is fixed32
					|| value is double || value is fixed64
					|| value is decimal || value is number)
					return value.ToString();

				if ((value is Boolean) && ((bool)value == true))
					return "true";
				if ((value is Boolean) && ((bool)value == false))
					return "false";
				if (value == null)
					return "null";

				if (value is IDictionary)
					return SerializeObject((IDictionary)value, replacer, filter, space, gap);

				if (value is IEnumerable)
					return SerializeArray((IEnumerable)value, replacer, filter, space, gap);

				Hashtable table = new Hashtable();
				foreach (FieldInfo field in value.GetType().GetFields()) {
					table[field.Name] = field.GetValue(value);
				}
				foreach (PropertyInfo prop in value.GetType().GetProperties()) {
					table[prop.Name] = prop.GetValue(value, null);
				}
				return SerializeObject(table, replacer, filter, space, gap);
			}

			static private string SerializeObject(IDictionary value, Func<string, object, object> replacer, string[] filter, string space, string gap)
			{
				string indent = gap + space;
				string sep = indent == "" ? ":" : ": ";
				List<string> partial = new List<string>();
				if (filter != null) {
					for (int i = 0, length = filter.Length; i < length; i++) {
						string k = filter[i];
						if (value.Contains(k)) {
							object v = SerializeValue(k, value[k], replacer, filter, space, indent);
							if (v == undefined)
								continue;
							partial.Add(SerializeString(k) + sep + (string)v);
						}
					}
				} else {
					IDictionaryEnumerator e = value.GetEnumerator();
					while (e.MoveNext()) {
						string k = e.Key.ToString();
						object v = SerializeValue(k, e.Value, replacer, filter, space, indent);
						if (v == undefined)
							continue;
						partial.Add(SerializeString(k) + sep + (string)v);
					}
				}
				return partial.Count == 0 ? "{}" : (indent == "" ? "{" + string.Join(",", partial.ToArray()) + "}" : "{\n" + indent + string.Join(",\n" + indent, partial.ToArray()) + "\n" + gap + "}");
			}

			static private string SerializeArray(IEnumerable value, Func<string, object, object> replacer, string[] filter, string space, string gap)
			{
				int i = 0;
				string indent = gap + space;
				List<string> partial = new List<string>();
				foreach (object val in value) {
					object v = SerializeValue(i.ToString(), val, replacer, filter, space, indent);
					partial.Add(v == undefined ? "null" : (string)v);
				}
				return partial.Count == 0 ? "[]" : (indent == "" ? "[" + string.Join(",", partial.ToArray()) + "]" : "[\n" + indent + string.Join(",\n" + indent, partial.ToArray()) + "\n" + gap + "]");
			}

			static private string SerializeString(string value)
			{
				string result = "\"";
				foreach (char ch in value) {
					result += SerializeChar(ch);
				}
				return result + "\"";
			}

			static private string SerializeChar(char value)
			{
				string result;
				if (characters.TryGetValue(value, out result)) {
					return result;
				}
				return value > 127 ? "\\u" + ((int)value).ToString("x4") : value.ToString();
			}
		}
		#endregion

		#region Decoder
		static class Decoder
		{
			static private readonly char NULL = '\0';

			static public object Decode(string json, Func<string, object, object> reviver)
			{
				char[] array = json.ToCharArray();
				int index = 0, length = array.Length;
				object value = ParseValue(array, ref index, length, reviver);
				if (reviver != null) {
					value = reviver("", value);
				}
				return value;
			}

			static private bool IsWhiteSpace(char ch)
			{
				return ch == '\u0020' || ch == '\u0009' || ch == '\u000C'
						|| ch == '\u00A0' || ch == '\u000B' || ch == '\uFEFF'
						|| ch == '\u1680' || ch == '\u180E'
						|| (ch >= '\u2000' && ch <= '\u200A') || ch == '\u202F'
						|| ch == '\u205F' || ch == '\u3000'
						|| ch == '\u000D' || ch == '\u000A'; // \r \n
			}

			static private bool IsHexDigit(char ch)
			{
				return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f')
						|| (ch >= 'A' && ch <= 'F');
			}

			static private bool IsDigit(char ch)
			{
				return ch >= '0' && ch <= '9';
			}

			static private char NextChar(char[] array, ref int index, int length, bool eat = true)
			{
				while (index < length) {
					char ch = array[index++];
					if (!eat || !IsWhiteSpace(ch))
						return ch;
				}
				return NULL;
			}

			static private object ParseValue(char[] array, ref int index, int length, Func<string, object, object> reviver)
			{
				char ch = NextChar(array, ref index, length);
				if (ch == '{')
					return ParseObject(array, ref index, length, reviver);
				if (ch == '[')
					return ParseArray(array, ref index, length, reviver);
				if (ch == '"')
					return ParseString(array, ref index, length);
				if (ch == 'f') {
					if (length - index >= 4) {
						if (array[index] == 'a' && array[index + 1] == 'l' && array[index + 2] == 's' && array[index + 3] == 'e') {
							index += 4;
							return false;
						}
					}
				} else if (ch == 't') {
					if (length - index >= 3) {
						if (array[index] == 'r' && array[index + 1] == 'u' && array[index + 2] == 'e') {
							index += 3;
							return true;
						}
					}
				} else if (ch == 'n') {
					if (length - index >= 3) {
						if (array[index] == 'u' && array[index + 1] == 'l' && array[index + 2] == 'l') {
							index += 3;
							return null;
						}
					}
				} else if (IsDigit(ch) || ch == '-' || ch == '.') {
					return ParseNumber(ch, array, ref index, length);
				}
				ThrowSyntaxError(array, index - 1, index, "Expecting '{', '[', '\"', 'true', 'false', 'null', 'number', got '" + ch + "'");
				return null;
			}

			static private Hashtable ParseObject(char[] array, ref int index, int length, Func<string, object, object> reviver)
			{
				char ch = NULL;
				int saveIndex = index;
				Hashtable table = new Hashtable();
				while ((ch = NextChar(array, ref index, length)) != NULL) {
					if (ch == ',')
						continue;
					if (ch == '}')
						return table;
					if (ch == '"') {
						string key = ParseString(array, ref index, length);
						if (NextChar(array, ref index, length) == ':') {
							object value = ParseValue(array, ref index, length, reviver);
							if (reviver != null) {
								value = reviver(key, value);
							}
							table[key] = value;
							continue;
						}
					}
					break;
				}
				ThrowSyntaxError(array, saveIndex, index, "ParseObject");
				return null;
			}

			static private ArrayList ParseArray(char[] array, ref int index, int length, Func<string, object, object> reviver)
			{
				char ch = NULL;
				int saveIndex = index;
				ArrayList list = new ArrayList();
				while ((ch = NextChar(array, ref index, length)) != NULL) {
					if (ch == ',')
						continue;
					if (ch == ']')
						return list;
					index--;
					object value = ParseValue(array, ref index, length, reviver);
					if (reviver != null) {
						value = reviver(list.Count.ToString(), value);
					}
					list.Add(value);
				}
				ThrowSyntaxError(array, saveIndex, index, "ParseArray");
				return null;
			}

			static private string ParseString(char[] array, ref int index, int length)
			{
				int saveIndex = index;
				StringBuilder builder = new StringBuilder();
				while (index < length) {
					char ch = array[index++];
					if (ch == '"')
						return builder.ToString();
					if (ch == '\\') {
						if (index >= length)
							break;
						ch = array[index++];
						if (ch == '"' || ch == '\\' || ch == '/') {
							builder.Append(ch);
						} else if (ch == 'b') {
							builder.Append('\b');
						} else if (ch == 'f') {
							builder.Append('\f');
						} else if (ch == 'n') {
							builder.Append('\n');
						} else if (ch == 'r') {
							builder.Append('\r');
						} else if (ch == 't') {
							builder.Append('\t');
						} else if (ch == 'u') {
							if (length - index < 4)
								break;
							for (int i = 0; i < 4; i++) {
								if (!IsHexDigit(array[index++]))
									ThrowSyntaxError(array, saveIndex, index, "Expecting '0-9', 'a-z', 'A-Z'.");
							}
							builder.Append((char)int.Parse(new string(array, index - 4, 4), NumberStyles.HexNumber));
						} else {
							ThrowSyntaxError(array, saveIndex, index, "Expecting '\"', '\\', '/', 'b', 'f', 'n', 'r', 't', got '" + ch + "'");
						}
					} else {
						builder.Append(ch);
					}
				}
				ThrowSyntaxError(array, saveIndex, index, "ParseString");
				return null;
			}

			static private object ParseNumber(char ch, char[] array, ref int index, int length)
			{
				int saveIndex = index - 1;
				bool frac = false, exp = false, negative = ch == '-';
				if (ch == '0') {
					ch = NextChar(array, ref index, length, false);
					if (IsDigit(ch))
						ThrowSyntaxError(array, saveIndex, index, "Expecting '.' or 'bound char', got '" + ch + "'");
					if (ch != '.') {
						index--;
						return 0;
					}
				} else if (negative) {
					ch = NextChar(array, ref index, length, false);
					if (ch != '.' && !IsDigit(ch))
						ThrowSyntaxError(array, saveIndex, index, "Expecting '.' or 'digit', got '" + ch + "'");
				}
				frac = ch == '.';
				if (frac) {
					ch = NextChar(array, ref index, length, false);
					if (!IsDigit(ch))
						ThrowSyntaxError(array, saveIndex, index, "Expecting 'digit', got '" + ch + "'");
				}
				while ((ch = NextChar(array, ref index, length, false)) != NULL) {
					if (IsDigit(ch))
						continue;
					if (ch == '.') {
						if (frac)
							ThrowSyntaxError(array, saveIndex, index, "ParseNumber");
						frac = true;
						if (!IsDigit(NextChar(array, ref index, length, false)))
							ThrowSyntaxError(array, saveIndex, index, "Expecting 'digit'");
					} else if (ch == 'e' || ch == 'E') {
						if (exp)
							ThrowSyntaxError(array, saveIndex, index, "ParseNumber");
						ch = NextChar(array, ref index, length, false);
						if (ch == '+' || ch == '-') {
							if (!IsDigit(NextChar(array, ref index, length, false)))
								ThrowSyntaxError(array, saveIndex, index, "Expecting 'digit'");
						} else if (!IsDigit(ch)) {
							ThrowSyntaxError(array, saveIndex, index, "Expecting 'digit', got '" + ch + "'");
						}
						exp = true;
					} else {
						--index;
						break;
					}
				}

				//object number;
				string value = new string(array, saveIndex, index - saveIndex);
				if (frac || exp) {
					double number = Convert.ToDouble(value);
					if (number >= float.MinValue && number <= float.MaxValue)
						return (float)number;
					return number;
				}
				if (negative) {
					long number = Convert.ToInt64(value);
					//if (number >= sbyte.MinValue)
					//	return (sbyte)number;
					//if (number >= short.MinValue)
					//	return (short)number;
					if (number >= int.MinValue)
						return (int)number;
					return (long)number;
				} else {
					ulong number = Convert.ToUInt64(value);
					//if (number <= byte.MaxValue)
					//	return (byte)number;
					//if (number <= ushort.MaxValue)
					//	return (ushort)number;
					//if (number <= uint.MaxValue)
					//	return (uint)number;
					if (number <= int.MaxValue)
						return (int)number;
					if (number <= long.MaxValue)
						return (long)number;
					return number;
				}
			}

			static private void ThrowSyntaxError(char[] array, int start, int index, string message)
			{
#if UNITY_EDITOR
				int count = index - start;
				throw new ArgumentException(string.Format("JSON.Parse: {0}\n{1}\n{2}^", message, new string(array, start, count), new string('-', count - 1)));
#endif
			}

		}
		#endregion
	}
}
