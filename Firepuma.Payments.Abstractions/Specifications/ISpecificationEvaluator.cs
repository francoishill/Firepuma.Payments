namespace Firepuma.Payments.Abstractions.Specifications;

public interface ISpecificationEvaluator<T> where T : class
{
    // https://github.com/ardalis/Specification/blob/2a2aecc26fd1930fdcfaebcaafc36873358d5456/ArdalisSpecification/src/Ardalis.Specification/ISpecificationEvaluator.cs

    IQueryable<TResult> GetQuery<TResult>(IQueryable<T> inputQuery, ISpecification<T, TResult> specification);
    IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification);
}