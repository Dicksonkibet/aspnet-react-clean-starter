using System.Linq.Expressions;
using CleanStart.Domain.Common;

namespace CleanStart.Application.Common.Specifications;

/// <summary>Base class every concrete specification inherits from. Subclass this to
/// express a reusable query, e.g.:
///
///   public class IncompleteTodosSpec : BaseSpecification&lt;TodoItem&gt;
///   {
///       public IncompleteTodosSpec() : base(t =&gt; !t.IsDone) =&gt; ApplyOrderByDescending(t =&gt; t.CreatedAt);
///   }
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
{
    protected BaseSpecification() { }
    protected BaseSpecification(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);
    protected void AddInclude(string includeString) => IncludeStrings.Add(includeString);
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) => OrderByDescending = orderByDescExpression;
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
