// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.CodeFirst
{
    using System.Linq.Expressions;
    using Xunit;

    public class CodePlex2846Mapping : TestBase
    {
        public class BaseClass
        {
            public int Id { get; set; }
        }

        public class Derived : BaseClass
        {

        }

        public class ContextWithDerivedEntity : DbContext
        {
            static ContextWithDerivedEntity()
            {
                Database.SetInitializer<ContextWithDerivedEntity>(null);
            }

            public DbSet<Derived> Deriveds { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Derived>()
                    .HasKey(CreateExpressionWithExtraConvertExplicitly<Derived>());
            }
        }

        public static Expression<Func<TType, int>> CreateExpressionWithExtraConvertExplicitly<TType>() where TType : BaseClass
        {
            var param = Expression.Parameter(typeof(TType), "u");

            var body = Expression.MakeMemberAccess(Expression.Convert(param, typeof(BaseClass)), typeof(TType).GetMember("Id")[0]);
            var ex = Expression.Lambda<Func<TType, int>>(body, param);
            return ex;
        }

        /// <summary>
        /// In the roslyn 1.1 compiler this expression will be emitted as u => Convert(u).Id because u refers to a
        /// type parameter.  Convert's Type property will be BaseClass (the limit type of TType).
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static Expression<Func<TType, int>> CreateExpressionWithExtraConvertInRoslyn11<TType>() where TType : BaseClass
        {
            return u => u.Id;
        }

        [Fact]
        public void Can_map_expression_with_extra_convert_in_expression()
        {
            Assert.DoesNotThrow(
                () =>
                {
                    using (var context = new ContextWithDerivedEntity())
                    {
                        Assert.NotNull(context.Deriveds.ToString());
                    }
                }
                );
        }
    }
}