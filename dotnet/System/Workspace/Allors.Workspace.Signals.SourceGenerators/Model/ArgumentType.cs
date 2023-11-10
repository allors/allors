﻿namespace Allors.Workspace.Mvvm.Generator;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class ArgumentType
{
    public ArgumentType(SemanticModel semanticModel, TypeSyntax typeSyntax)
    {
        this.TypeSyntax = typeSyntax;

        var nullableTypeSyntax = this.TypeSyntax as NullableTypeSyntax;
        var nullableTypeElementType = nullableTypeSyntax?.ElementType;

        this.IsNullable = nullableTypeElementType != null;

        this.NormalizedTypeSyntax = nullableTypeElementType ?? this.TypeSyntax;

        if (this.NormalizedTypeSyntax is GenericNameSyntax genericNameSyntax)
        {
            var argument = genericNameSyntax.TypeArgumentList.Arguments.First();
            this.NestedArgumentType = new ArgumentType(semanticModel, argument);
        }

        var typeSymbol = semanticModel.GetSymbolInfo(this.NormalizedTypeSyntax).Symbol as INamedTypeSymbol;
        this.ImplementedTypes = typeSymbol?.AllInterfaces.Prepend(typeSymbol).ToArray().Select(v => new ImplementedType(v)).ToArray() ?? Array.Empty<ImplementedType>();
    }

    public TypeSyntax TypeSyntax { get; }

    public bool IsNullable { get; }

    public TypeSyntax NormalizedTypeSyntax { get; }

    public ArgumentType? NestedArgumentType { get; }

    public ImplementedType[] ImplementedTypes { get; }

    public bool IsUnitRole => this.NormalizedName.StartsWith("IUnitRole<");

    public string NormalizedName => this.NormalizedTypeSyntax.ToString();

    public string FullName => this.TypeSyntax.ToFullString();
}