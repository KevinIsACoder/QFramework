//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Spark
{
	[StructLayout(LayoutKind.Explicit)]
	public struct number : IComparable, IFormattable, IComparable<number>, IEquatable<number>
	{
		static private long m_CryptoKey = (long)(UnityEngine.Random.value * long.MaxValue);

		[FieldOffset(0)]
		private decimal m_DecimalValue;

		[FieldOffset(0)]
		private long m_LongValue1;

		[FieldOffset(8)]
		private long m_LongValue2;

		[FieldOffset(16)]
		private long m_VerifyValue1;
		[FieldOffset(24)]
		private long m_VerifyValue2;

		private number(decimal value)
		{
			m_DecimalValue = m_LongValue1 = m_LongValue2 = m_VerifyValue1 = m_VerifyValue2 = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(decimal value)
		{
			m_DecimalValue = value;
			m_LongValue1 ^= m_CryptoKey;
			m_LongValue2 ^= m_CryptoKey;
			m_VerifyValue1 = ~m_LongValue1;
			m_VerifyValue2 = ~m_LongValue2;
		}
		internal decimal InternalDecrypt()
		{
			if (m_LongValue1 != ~m_VerifyValue1 || m_LongValue2 != ~m_VerifyValue2) {
				throw new SystemException("number: Value was changed.");
			}

			decimal decrypted = 0m;
			long encrypted1 = m_LongValue1, encrypted2 = m_LongValue2;
			m_LongValue1 ^= m_CryptoKey;
			m_LongValue2 ^= m_CryptoKey;
			decrypted = m_DecimalValue;
			m_LongValue1 = encrypted1;
			m_LongValue2 = encrypted2;
			return decrypted;
		}

		#region operators, implicit, explicit
		public static implicit operator number(int8 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(uint8 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(int16 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(uint16 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(int32 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(uint32 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(int64 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static implicit operator number(uint64 value)
		{
			return new number(value.InternalDecrypt());
		}
		public static explicit operator number(fixed32 value)
		{
			return new number((decimal)value.InternalDecrypt());
		}
		public static explicit operator number(fixed64 value)
		{
			return new number((decimal)value.InternalDecrypt());
		}

		#region c# types to int16
		public static explicit operator number(float value)
		{
			return new number((decimal)value);
		}
		public static explicit operator number(double value)
		{
			return new number((decimal)value);
		}
		public static implicit operator number(decimal value)
		{
			return new number(value);
		}
		#endregion

		#region int16 to c# types
		public static explicit operator char(number value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(number value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(number value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(number value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(number value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static explicit operator int(number value)
		{
			return (int)value.InternalDecrypt();
		}
		public static explicit operator uint(number value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static explicit operator long(number value)
		{
			return (long)value.InternalDecrypt();
		}
		public static explicit operator ulong(number value)
		{
			return (ulong)value.InternalDecrypt();
		}
		public static explicit operator float(number value)
		{
			return (float)value.InternalDecrypt();
		}

		public static explicit operator double(number value)
		{
			return (double)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator decimal(number value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator decimal(number value)
		{
			return value.InternalDecrypt();
		}
#endif

		#endregion

		public static number operator ++(number input)
		{
			decimal value = decimal.Add(input.InternalDecrypt(), 1m);
			input.InternalEncrypt(value);
			return input;
		}
		public static number operator --(number input)
		{
			decimal value = decimal.Add(input.InternalDecrypt(), -1m);
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
			if (value is decimal) {
				return decimal.Compare(InternalDecrypt(), (decimal)value);
			}
			if (!(value is number)) {
				throw new ArgumentException("Value is not a number");
			}
			return decimal.Compare(InternalDecrypt(), ((number)value).InternalDecrypt());
		}
		public int CompareTo(number value)
		{
			return decimal.Compare(InternalDecrypt(), value.InternalDecrypt());
		}
		public int CompareTo(decimal value)
		{
			return decimal.Compare(InternalDecrypt(), value);
		}

		public bool Equals(decimal obj)
		{
			return decimal.Equals(InternalDecrypt(), obj);
		}
		public bool Equals(number obj)
		{
			return decimal.Equals(InternalDecrypt(), obj.InternalDecrypt());
		}
		public override bool Equals(object obj)
		{
			return obj is number && decimal.Equals(InternalDecrypt(), ((number)obj).InternalDecrypt());
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