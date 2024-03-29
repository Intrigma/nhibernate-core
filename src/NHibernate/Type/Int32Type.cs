using System;
using System.Collections;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NHibernate.Type
{
	/// <summary>
	/// Maps a <see cref="System.Int32"/> Property 
	/// to a <see cref="DbType.Int32"/> column.
	/// </summary>
	[Serializable]
	public partial class Int32Type : PrimitiveType, IDiscriminatorType, IVersionType
	{
		// Took the approach from here: https://github.com/dotnet/runtime/pull/79061
		private static readonly object[] SmallNumberCache = new object[byte.MaxValue + 1];

		/// <summary></summary>
		public Int32Type() : base(SqlTypeFactory.Int32)
		{
		}

		/// <summary></summary>
		public override string Name
		{
			get { return "Int32"; }
		}

		private static readonly object ZeroObject = GetInt32AsObject(0);

		public override object Get(DbDataReader rs, int index, ISessionImplementor session)
		{
			try
			{
				int value;

				var fieldType = rs.GetFieldType(index);
				if (fieldType == typeof(int))
				{
					value = rs.GetInt32(index);
				}
				else if (fieldType == typeof(long))
				{
					value = Convert.ToInt32(rs.GetInt64(index));
				}
				else if (fieldType == typeof(decimal))
				{
					value = Convert.ToInt32(rs.GetDecimal(index));
				}
				else
				{
					// anything else we haven't thought of goes through boxing
					value = rs[index] switch
					{
						// BigInteger does not implement IConvertible, but implements explicit conversion
						BigInteger bi => (int) bi,
						var c => Convert.ToInt32(c)
					};
				}

				return GetInt32AsObject(value);
			}
			catch (Exception ex)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
			}
		}

		public override object Get(DbDataReader rs, string name, ISessionImplementor session)
		{
			return Get(rs, rs.GetOrdinal(name), session);
		}

		public override System.Type ReturnedClass
		{
			get { return typeof(Int32); }
		}

		public override void Set(DbCommand rs, object value, int index, ISessionImplementor session)
		{
			rs.Parameters[index].Value = GetInt32AsObject(Convert.ToInt32(value));
		}

		// 6.0 TODO: rename "xml" parameter as "value": it is not a xml string. The fact it generally comes from a xml
		// attribute value is irrelevant to the method behavior.
		/// <inheritdoc />
		public object StringToObject(string xml)
		{
			// 6.0 TODO: remove warning disable/restore
#pragma warning disable 618
			return FromStringValue(xml);
#pragma warning restore 618
		}

		// 6.0 TODO: rename "xml" parameter as "value": it is not a xml string. The fact it generally comes from a xml
		// attribute value is irrelevant to the method behavior. Replace override keyword by virtual after having
		// removed the obsoleted base.
		/// <inheritdoc cref="IVersionType.FromStringValue"/>
#pragma warning disable 672
		public override object FromStringValue(string xml)
#pragma warning restore 672
		{
			return GetInt32AsObject(int.Parse(xml));
		}

		#region IVersionType Members

		public virtual object Next(object current, ISessionImplementor session)
		{
			return GetInt32AsObject((int) current + 1);
		}

		public virtual object Seed(ISessionImplementor session)
		{
			return GetInt32AsObject(1);
		}

		public IComparer Comparator
		{
			get { return Comparer<Int32>.Default; }
		}

		#endregion

		public override System.Type PrimitiveClass
		{
			get { return typeof(Int32); }
		}

		public override object DefaultValue => ZeroObject;

		public override string ObjectToSQLString(object value, Dialect.Dialect dialect)
		{
			return value.ToString();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object GetInt32AsObject(int value)
		{
			// it is more likely that value is bigger than the max cached number rather than it is negative
			if (SmallNumberCache.Length > value && value >= 0)
			{
				return SmallNumberCache[value] ?? CreateAndCacheObject(value);
			}

			return value;

			[MethodImpl(MethodImplOptions.NoInlining)] // keep rare usage out of fast path
			static object CreateAndCacheObject(int value)
			{
				return SmallNumberCache[value] = value;
			}
		}
	}
}
