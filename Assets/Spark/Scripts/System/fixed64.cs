//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Spark
{
	[StructLayout(LayoutKind.Explicit)]
	public struct fixed64 : IComparable, IFormattable, IComparable<fixed64>, IEquatable<fixed64>
	{
		static private long m_CryptoKey = (long)(UnityEngine.Random.value * long.MaxValue);

		[FieldOffset(0)]
		private double m_DoubleValue;

		[FieldOffset(0)]
		private long m_LongValue;

		[FieldOffset(8)]
		private long m_VerifyValue;

		private fixed64(double value)
		{
			m_DoubleValue = m_LongValue = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(double value)
		{
			m_DoubleValue = value;
			m_LongValue ^= m_CryptoKey;
			m_VerifyValue = ~m_LongValue;
		}
		internal double InternalDecrypt()
		{
			if (m_LongValue != ~m_VerifyValue) {
				throw new SystemException("fixed64: Value was changed.");
			}

			double decrypted = 0d;
			long encrypted = m_LongValue;
			m_LongValue ^= m_CryptoKey;
			decrypted = m_DoubleValue;
			m_LongValue = encrypted;
			return decrypted;
		}

		#region operators, implicit, explicit
		public static implicit operator fixed64(int8 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(uint8 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(int16 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(uint16 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(int32 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(uint32 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(int64 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(uint64 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static implicit operator fixed64(fixed32 value)
		{
			return new fixed64(value.InternalDecrypt());
		}
		public static explicit operator fixed64(number value)
		{
			return new fixed64((double)value.InternalDecrypt());
		}

		#region c# types to float64
		public static implicit operator fixed64(double value)
		{
			return new fixed64(value);
		}
		public static explicit operator fixed64(decimal value)
		{
			return new fixed64((double)value);
		}
		#endregion

		#region float64 to c# types
		public static explicit operator char(fixed64 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(fixed64 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(fixed64 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(fixed64 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(fixed64 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static explicit operator int(fixed64 value)
		{
			return (int)value.InternalDecrypt();
		}
		public static explicit operator uint(fixed64 value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static explicit operator long(fixed64 value)
		{
			return (long)value.InternalDecrypt();
		}
		public static explicit operator ulong(fixed64 value)
		{
			return (ulong)value.InternalDecrypt();
		}
		public static explicit operator float(fixed64 value)
		{
			return (float)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator double(float64 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator double(fixed64 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static explicit operator decimal(fixed64 value)
		{
			return (decimal)value.InternalDecrypt();
		}
		#endregion

		public static fixed64 operator ++(fixed64 input)
		{
			double value = input.InternalDecrypt() + 1f;
			input.InternalEncrypt(value);
			return input;
		}
		public static fixed64 operator --(fixed64 input)
		{
			double value = input.InternalDecrypt() - 1f;
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
			if (value is double) {
				return CompareTo((double)value);
			}
			if (!(value is fixed64)) {
				throw new ArgumentException("Value is not a float64");
			}
			return CompareTo(((fixed64)value).InternalDecrypt());
		}
		public int CompareTo(fixed64 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(double value)
		{
			double thisVal = this.InternalDecrypt();

			if (double.IsPositiveInfinity(thisVal) && double.IsPositiveInfinity(value)) {
				return 0;
			}
			if (double.IsNegativeInfinity(thisVal) && double.IsNegativeInfinity(value)) {
				return 0;
			}
			if (double.IsNaN(value)) {
				if (double.IsNaN(thisVal)) {
					return 0;
				}
				return 1;
			} else {
				if (double.IsNaN(thisVal)) {
					if (double.IsNaN(value)) {
						return 0;
					}
					return -1;
				} else {
					if (thisVal > value) {
						return 1;
					}
					if (thisVal < value) {
						return -1;
					}
					return 0;
				}
			}
		}

		public bool Equals(double obj)
		{
			double thisVal = this.InternalDecrypt();
			if (double.IsNaN(obj)) {
				return double.IsNaN(thisVal);
			}
			return obj == thisVal;
		}
		public bool Equals(fixed64 obj)
		{
			return Equals(obj.InternalDecrypt());
		}
		public override bool Equals(object obj)
		{
			return obj is fixed64 && Equals((fixed64)obj);
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