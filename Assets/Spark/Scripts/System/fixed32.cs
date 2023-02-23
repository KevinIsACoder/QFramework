//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Spark
{
	[StructLayout(LayoutKind.Explicit)]
	public struct fixed32 : IComparable, IFormattable, IComparable<fixed32>, IEquatable<fixed32>
	{
		static private int m_CryptoKey = (int)(UnityEngine.Random.value * int.MaxValue);

		[FieldOffset(0)]
		private float m_FloatValue;

		[FieldOffset(0)]
		private int m_IntValue;

		[FieldOffset(4)]
		private int m_VerifyValue;

		private fixed32(float value)
		{
			m_FloatValue = m_IntValue = m_VerifyValue = 0;
			InternalEncrypt(value);
		}

		internal void InternalEncrypt(float value)
		{
			m_FloatValue = value;
			m_IntValue ^= m_CryptoKey;
			m_VerifyValue = ~m_IntValue;
		}
		internal float InternalDecrypt()
		{
			if (m_IntValue != ~m_VerifyValue) {
				throw new SystemException("fixed32: Value was changed.");
			}

			float decrypted = 0f;
			int encrypted = m_IntValue;
			m_IntValue ^= m_CryptoKey;
			decrypted = m_FloatValue;
			m_IntValue = encrypted;
			return decrypted;
		}

		#region operators, implicit, explicit
		public static implicit operator fixed32(int8 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(uint8 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(int16 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(uint16 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(int32 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(uint32 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(int64 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static implicit operator fixed32(uint64 value)
		{
			return new fixed32(value.InternalDecrypt());
		}
		public static explicit operator fixed32(fixed64 value)
		{
			return new fixed32((float)value.InternalDecrypt());
		}
		public static explicit operator fixed32(number value)
		{
			return new fixed32((float)value.InternalDecrypt());
		}

		#region c# types to float32
		public static implicit operator fixed32(float value)
		{
			return new fixed32(value);
		}
		public static explicit operator fixed32(double value)
		{
			return new fixed32((float)value);
		}
		public static explicit operator fixed32(decimal value)
		{
			return new fixed32((float)value);
		}
		#endregion

		#region float32 to c# types
		public static explicit operator char(fixed32 value)
		{
			return (char)value.InternalDecrypt();
		}
		public static explicit operator sbyte(fixed32 value)
		{
			return (sbyte)value.InternalDecrypt();
		}
		public static explicit operator byte(fixed32 value)
		{
			return (byte)value.InternalDecrypt();
		}
		public static explicit operator short(fixed32 value)
		{
			return (short)value.InternalDecrypt();
		}
		public static explicit operator ushort(fixed32 value)
		{
			return (ushort)value.InternalDecrypt();
		}
		public static explicit operator int(fixed32 value)
		{
			return (int)value.InternalDecrypt();
		}
		public static explicit operator uint(fixed32 value)
		{
			return (uint)value.InternalDecrypt();
		}
		public static explicit operator long(fixed32 value)
		{
			return (long)value.InternalDecrypt();
		}
		public static explicit operator ulong(fixed32 value)
		{
			return (ulong)value.InternalDecrypt();
		}

#if USE_STRICT
		public static explicit operator float(float32 value)
		{
			return value.InternalDecrypt();
		}
#else
		public static implicit operator float(fixed32 value)
		{
			return value.InternalDecrypt();
		}
#endif

		public static implicit operator double(fixed32 value)
		{
			return value.InternalDecrypt();
		}
		public static explicit operator decimal(fixed32 value)
		{
			return (decimal)value.InternalDecrypt();
		}
		#endregion

		public static fixed32 operator ++(fixed32 input)
		{
			float value = input.InternalDecrypt() + 1f;
			input.InternalEncrypt(value);
			return input;
		}
		public static fixed32 operator --(fixed32 input)
		{
			float value = input.InternalDecrypt() - 1f;
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
			if (value is float) {
				return CompareTo((float)value);
			}
			if (!(value is fixed32)) {
				throw new ArgumentException("Value is not a float32");
			}
			return CompareTo(((fixed32)value).InternalDecrypt());
		}
		public int CompareTo(fixed32 value)
		{
			return CompareTo(value.InternalDecrypt());
		}
		public int CompareTo(float value)
		{
			float thisVal = this.InternalDecrypt();

			if (float.IsPositiveInfinity(thisVal) && float.IsPositiveInfinity(value)) {
				return 0;
			}
			if (float.IsNegativeInfinity(thisVal) && float.IsNegativeInfinity(value)) {
				return 0;
			}
			if (float.IsNaN(value)) {
				if (float.IsNaN(thisVal)) {
					return 0;
				}
				return 1;
			} else {
				if (float.IsNaN(thisVal)) {
					if (float.IsNaN(value)) {
						return 0;
					}
					return -1;
				} else {
					if (thisVal == value) {
						return 0;
					}
					if (thisVal > value) {
						return 1;
					}
					return -1;
				}
			}
		}

		public bool Equals(float obj)
		{
			float thisVal = this.InternalDecrypt();
			if (float.IsNaN(obj)) {
				return float.IsNaN(thisVal);
			}
			return obj == thisVal;
		}
		public bool Equals(fixed32 obj)
		{
			return Equals(obj.InternalDecrypt());
		}
		public override bool Equals(object obj)
		{
			return obj is fixed32 && Equals((fixed32)obj);
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