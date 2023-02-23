//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct int32 : IComparable, IFormattable, IComparable<int32>, IEquatable<int32>
	{
		static private int m_CryptoKey = (int)(UnityEngine.Random.value * int.MaxValue);

		private int m_Value;
		private int m_VerifyValue;

		public int32(int value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(int value)
		{
			m_Value = value ^ m_CryptoKey;
			m_VerifyValue = ~m_Value;
		}
		internal int InternalDecrypt()
		{
			if (m_Value != ~m_VerifyValue) {
				throw new SystemException("int32: Value was changed.");
			}

			return m_Value ^ m_CryptoKey;
		}

		#region operators, implicit, explicit
		public static implicit operator int32(int8 value)
		{
			return new int32(value.InternalDecrypt());
		}
		public static implicit operator int32(uint8 value)
		{
			return new int32(value.InternalDecrypt());
		}
		public static implicit operator int32(int16 value)
		{
			return new int32(value.InternalDecrypt());
		}
		public static implicit operator int32(uint16 value)
		{
			return new int32(value.InternalDecrypt());
		}
		public static explicit operator int32(uint32 value)
		{
			return new int32((int)value.InternalDecrypt());
		}
		public static explicit operator int32(int64 value)
		{
			return new int32((int)value.InternalDecrypt());
		}
		public static explicit operator int32(uint64 value)
		{
			return new int32((int)value.InternalDecrypt());
		}
		public static explicit operator int32(fixed32 value)
		{
			return new int32((int)value.InternalDecrypt());
		}
		public static explicit operator int32(fixed64 value)
		{
			return new int32((int)value.InternalDecrypt());
		}
		public static explicit operator int32(number value)
		{
			return new int32((int)value.InternalDecrypt());
		}

		#region c# types to int32
		public static implicit operator int32(int value)
		{
			return new int32(value);
		}
		public static explicit operator int32(uint value)
		{
			return new int32((int)value);
		}
		public static explicit operator int32(long value)
		{
			return new int32((int)value);
		}
		public static explicit operator int32(ulong value)
		{
			return new int32((int)value);
		}
		public static explicit operator int32(float value)
		{
			return new int32((int)value);
		}
		public static explicit operator int32(double value)
		{
			return new int32((int)value);
		}
		public static explicit operator int32(decimal value)
		{
			return new int32((int)value);
		}
		#endregion

		#region int32 to c# types
		public static explicit operator char(int32 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(int32 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(int32 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(int32 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(int32 value)
		{
			return (ushort)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator int(int32 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator int(int32 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static explicit operator uint(int32 value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static implicit operator long(int32 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator ulong(int32 value)
		{
			return (ulong)value.InternalDecrypt();
		}
		public static implicit operator float(int32 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(int32 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(int32 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static int32 operator ++(int32 input)
		{
			int value = input.InternalDecrypt() + 1;
			input.InternalEncrypt(value);
			return input;
		}
		public static int32 operator --(int32 input)
		{
			int value = input.InternalDecrypt() - 1;
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
			if (value is int) {
				return CompareTo((int)value);
			}
			if (!(value is int32)) {
				throw new ArgumentException("Value is not a int32");
			}
			return CompareTo(((int32)value).InternalDecrypt());
		}
		public int CompareTo(int32 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(int value)
		{
			int thisVal = this.InternalDecrypt();
			if (thisVal == value) {
				return 0;
			}
			if (thisVal > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(int obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(int32 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is int32 && Equals((int32)obj);
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