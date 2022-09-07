using System.Linq.Expressions;

namespace Firepuma.Payments.Core.Infrastructure.Specifications;

public class Specification<T> : ISpecification<T>
{
    public IEnumerable<Expression<Func<T, bool>>> WhereExpressions { get; }
        = new List<Expression<Func<T, bool>>>();

    public IEnumerable<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; }
        = new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();

    public int? Take { get; set; } = null;

    public int? Skip { get; set; } = null;
}