//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct int8 : IComparable, IFormattable, IComparable<int8>, IEquatable<int8>
	{
		static private sbyte m_CryptoKey = (sbyte)(UnityEngine.Random.value * sbyte.MaxValue);

		private sbyte m_Value;
		private sbyte m_VerifyValue;

		private int8(sbyte value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(sbyte value)
		{
			m_Value = (sbyte)(value ^ m_CryptoKey);
			m_VerifyValue = (sbyte)(~m_Value);
		}
		internal sbyte InternalDecrypt()
		{
			if (m_Value != (sbyte)(~m_VerifyValue)) {
				throw new SystemException("int8: Value was changed.");
			}

			return (sbyte)(m_Value ^ m_CryptoKey);
		}

		#region operators, implicit, explicit
		public static explicit operator int8(uint8 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(int16 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(uint16 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(int32 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(uint32 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(int64 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(uint64 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(fixed32 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(fixed64 value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}
		public static explicit operator int8(number value)
		{
			return new int8((sbyte)value.InternalDecrypt());
		}

		#region c# types to int8
		public static explicit operator int8(char value)
		{
			return new int8((sbyte)value);
		}
		public static implicit operator int8(sbyte value)
		{
			return new int8(value);
		}
		public static explicit operator int8(byte value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(short value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(ushort value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(int value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(uint value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(long value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(ulong value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(float value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(double value)
		{
			return new int8((sbyte)value);
		}
		public static explicit operator int8(decimal value)
		{
			return new int8((sbyte)value);
		}
		#endregion

		#region int8 to c# types
		public static explicit operator char(int8 value)
		{
			return (char)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator sbyte(int8 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator sbyte(int8 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static explicit operator byte(int8 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static implicit operator short(int8 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator ushort(int8 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static implicit operator int(int8 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator uint(int8 value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static implicit operator long(int8 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator ulong(int8 value)
		{
			return (ulong)value.InternalDecrypt();
		}
		public static implicit operator float(int8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(int8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(int8 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static int8 operator ++(int8 input)
		{
			sbyte value = (sbyte)(input.InternalDecrypt() + 1);
			input.InternalEncrypt(value);
			return input;
		}
		public static int8 operator --(int8 input)
		{
			sbyte value = (sbyte)(input.InternalDecrypt() - 1);
			input.InternalEncrypt(value);
			return input;
		}
		#endregion

		#region overrides, interface implementations
		public int CompareTo(object value)
		{
			if (value == null) {
				return 1;
			}
			if (value is sbyte) {
				return CompareTo((sbyte)value);
			}
			if (!(value is int8)) {
				throw new ArgumentException("Value is not a int8");
			}
			return CompareTo(((int8)value).InternalDecrypt());
		}
		public int CompareTo(int8 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(sbyte value)
		{
			sbyte val = this.InternalDecrypt();
			if (val == value) {
				return 0;
			}
			if (val > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(sbyte obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(int8 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is int8 && Equals((int8)obj);
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}
		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}

		#endregion
	}
}