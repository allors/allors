﻿// <copyright file="ICompositePredicate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ICompositePredicate type.</summary>

namespace Allors.Database;

using System;
using System.Collections.Generic;
using Allors.Database.Meta;

/// <summary>
///     <para>
///         A Predicate is an expression that either returns true, false or unknown (Three Value Logic).
///         A CompositePredicate is a Predicate that can contain other Predicates.
///     </para>
///     <para>
///         CompositePredicates are applied to other predicates (And and Or) or to a single other predicate (Not).
///         Non-CompositePredicates are applied to objects (Instanceof) or to
///         relations of those objects (Instanceof,Exists,NotExists,Contains,Equals,Like,LessThan,GreaterThan and Between).
///     </para>
///     <para>
///         Adding a CompositePredicate returns the newly added CompositePredicate,
///         adding a Non-CompositePredicate returns the composing CompositePredicate to which the Non-CompositePredicate
///         was added.
///         This allows for chained method invocations, e.g predicate.AddEquals(...).AddEquals(...)
///     </para>
/// </summary>
public interface ICompositePredicate
{
    /// <summary>
    ///     Adds a CompositePredicate that evaluates to true if all of its composed predicates evaluate to true.
    ///     This predicate is ignored when there are no composed predicates.
    /// </summary>
    /// <returns>the newly added CompositePredicate.</returns>
    ICompositePredicate AddAnd(Action<ICompositePredicate> init = null);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation is between the first and the
    ///     second object.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="firstValue">The first object.</param>
    /// <param name="secondValue">The second object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddBetween(RoleType role, object firstValue, object secondValue);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the role of the object under evaluation is contained in
    ///     the containingExtent.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="containingExtent">The extent.</param>
    /// <returns>this CompositePredicate.</returns>
    IPredicate AddIntersects(RoleType role, IExtent<IObject> containingExtent);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the role of the object under evaluation is contained in
    ///     the containingExtent.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="containingEnumerable">The enumerable.</param>
    /// <returns>This CompositePredicate. </returns>
    IPredicate AddIntersects(RoleType role, IEnumerable<IObject> containingEnumerable);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the association of the object under evaluation is
    ///     contained in the containingExtent.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <param name="containingExtent">The extent.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddIntersects(AssociationType association, IExtent<IObject> containingExtent);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the role of the object under evaluation is contained in
    ///     the containingExtent.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <param name="containingEnumerable">The enumerable.</param>
    /// <returns>This CompositePredicate. </returns>
    IPredicate AddIntersects(AssociationType association, IEnumerable<IObject> containingEnumerable);
    
    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the role of the object under evaluation is contained in
    ///     the containingExtent.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="containingExtent">The extent.</param>
    /// <returns>this CompositePredicate.</returns>
    IPredicate AddIn(RoleType role, IExtent<IObject> containingExtent);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the role of the object under evaluation is contained in
    ///     the containingExtent.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="containingEnumerable">The enumerable.</param>
    /// <returns>This CompositePredicate. </returns>
    IPredicate AddIn(RoleType role, IEnumerable<IObject> containingEnumerable);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the association of the object under evaluation is
    ///     contained in the containingExtent.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <param name="containingExtent">The extent.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddIn(AssociationType association, IExtent<IObject> containingExtent);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if any object of the role of the object under evaluation is contained in
    ///     the containingExtent.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <param name="containingEnumerable">The enumerable.</param>
    /// <returns>This CompositePredicate. </returns>
    IPredicate AddIn(AssociationType association, IEnumerable<IObject> containingEnumerable);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation contains the allorsObject.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="containedObject">The allors object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddHas(RoleType role, IObject containedObject);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the association of the object under evaluation contains the
    ///     allorsObject.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <param name="containedObject">The allors object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddHas(AssociationType association, IObject containedObject);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the object under evaluation equals the allorsObject.
    /// </summary>
    /// <param name="allorsObject">The allors object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddEquals(IObject allorsObject);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation equals the object (unit or
    ///     composite).
    /// </summary>
    /// <param name="roleType">The role .</param>
    /// <param name="valueOrAllorsObject">The object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddEquals(RoleType roleType, object valueOrAllorsObject);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the association of the object under evaluation equals the allorsObject.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <param name="allorsObject">The allors object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddEquals(AssociationType association, IObject allorsObject);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation exists.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddExists(RoleType role);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the association of the object under evaluation exists.
    /// </summary>
    /// <param name="association">The assocation.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddExists(AssociationType association);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation is greater than the object.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="value">The object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddGreaterThan(RoleType role, object value);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the object under evaluation is an state of the IObjectType.
    /// </summary>
    /// <param name="objectType">the IObjectType.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddInstanceOf(Composite objectType);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation is an state of the IObjectType.
    /// </summary>
    /// <param name="role">the RoleType .</param>
    /// <param name="objectType">the IObjectType.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddInstanceOf(RoleType role, Composite objectType);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the association of the object under evaluation is an state of the
    ///     IObjectType.
    /// </summary>
    /// <param name="association">the AssociationType.</param>
    /// <param name="objectType">the IObjectType.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddInstanceOf(AssociationType association, Composite objectType);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation is less than the object.
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="value">The object.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddLessThan(RoleType role, object value);

    /// <summary>
    ///     Adds a Predicate that evaluates to true if the role of the object under evaluation is like the string (Sql like).
    /// </summary>
    /// <param name="role">The role .</param>
    /// <param name="value">The string.</param>
    /// <returns>the composing CompositePredicate.</returns>
    IPredicate AddLike(RoleType role, string value);

    /// <summary>
    ///     Adds a CompositePredicate that evaluates to true if its composed predicate evaluates to false.
    ///     This predicate is ignored when there are no composed predicates.
    /// </summary>
    /// <returns>the newly added CompositePredicate.</returns>
    ICompositePredicate AddNot(Action<ICompositePredicate> init = null);

    /// <summary>
    ///     Adds a CompositePredicate that evaluates to true if any of its composed predicates evaluate to true.
    ///     This predicate is ignored when there are no composed predicates.
    /// </summary>
    /// <returns>the newly added CompositePredicate.</returns>
    ICompositePredicate AddOr(Action<ICompositePredicate> init = null);
}
