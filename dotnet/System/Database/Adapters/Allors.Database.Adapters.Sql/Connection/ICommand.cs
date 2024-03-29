namespace Allors.Database.Adapters.Sql;

using System;
using System.Collections.Generic;
using System.Data;
using Allors.Database.Meta;

public interface ICommand : IDisposable
{
    CommandType CommandType { get; set; }

    string CommandText { get; set; }

    void AddInParameter(string parameterName, object value);

    void ObjectParameter(long objectId);

    void AddTypeParameter(Class @class);

    void AddCountParameter(int count);

    void AddUnitRoleParameter(RoleType roleType, object unit);

    void AddCompositeRoleParameter(long objectId);

    void AddAssociationParameter(long objectId);

    void AddCompositesRoleTableParameter(IEnumerable<long> objectIds);

    void ObjectTableParameter(IEnumerable<long> objectIds);

    void UnitTableParameter(RoleType roleType, IEnumerable<UnitRelation> relations);

    void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations);

    object ExecuteScalar();

    void ExecuteNonQuery();

    IReader ExecuteReader();

    object GetValue(IReader reader, string tag, int i);
}
