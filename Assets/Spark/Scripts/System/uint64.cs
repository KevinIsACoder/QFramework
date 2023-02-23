//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;

namespace Spark
{
	public struct uint64 : IComparable, IFormattable, IComparable<uint64>, IEquatable<uint64>
	{
		static private ulong m_CryptoKey = (ulong)(UnityEngine.Random.value * ulong.MaxValue);

		private ulong m_Value;
		private ulong m_VerifyValue;

		private uint64(ulong value)
		{
			m_Value = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(ulong value)
		{
			m_Value = value ^ m_CryptoKey;
			m_VerifyValue = ~m_Value;
		}
		internal ulong InternalDecrypt()
		{
			if (m_Value != ~m_VerifyValue) {
				throw new SystemException("uint64: Value was changed.");
			}

			return m_Value ^ m_CryptoKey;
		}

		#region operators, implicit, explicit
		public static explicit operator uint64(int8 value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}
		public static implicit operator uint64(uint8 value)
		{
			return new uint64(value.InternalDecrypt());
		}
		public static explicit operator uint64(int16 value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}
		public static implicit operator uint64(uint16 value)
		{
			return new uint64(value.InternalDecrypt());
		}
		public static explicit operator uint64(int32 value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}
		public static implicit operator uint64(uint32 value)
		{
			return new uint64(value.InternalDecrypt());
		}
		public static explicit operator uint64(int64 value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}
		public static explicit operator uint64(fixed32 value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}
		public static explicit operator uint64(fixed64 value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}
		public static explicit operator uint64(number value)
		{
			return new uint64((ulong)value.InternalDecrypt());
		}

		#region c# types to uint64
		public static explicit operator uint64(sbyte value)
		{
			return new uint64((ulong)value);
		}
		public static explicit operator uint64(short value)
		{
			return new uint64((ulong)value);
		}
		public static explicit operator uint64(int value)
		{
			return new uint64((ulong)value);
		}
		public static explicit operator uint64(long value)
		{
			return new uint64((ulong)value);
		}
		public static implicit operator uint64(ulong value)
		{
			return new uint64(value);
		}
		public static explicit operator uint64(float value)
		{
			return new uint64((ulong)value);
		}
		public static explicit operator uint64(double value)
		{
			return new uint64((ulong)value);
		}
		public static explicit operator uint64(decimal value)
		{
			return new uint64((ulong)value);
		}
		#endregion

		#region uint64 to c# types
		public static explicit operator char(uint64 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(uint64 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(uint64 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(uint64 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(uint64 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static explicit operator int(uint64 value)
		{
			return (int)value.InternalDecrypt();
		}
		public static explicit operator uint(uint64 value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static explicit operator long(uint64 value)
		{
			return (long)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator ulong(uint64 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator ulong(uint64 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static implicit operator float(uint64 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator double(uint64 value)
		{
			return value.InternalDecrypt();
		}
		public static implicit operator decimal(uint64 value)
		{
			return value.InternalDecrypt();
		}
		#endregion

		public static uint64 operator ++(uint64 input)
		{
			ulong value = input.InternalDecrypt() + 1L;
			input.InternalEncrypt(value);
			return input;
		}
		public static uint64 operator --(uint64 input)
		{
			ulong value = input.InternalDecrypt() - 1L;
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
			if (value is ulong) {
				return CompareTo((ulong)value);
			}
			if (!(value is uint64)) {
				throw new ArgumentException("Value is not a uint64");
			}
			return CompareTo(((uint64)value).InternalDecrypt());
		}
		public int CompareTo(uint64 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(ulong value)
		{
			ulong val = this.InternalDecrypt();
			if (val == value) {
				return 0;
			}
			if (val > value) {
				return 1;
			}
			return -1;
		}

		public bool Equals(ulong obj)
		{
			return InternalDecrypt() == obj;
		}
		public bool Equals(uint64 obj)
		{
			return obj.InternalDecrypt() == this.InternalDecrypt();
		}
		public override bool Equals(object obj)
		{
			return obj is uint64 && Equals((uint64)obj);
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