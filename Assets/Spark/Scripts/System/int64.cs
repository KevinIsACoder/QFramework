//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct int64 : IComparable, IFormattable, IComparable<int64>, IEquatable<int64>
	{
		static private long m_CryptoKey = (long)(UnityEngine.Random.value * long.MaxValue);

		private long m_Value;
		private long m_VerifyValue;

		private int64(long value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(long value)
		{
			m_Value = value ^ m_CryptoKey;
			m_VerifyValue = ~m_Value;
		}
		internal long InternalDecrypt()
		{
			if (m_Value != ~m_VerifyValue) {
				throw new SystemException("int64: Value was changed.");
			}

			return m_Value ^ m_CryptoKey;
		}

		#region operators, implicit, explicit
		public static implicit operator int64(int8 value)
		{
			return new int64(value.InternalDecrypt());
		}
		public static implicit operator int64(uint8 value)
		{
			return new int64(value.InternalDecrypt());
		}
		public static implicit operator int64(int16 value)
		{
			return new int64(value.InternalDecrypt());
		}
		public static implicit operator int64(uint16 value)
		{
			return new int64(value.InternalDecrypt());
		}
		public static implicit operator int64(int32 value)
		{
			return new int64(value.InternalDecrypt());
		}
		public static implicit operator int64(uint32 value)
		{
			return new int64(value.InternalDecrypt());
		}
		public static explicit operator int64(uint64 value)
		{
			return new int64((long)value.InternalDecrypt());
		}
		public static explicit operator int64(fixed32 value)
		{
			return new int64((long)value.InternalDecrypt());
		}
		public static explicit operator int64(fixed64 value)
		{
			return new int64((long)value.InternalDecrypt());
		}
		public static explicit operator int64(number value)
		{
			return new int64((long)value.InternalDecrypt());
		}

		#region c# types to int64
		public static implicit operator int64(long value)
		{
			return new int64(value);
		}
		public static explicit operator int64(ulong value)
		{
			return new int64((long)value);
		}
		public static explicit operator int64(float value)
		{
			return new int64((long)value);
		}
		public static explicit operator int64(double value)
		{
			return new int64((long)value);
		}
		public static explicit operator int64(decimal value)
		{
			return new int64((long)value);
		}
		#endregion

		#region int64 to c# types
		public static explicit operator char(int64 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(int64 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(int64 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(int64 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(int64 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static explicit operator int(int64 value)
		{
			return (int)value.InternalDecrypt();
		}
		public static explicit operator uint(int64 value)
		{
			return (uint)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator long(int64 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator long(int64 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static explicit operator ulong(int64 value)
		{
			return (ulong)value.InternalDecrypt();
		}
		public static implicit operator float(int64 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(int64 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(int64 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static int64 operator ++(int64 input)
		{
			long value = input.InternalDecrypt() + 1L;
			input.InternalEncrypt(value);
			return input;
		}
		public static int64 operator --(int64 input)
		{
			long value = input.InternalDecrypt() - 1L;
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
			if (value is long) {
				return CompareTo((long)value);
			}
			if (!(value is int64)) {
				throw new ArgumentException("Value is not a int64");
			}
			return CompareTo(((int64)value).InternalDecrypt());
		}
		public int CompareTo(int64 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(long value)
		{
			long thisVal = this.InternalDecrypt();
			if (thisVal == value) {
				return 0;
			}
			if (thisVal > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(long obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(int64 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is int64 && Equals((int64)obj);
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