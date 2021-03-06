﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Linq;
using System.Transactions;
using NHibernate.Dialect;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class QueryLockAsync : LinqTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsSelectForUpdate;
		}

		protected override bool AppliesTo(ISessionFactoryImplementor factory)
		{
			return !(factory.ConnectionProvider.Driver is OdbcDriver);
		}

		[Test]
		public async Task CanSetLockLinqQueriesOuterAsync()
		{
			using (session.BeginTransaction())
			{
				var result = await ((from e in db.Customers
				              select e).WithLock(LockMode.Upgrade).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(91));
				Assert.That(session.GetCurrentLockMode(result[0]), Is.EqualTo(LockMode.Upgrade));
				await (AssertSeparateTransactionIsLockedOutAsync(result[0].CustomerId));
			}
		}

		[Test]
		public async Task CanSetLockLinqQueriesAsync()
		{
			using (session.BeginTransaction())
			{
				var result = await ((from e in db.Customers.WithLock(LockMode.Upgrade)
				              select e).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(91));
				Assert.That(session.GetCurrentLockMode(result[0]), Is.EqualTo(LockMode.Upgrade));
				await (AssertSeparateTransactionIsLockedOutAsync(result[0].CustomerId));
			}
		}

		[Test]
		public async Task CanSetLockOnJoinHqlAsync()
		{
			using (session.BeginTransaction())
			{
				await (session
					.CreateQuery("select o from Customer c join c.Orders o")
					.SetLockMode("o", LockMode.Upgrade)
					.ListAsync());
			}
		}

		[Test]
		public async Task CanSetLockOnJoinAsync()
		{
			using (session.BeginTransaction())
			{
				var result = await ((from c in db.Customers
				              from o in c.Orders.WithLock(LockMode.Upgrade)
				              select o).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(830));
				Assert.That(session.GetCurrentLockMode(result[0]), Is.EqualTo(LockMode.Upgrade));
			}
		}

		[Test]
		public async Task CanSetLockOnJoinOuterAsync()
		{
			using (session.BeginTransaction())
			{
				var result = await ((from c in db.Customers
				              from o in c.Orders
				              select o).WithLock(LockMode.Upgrade).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(830));
				Assert.That(session.GetCurrentLockMode(result[0]), Is.EqualTo(LockMode.Upgrade));
			}
		}

		[Test]
		public void CanSetLockOnJoinOuterNotSupportedAsync()
		{
			using (session.BeginTransaction())
			{
				var query = (
					from c in db.Customers
					from o in c.Orders
					select new {o, c}
				).WithLock(LockMode.Upgrade);

				Assert.ThrowsAsync<NotSupportedException>(() => query.ToListAsync());
			}
		}

		[Test]
		public async Task CanSetLockOnJoinOuter2HqlAsync()
		{
			using (session.BeginTransaction())
			{
				await (session
					.CreateQuery("select o, c from Customer c join c.Orders o")
					.SetLockMode("o", LockMode.Upgrade)
					.SetLockMode("c", LockMode.Upgrade)
					.ListAsync());
			}
		}

		[Test]
		public async Task CanSetLockOnBothJoinAndMainAsync()
		{
			using (session.BeginTransaction())
			{
				var result = await ((
					from c in db.Customers.WithLock(LockMode.Upgrade)
					from o in c.Orders.WithLock(LockMode.Upgrade)
					select new {o, c}
				).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(830));
				Assert.That(session.GetCurrentLockMode(result[0].o), Is.EqualTo(LockMode.Upgrade));
				Assert.That(session.GetCurrentLockMode(result[0].c), Is.EqualTo(LockMode.Upgrade));
			}
		}

		[Test]
		public async Task CanSetLockOnBothJoinAndMainComplexAsync()
		{
			using (session.BeginTransaction())
			{
				var result = await ((
					from c in db.Customers.Where(x => true).WithLock(LockMode.Upgrade)
					from o in c.Orders.Where(x => true).WithLock(LockMode.Upgrade)
					select new {o, c}
				).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(830));
				Assert.That(session.GetCurrentLockMode(result[0].o), Is.EqualTo(LockMode.Upgrade));
				Assert.That(session.GetCurrentLockMode(result[0].c), Is.EqualTo(LockMode.Upgrade));
			}
		}

		[Test]
		public async Task CanSetLockOnLinqPagingQueryAsync()
		{
			Assume.That(TestDialect.SupportsSelectForUpdateWithPaging, Is.True, "Dialect does not support locking in subqueries");
			using (session.BeginTransaction())
			{
				var result = await ((from e in db.Customers
				              select e).Skip(5).Take(5).WithLock(LockMode.Upgrade).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(5));
				Assert.That(session.GetCurrentLockMode(result[0]), Is.EqualTo(LockMode.Upgrade));

				await (AssertSeparateTransactionIsLockedOutAsync(result[0].CustomerId));
			}
		}

		[Test]
		public async Task CanLockBeforeSkipOnLinqOrderedPageQueryAsync()
		{
			Assume.That(TestDialect.SupportsSelectForUpdateWithPaging, Is.True, "Dialect does not support locking in subqueries");
			using (session.BeginTransaction())
			{
				var result = await ((from e in db.Customers
				              orderby e.CompanyName
				              select e)
				             .WithLock(LockMode.Upgrade).Skip(5).Take(5).ToListAsync());

				Assert.That(result, Has.Count.EqualTo(5));
				Assert.That(session.GetCurrentLockMode(result[0]), Is.EqualTo(LockMode.Upgrade));
			
				await (AssertSeparateTransactionIsLockedOutAsync(result[0].CustomerId));
			}
		}

		private Task AssertSeparateTransactionIsLockedOutAsync(string customerId, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
				using (var s2 = OpenSession())
				using (s2.BeginTransaction())
				{
					// TODO: We should try to verify that the exception actually IS a locking failure and not something unrelated.
					Assert.ThrowsAsync<GenericADOException>(
					async () =>
						{
							var result2 = await ((
								from e in s2.Query<Customer>()
								where e.CustomerId == customerId
								select e
							).WithLock(LockMode.UpgradeNoWait)
							 .WithOptions(o => o.SetTimeout(5))
							 .ToListAsync(cancellationToken));
							Assert.That(result2, Is.Not.Null);
						},
					"Expected an exception to indicate locking failure due to already locked.");
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Verify that different lock modes are respected even if the query is otherwise exactly the same.")]
		public async Task CanChangeLockModeForQueryAsync()
		{
			// Limit to a few dialects where we know the "nowait" keyword is used to make life easier.
			Assume.That(Dialect is MsSql2000Dialect || Dialect is Oracle8iDialect || Dialect is PostgreSQL81Dialect);

			using (session.BeginTransaction())
			{
				var result = await (BuildQueryableAllCustomers(db.Customers, LockMode.Upgrade).ToListAsync());
				Assert.That(result, Has.Count.EqualTo(91));

				using (var logSpy = new SqlLogSpy())
				{
					// Only difference in query is the lockmode - make sure it gets picked up.
					var result2 = await (BuildQueryableAllCustomers(session.Query<Customer>(), LockMode.UpgradeNoWait)
						.ToListAsync());
					Assert.That(result2, Has.Count.EqualTo(91));

					Assert.That(logSpy.GetWholeLog().ToLower(), Does.Contain("nowait"));
				}
			}
		}

		private static IQueryable<Customer> BuildQueryableAllCustomers(
			IQueryable<Customer> dbCustomers,
			LockMode lockMode)
		{
			return (from e in dbCustomers select e).WithLock(lockMode).WithOptions(o => o.SetTimeout(5));
		}
	}
}
