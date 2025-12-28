using System.Linq.Expressions;
using RepositoryUoW.Domain.Common;

namespace RepositoryUoW.Core.Specifications;

/// <summary>
/// Specification pattern interface for complex query logic
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T> where T : BaseEntity
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}
