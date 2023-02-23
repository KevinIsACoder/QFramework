//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct int16 : IComparable, IFormattable, IComparable<int16>, IEquatable<int16>
	{
		static private short m_CryptoKey = (short)(UnityEngine.Random.value * short.MaxValue);

		private short m_Value;
		private short m_VerifyValue;

		private int16(short value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(short value)
		{
			m_Value = (short)(value ^ m_CryptoKey);
			m_VerifyValue = (short)(~m_Value);
		}
		internal short InternalDecrypt()
		{
			if (m_Value != (short)(~m_VerifyValue)) {
				throw new SystemException("int16: Value was changed.");
			}

			return (short)(m_Value ^ m_CryptoKey);
		}

		#region operators, implicit, explicit
		public static implicit operator int16(int8 value)
		{
			return new int16(value.InternalDecrypt());
		}
		public static implicit operator int16(uint8 value)
		{
			return new int16(value.InternalDecrypt());
		}
		public static explicit operator int16(uint16 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(int32 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(uint32 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(int64 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(uint64 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(fixed32 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(fixed64 value)
		{
			return new int16((short)value.InternalDecrypt());
		}
		public static explicit operator int16(number value)
		{
			return new int16((short)value.InternalDecrypt());
		}

		#region c# types to int16
		public static explicit operator int16(char value)
		{
			return new int16((short)value);
		}
		public static implicit operator int16(short value)
		{
			return new int16(value);
		}
		public static explicit operator int16(ushort value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(int value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(uint value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(long value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(ulong value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(float value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(double value)
		{
			return new int16((short)value);
		}
		public static explicit operator int16(decimal value)
		{
			return new int16((short)value);
		}
		#endregion

		#region int16 to c# types
		public static explicit operator char(int16 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(int16 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(int16 value)
		{
			return (byte)value.InternalDecrypt();
		}

#if USE_STRICT
	public static explicit operator short(int16 value)
	{
		return value.InternalDecrypt();
	}
#else
		public static implicit operator short(int16 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static explicit operator ushort(int16 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static implicit operator int(int16 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator uint(int16 value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static implicit operator long(int16 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator ulong(int16 value)
		{
			return (ulong)value.InternalDecrypt();
		}
		public static implicit operator float(int16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(int16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(int16 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static int16 operator ++(int16 input)
		{
			short value = (short)(input.InternalDecrypt() + 1);
			input.InternalEncrypt(value);
			return input;
		}
		public static int16 operator --(int16 input)
		{
			short value = (short)(input.InternalDecrypt() - 1);
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
			if (value is short) {
				return CompareTo((short)value);
			}
			if (!(value is int16)) {
				throw new ArgumentException("Value is not a int16");
			}
			return CompareTo(((int16)value).InternalDecrypt());
		}
		public int CompareTo(int16 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(short value)
		{
			short thisVal = this.InternalDecrypt();
			if (thisVal == value) {
				return 0;
			}
			if (thisVal > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(short obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(int16 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is int16 && Equals((int16)obj);
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