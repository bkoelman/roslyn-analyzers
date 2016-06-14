// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Analyzer.Utilities
{
    public static class SyntaxGeneratorExtensions
    {
        /// <summary>
        /// Creates a declaration for an operator equality overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <param name="leftParameterName">
        /// The optional identifier name of the left parameter.
        /// </param>
        /// <param name="rightParameterName">
        /// The optional identifier name of the right parameter.
        /// </param>
        /// <param name="accessibility">
        /// The optional operator method accessibility. Defaults to Public.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode OperatorEqualityDeclaration(this SyntaxGenerator generator, INamedTypeSymbol containingType,
            SyntaxNode leftParameterName = null, SyntaxNode rightParameterName = null, Accessibility accessibility = Accessibility.Public)
        {
            SyntaxNode leftArgument = leftParameterName ?? generator.IdentifierName("left");
            SyntaxNode rightArgument = rightParameterName ?? generator.IdentifierName("right");

            List<SyntaxNode> statements = new List<SyntaxNode>();

            if (containingType.TypeKind == TypeKind.Class)
            {
                statements.AddRange(new[]
                {
                    generator.IfStatement(
                        generator.InvocationExpression(
                            generator.IdentifierName("ReferenceEquals"),
                            leftArgument,
                            rightArgument),
                        new[]
                        {
                            generator.ReturnStatement(generator.TrueLiteralExpression())
                        }),
                    generator.IfStatement(
                        generator.InvocationExpression(
                            generator.IdentifierName("ReferenceEquals"),
                            leftArgument,
                            generator.NullLiteralExpression()),
                        new[]
                        {
                            generator.ReturnStatement(generator.FalseLiteralExpression())
                        })
                });
            }

            statements.Add(
                generator.ReturnStatement(
                    generator.InvocationExpression(
                        generator.MemberAccessExpression(
                            leftArgument, "Equals"),
                        rightArgument)));

            return generator.ComparisonOperatorDeclaration(OperatorKind.Equality, containingType, leftArgument, rightArgument, accessibility, statements.ToArray());
        }

        /// <summary>
        /// Creates a declaration for an operator inequality overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <param name="leftParameterName">
        /// The optional identifier name of the left parameter.
        /// </param>
        /// <param name="rightParameterName">
        /// The optional identifier name of the right parameter.
        /// </param>
        /// <param name="accessibility">
        /// The optional operator method accessibility. Defaults to Public.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode OperatorInequalityDeclaration(this SyntaxGenerator generator, INamedTypeSymbol containingType, 
            SyntaxNode leftParameterName = null, SyntaxNode rightParameterName = null, Accessibility accessibility = Accessibility.Public)
        {
            SyntaxNode leftArgument = leftParameterName ?? generator.IdentifierName("left");
            SyntaxNode rightArgument = rightParameterName ?? generator.IdentifierName("right");

            var returnStatement = generator.ReturnStatement(
                    generator.LogicalNotExpression(
                        generator.ValueEqualsExpression(
                            leftArgument,
                            rightArgument)));

            return generator.ComparisonOperatorDeclaration(OperatorKind.Inequality, containingType, leftArgument, rightArgument, accessibility, returnStatement);
        }

        /// <summary>
        /// Creates a declaration for an operator less than overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <param name="leftParameterName">
        /// The optional identifier name of the left parameter.
        /// </param>
        /// <param name="rightParameterName">
        /// The optional identifier name of the right parameter.
        /// </param>
        /// <param name="accessibility">
        /// The optional operator method accessibility. Defaults to Public.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode OperatorLessThanDeclaration(this SyntaxGenerator generator, INamedTypeSymbol containingType,
            SyntaxNode leftParameterName = null, SyntaxNode rightParameterName = null, Accessibility accessibility = Accessibility.Public)
        {
            SyntaxNode leftArgument = leftParameterName ?? generator.IdentifierName("left");
            SyntaxNode rightArgument = rightParameterName ?? generator.IdentifierName("right");

            SyntaxNode expression;

            if (containingType.TypeKind == TypeKind.Class)
            {
                expression =
                    generator.ConditionalExpression(
                        generator.InvocationExpression(
                            generator.IdentifierName("ReferenceEquals"),
                            leftArgument,
                            generator.NullLiteralExpression()),
                        generator.LogicalNotExpression(
                            generator.InvocationExpression(
                                generator.IdentifierName("ReferenceEquals"),
                                rightArgument,
                                generator.NullLiteralExpression())),
                        generator.LessThanExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                                rightArgument),
                            generator.LiteralExpression(0)));
            }
            else
            {
                expression =
                    generator.LessThanExpression(
                        generator.InvocationExpression(
                            generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                            rightArgument),
                        generator.LiteralExpression(0));
            }

            var returnStatement = generator.ReturnStatement(expression);
            return generator.ComparisonOperatorDeclaration(OperatorKind.LessThan, containingType, leftArgument, rightArgument, accessibility, returnStatement);
        }

        /// <summary>
        /// Creates a declaration for an operator less than or equal overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <param name="leftParameterName">
        /// The optional identifier name of the left parameter.
        /// </param>
        /// <param name="rightParameterName">
        /// The optional identifier name of the right parameter.
        /// </param>
        /// <param name="accessibility">
        /// The optional operator method accessibility. Defaults to Public.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode OperatorLessThanOrEqualDeclaration(this SyntaxGenerator generator, INamedTypeSymbol containingType,
            SyntaxNode leftParameterName = null, SyntaxNode rightParameterName = null, Accessibility accessibility = Accessibility.Public)
        {
            SyntaxNode leftArgument = leftParameterName ?? generator.IdentifierName("left");
            SyntaxNode rightArgument = rightParameterName ?? generator.IdentifierName("right");

            SyntaxNode expression;

            if (containingType.TypeKind == TypeKind.Class)
            {
                expression =
                    generator.LogicalOrExpression(
                        generator.InvocationExpression(
                            generator.IdentifierName("ReferenceEquals"),
                            leftArgument,
                            generator.NullLiteralExpression()),
                        generator.LessThanOrEqualExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                                rightArgument),
                            generator.LiteralExpression(0)));
            }
            else
            {
                expression =
                    generator.LessThanOrEqualExpression(
                        generator.InvocationExpression(
                            generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                            rightArgument),
                        generator.LiteralExpression(0));
            }

            var returnStatement = generator.ReturnStatement(expression);
            return generator.ComparisonOperatorDeclaration(OperatorKind.LessThanOrEqual, containingType, leftArgument, rightArgument, accessibility, returnStatement);
        }

        /// <summary>
        /// Creates a declaration for an operator greater than overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <param name="leftParameterName">
        /// The optional identifier name of the left parameter.
        /// </param>
        /// <param name="rightParameterName">
        /// The optional identifier name of the right parameter.
        /// </param>
        /// <param name="accessibility">
        /// The optional operator method accessibility. Defaults to Public.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode OperatorGreaterThanDeclaration(this SyntaxGenerator generator, INamedTypeSymbol containingType,
            SyntaxNode leftParameterName = null, SyntaxNode rightParameterName = null, Accessibility accessibility = Accessibility.Public)
        {
            SyntaxNode leftArgument = leftParameterName ?? generator.IdentifierName("left");
            SyntaxNode rightArgument = rightParameterName ?? generator.IdentifierName("right");

            SyntaxNode expression;

            if (containingType.TypeKind == TypeKind.Class)
            {
                expression =
                    generator.LogicalAndExpression(
                        generator.LogicalNotExpression(
                            generator.InvocationExpression(
                                generator.IdentifierName("ReferenceEquals"),
                                leftArgument,
                                generator.NullLiteralExpression())),
                        generator.GreaterThanExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                                rightArgument),
                            generator.LiteralExpression(0)));
            }
            else
            {
                expression =
                    generator.GreaterThanExpression(
                        generator.InvocationExpression(
                            generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                            rightArgument),
                        generator.LiteralExpression(0));
            }

            var returnStatement = generator.ReturnStatement(expression);
            return generator.ComparisonOperatorDeclaration(OperatorKind.GreaterThan, containingType, leftArgument, rightArgument, accessibility, returnStatement);
        }

        /// <summary>
        /// Creates a declaration for an operator greater than or equal overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <param name="leftParameterName">
        /// The optional identifier name of the left parameter.
        /// </param>
        /// <param name="rightParameterName">
        /// The optional identifier name of the right parameter.
        /// </param>
        /// <param name="accessibility">
        /// The optional operator method accessibility. Defaults to Public.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode OperatorGreaterThanOrEqualDeclaration(this SyntaxGenerator generator, INamedTypeSymbol containingType,
            SyntaxNode leftParameterName = null, SyntaxNode rightParameterName = null, Accessibility accessibility = Accessibility.Public)
        {
            SyntaxNode leftArgument = leftParameterName ?? generator.IdentifierName("left");
            SyntaxNode rightArgument = rightParameterName ?? generator.IdentifierName("right");

            SyntaxNode expression;

            if (containingType.TypeKind == TypeKind.Class)
            {
                expression =
                    generator.ConditionalExpression(
                            generator.InvocationExpression(
                                generator.IdentifierName("ReferenceEquals"),
                                leftArgument,
                                generator.NullLiteralExpression()),
                            generator.InvocationExpression(
                                generator.IdentifierName("ReferenceEquals"),
                                rightArgument,
                                generator.NullLiteralExpression()),
                        generator.GreaterThanOrEqualExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                                rightArgument),
                            generator.LiteralExpression(0)));
            }
            else
            {
                expression =
                    generator.GreaterThanOrEqualExpression(
                        generator.InvocationExpression(
                            generator.MemberAccessExpression(leftArgument, generator.IdentifierName("CompareTo")),
                            rightArgument),
                        generator.LiteralExpression(0));
            }

            var returnStatement = generator.ReturnStatement(expression);
            return generator.ComparisonOperatorDeclaration(OperatorKind.GreaterThanOrEqual, containingType, leftArgument, rightArgument, accessibility, returnStatement);
        }

        private static SyntaxNode ComparisonOperatorDeclaration(this SyntaxGenerator generator, OperatorKind operatorKind, 
            INamedTypeSymbol containingType, SyntaxNode leftParameterName, SyntaxNode rightParameterName, Accessibility accessibility, params SyntaxNode[] statements)
        {
            return generator.OperatorDeclaration(
                operatorKind,
                new[]
                {
                    generator.ParameterDeclaration(leftParameterName.ToString(), generator.TypeExpression(containingType)),
                    generator.ParameterDeclaration(rightParameterName.ToString(), generator.TypeExpression(containingType))
                },
                generator.TypeExpression(SpecialType.System_Boolean),
                accessibility,
                DeclarationModifiers.Static,
                statements);
        }

        /// <summary>
        /// Creates a declaration for an override of <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="compilation">The compilation</param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode EqualsOverrideDeclaration(this SyntaxGenerator generator, Compilation compilation)
        {
            return generator.MethodDeclaration(
                WellKnownMemberNames.ObjectEquals,
                new[]
                {
                    generator.ParameterDeclaration("obj", generator.TypeExpression(SpecialType.System_Object))
                },
                returnType: generator.TypeExpression(SpecialType.System_Boolean),
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Override,
                statements: generator.DefaultMethodBody(compilation));
        }

        /// <summary>
        /// Creates a declaration for an override of <see cref="object.GetHashCode()"/>.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="compilation">The compilation</param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode GetHashCodeOverrideDeclaration(
            this SyntaxGenerator generator, Compilation compilation)
        {
            return generator.MethodDeclaration(
                WellKnownMemberNames.ObjectGetHashCode,
                returnType: generator.TypeExpression(SpecialType.System_Int32),
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Override,
                statements: generator.DefaultMethodBody(compilation));
        }

        /// <summary>
        /// Creates a default set of statements to place within a generated method body.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the statements.
        /// </param>
        /// <param name="compilation">The compilation</param>
        /// <returns>
        /// An sequence containing a single statement that throws <see cref="System.NotImplementedException"/>.
        /// </returns>
        public static IEnumerable<SyntaxNode> DefaultMethodBody(
            this SyntaxGenerator generator, Compilation compilation)
        {
            yield return DefaultMethodStatement(generator, compilation);
        }

        public static SyntaxNode DefaultMethodStatement(this SyntaxGenerator generator, Compilation compilation)
        {
            return generator.ThrowStatement(generator.ObjectCreationExpression(
                generator.TypeExpression(
                    compilation.GetTypeByMetadataName("System.NotImplementedException"))));
        }
    }
}