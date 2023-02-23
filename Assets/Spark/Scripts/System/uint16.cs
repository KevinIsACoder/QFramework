//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct uint16 : IComparable, IFormattable, IComparable<uint16>, IEquatable<uint16>
	{
		static private ushort m_CryptoKey = (ushort)(UnityEngine.Random.value * ushort.MaxValue);

		private ushort m_Value;
		private ushort m_VerifyValue;

		private uint16(ushort value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(ushort value)
		{
			m_Value = (ushort)(value ^ m_CryptoKey);
			m_VerifyValue = (ushort)(~m_Value);
		}
		internal ushort InternalDecrypt()
		{
			if (m_Value != (ushort)(~m_VerifyValue)) {
				throw new SystemException("uint16: Value was changed.");
			}

			return (ushort)(m_Value ^ m_CryptoKey);
		}

		#region operators, implicit, explicit
		public static explicit operator uint16(int8 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static implicit operator uint16(uint8 value)
		{
			return new uint16(value.InternalDecrypt());
		}
		public static explicit operator uint16(int16 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(int32 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(uint32 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(int64 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(uint64 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(fixed32 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(fixed64 value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}
		public static explicit operator uint16(number value)
		{
			return new uint16((ushort)value.InternalDecrypt());
		}

		#region c# types to uint16
		public static explicit operator uint16(char value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(sbyte value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(short value)
		{
			return new uint16((ushort)value);
		}
		public static implicit operator uint16(ushort value)
		{
			return new uint16(value);
		}
		public static explicit operator uint16(int value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(uint value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(long value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(ulong value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(float value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(double value)
		{
			return new uint16((ushort)value);
		}
		public static explicit operator uint16(decimal value)
		{
			return new uint16((ushort)value);
		}
		#endregion

		#region uint16 to c# types
		public static explicit operator char(uint16 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(uint16 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(uint16 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(uint16 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static implicit operator ushort(uint16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator int(uint16 value)
		{
			return value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator uint(uint16 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator uint(uint16 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static implicit operator long(uint16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator ulong(uint16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator float(uint16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(uint16 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(uint16 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static uint16 operator ++(uint16 input)
		{
			ushort value = (ushort)(input.InternalDecrypt() + 1);
			input.InternalEncrypt(value);
			return input;
		}
		public static uint16 operator --(uint16 input)
		{
			ushort value = (ushort)(input.InternalDecrypt() - 1);
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
			if (value is ushort) {
				return CompareTo((ushort)value);
			}
			if (!(value is uint16)) {
				throw new ArgumentException("Value is not a int16");
			}
			return CompareTo(((uint16)value).InternalDecrypt());
		}
		public int CompareTo(uint16 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(ushort value)
		{
			ushort val = this.InternalDecrypt();
			if (val == value) {
				return 0;
			}
			if (val > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(ushort obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(uint16 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is uint16 && Equals((uint16)obj);
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