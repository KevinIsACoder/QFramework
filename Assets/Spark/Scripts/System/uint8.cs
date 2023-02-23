//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct uint8 : IComparable, IFormattable, IComparable<uint8>, IEquatable<uint8>
	{
		static private byte m_CryptoKey = (byte)(UnityEngine.Random.value * byte.MaxValue);

		private byte m_Value;
		private byte m_VerifyValue;

		private uint8(byte value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(byte value)
		{
			m_Value = (byte)(value ^ m_CryptoKey);
			m_VerifyValue = (byte)(~m_Value);
		}
		internal byte InternalDecrypt()
		{
			if (m_Value != (byte)(~m_VerifyValue)) {
				throw new SystemException("uint8: Value was changed.");
			}

			return (byte)(m_Value ^ m_CryptoKey);
		}

		#region operators, implicit, explicit
		public static explicit operator uint8(int8 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(int16 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(uint16 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(int32 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(uint32 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(int64 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(uint64 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(fixed32 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(fixed64 value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}
		public static explicit operator uint8(number value)
		{
			return new uint8((byte)value.InternalDecrypt());
		}

		#region c# types to uint8
		public static explicit operator uint8(char value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(sbyte value)
		{
			return new uint8((byte)value);
		}
		public static implicit operator uint8(byte value)
		{
			return new uint8(value);
		}
		public static explicit operator uint8(short value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(ushort value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(int value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(uint value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(long value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(ulong value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(float value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(double value)
		{
			return new uint8((byte)value);
		}
		public static explicit operator uint8(decimal value)
		{
			return new uint8((byte)value);
		}
		#endregion

		#region uint8 to c# types
		public static explicit operator char(uint8 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(uint8 value)
		{
			return (sbyte)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator byte(uint8 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator byte(uint8 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static implicit operator short(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator ushort(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator int(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator uint(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator long(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator ulong(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator float(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(uint8 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(uint8 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static uint8 operator ++(uint8 input)
		{
			byte value = (byte)(input.InternalDecrypt() + 1);
			input.InternalEncrypt(value);
			return input;
		}
		public static uint8 operator --(uint8 input)
		{
			byte value = (byte)(input.InternalDecrypt() - 1);
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
			if (value is byte) {
				return CompareTo((byte)value);
			}
			if (!(value is uint8)) {
				throw new ArgumentException("Value is not a uint8");
			}
			return CompareTo(((uint8)value).InternalDecrypt());
		}
		public int CompareTo(uint8 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(byte value)
		{
			byte val = this.InternalDecrypt();
			if (val == value) {
				return 0;
			}
			if (val > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(byte obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(uint8 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is uint8 && Equals((uint8)obj);
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