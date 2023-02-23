//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct uint32 : IComparable, IFormattable, IComparable<uint32>, IEquatable<uint32>
	{
		static private uint m_CryptoKey = (uint)(UnityEngine.Random.value * uint.MaxValue);

		private uint m_Value;
		private uint m_VerifyValue;

		private uint32(uint value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(uint value)
		{
			m_Value = value ^ m_CryptoKey;
			m_VerifyValue = ~m_Value;
		}
		internal uint InternalDecrypt()
		{
			if (m_Value != ~m_VerifyValue) {
				throw new SystemException("uint32: Value was changed.");
			}

			return m_Value ^ m_CryptoKey;
		}

		#region operators, implicit, explicit
		public static explicit operator uint32(int8 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static implicit operator uint32(uint8 value)
		{
			return new uint32(value.InternalDecrypt());
		}
		public static explicit operator uint32(int16 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static implicit operator uint32(uint16 value)
		{
			return new uint32(value.InternalDecrypt());
		}
		public static explicit operator uint32(int32 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static explicit operator uint32(int64 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static explicit operator uint32(uint64 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static explicit operator uint32(fixed32 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static explicit operator uint32(fixed64 value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}
		public static explicit operator uint32(number value)
		{
			return new uint32((uint)value.InternalDecrypt());
		}

		#region c# types to uint32
		public static explicit operator uint32(sbyte value)
		{
			return new uint32((uint)value);
		}
		public static explicit operator uint32(short value)
		{
			return new uint32((uint)value);
		}
		public static explicit operator uint32(int value)
		{
			return new uint32((uint)value);
		}
		public static implicit operator uint32(uint value)
		{
			return new uint32(value);
		}
		public static explicit operator uint32(long value)
		{
			return new uint32((uint)value);
		}
		public static explicit operator uint32(ulong value)
		{
			return new uint32((uint)value);
		}
		public static explicit operator uint32(float value)
		{
			return new uint32((uint)value);
		}
		public static explicit operator uint32(double value)
		{
			return new uint32((uint)value);
		}
		public static explicit operator uint32(decimal value)
		{
			return new uint32((uint)value);
		}
		#endregion

		#region uint32 to c# types
		public static explicit operator char(uint32 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(uint32 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(uint32 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(uint32 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(uint32 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static explicit operator int(uint32 value)
		{
			return (int)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator uint(uint32 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator uint(uint32 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static implicit operator long(uint32 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator ulong(uint32 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator float(uint32 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(uint32 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(uint32 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static uint32 operator ++(uint32 input)
		{
			uint value = input.InternalDecrypt() + 1;
			input.InternalEncrypt(value);
			return input;
		}
		public static uint32 operator --(uint32 input)
		{
			uint value = input.InternalDecrypt() - 1;
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
			if (value is uint) {
				return CompareTo((uint)value);
			}
			if (!(value is uint32)) {
				throw new ArgumentException("Value is not a uint32");
			}
			return CompareTo(((uint32)value).InternalDecrypt());
		}
		public int CompareTo(uint32 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(uint value)
		{
			uint val = this.InternalDecrypt();
			if (val == value) {
				return 0;
			}
			if (val > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(uint obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(uint32 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is uint32 && Equals((uint32)obj);
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