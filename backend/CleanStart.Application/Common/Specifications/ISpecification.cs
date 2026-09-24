using System.Linq.Expressions;

namespace CleanStart.Application.Common.Specifications;

/// <summary>Describes a query (filter + includes + ordering + paging) without leaking
/// EF Core into the Application layer. Concrete specs live next to the feature that
/// needs them (e.g. TodoItems/Specifications/IncompleteTodosSpec.cs).</summary>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Skip { get; }
    int Take { get; }
    bool IsPagingEnabled { get; }
}
