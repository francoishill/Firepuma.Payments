using System.Linq.Expressions;

namespace Firepuma.Payments.Core.Infrastructure.Specifications;

public interface ISpecification<T>
{
    // https://github.com/ardalis/Specification/blob/2a2aecc26fd1930fdcfaebcaafc36873358d5456/ArdalisSpecification/src/Ardalis.Specification/ISpecification.cs

    IEnumerable<Expression<Func<T, bool>>> WhereExpressions { get; }

    IEnumerable<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; }

    int? Take { get; }

    int? Skip { get; }
}

public interface ISpecification<T, TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>> Selector { get; }
}