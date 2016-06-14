// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2226: Operators should have symmetrical overloads
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class OperatorsShouldHaveSymmetricalOverloadsFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OperatorsShouldHaveSymmetricalOverloadsAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    MicrosoftApiDesignGuidelinesAnalyzersResources.Generate_missing_operators,
                    c => CreateChangedDocument(context, c),
                    nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.Generate_missing_operators)),
                context.Diagnostics);
            return Task.FromResult(true);
        }

        private async Task<Document> CreateChangedDocument(
            CodeFixContext context, CancellationToken cancellationToken)
        {
            var document = context.Document;
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var semanticModel = editor.SemanticModel;
            var root = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var operatorNode = root.FindNode(context.Diagnostics.First().Location.SourceSpan);

            var containingOperator = (IMethodSymbol)semanticModel.GetDeclaredSymbol(operatorNode, cancellationToken);

            Debug.Assert(containingOperator.IsUserDefinedOperator());

            var newOperator = GetSymmetricalOperatorDeclaration(editor.Generator, containingOperator, operatorNode, semanticModel);

            operatorNode = operatorNode.AncestorsAndSelf().First(a => a.RawKind == newOperator.RawKind);

            editor.InsertAfter(operatorNode, newOperator);
            return editor.GetChangedDocument();
        }

        private SyntaxNode GetSymmetricalOperatorDeclaration(SyntaxGenerator generator, IMethodSymbol containingOperator,
            SyntaxNode operatorNode, SemanticModel semanticModel)
        {
            SyntaxNode[] parameterNames = containingOperator.GetParameters().Select(p => generator.IdentifierName(p.Name)).ToArray();
            Accessibility accessibility = containingOperator.DeclaredAccessibility;

            switch (containingOperator.Name)
            {
                case WellKnownMemberNames.EqualityOperatorName:
                    return generator.OperatorInequalityDeclaration(containingOperator.ContainingType, parameterNames[0], parameterNames[1], accessibility);

                case WellKnownMemberNames.InequalityOperatorName:
                    return generator.OperatorEqualityDeclaration(containingOperator.ContainingType, parameterNames[0], parameterNames[1], accessibility);

                case WellKnownMemberNames.LessThanOperatorName:
                    return TypeImplementsComparableInterface(containingOperator.ContainingType, semanticModel.Compilation)
                        ? generator.OperatorGreaterThanDeclaration(containingOperator.ContainingType, parameterNames[0], parameterNames[1], accessibility)
                        : GetThrowingOperatorImplementation(OperatorKind.GreaterThan, containingOperator, operatorNode, generator, semanticModel);

                case WellKnownMemberNames.LessThanOrEqualOperatorName:
                    return TypeImplementsComparableInterface(containingOperator.ContainingType, semanticModel.Compilation)
                        ? generator.OperatorGreaterThanOrEqualDeclaration(containingOperator.ContainingType, parameterNames[0], parameterNames[1], accessibility)
                        : GetThrowingOperatorImplementation(OperatorKind.GreaterThanOrEqual, containingOperator, operatorNode, generator, semanticModel);

                case WellKnownMemberNames.GreaterThanOperatorName:
                    return TypeImplementsComparableInterface(containingOperator.ContainingType, semanticModel.Compilation)
                        ? generator.OperatorLessThanDeclaration(containingOperator.ContainingType, parameterNames[0], parameterNames[1], accessibility)
                        : GetThrowingOperatorImplementation(OperatorKind.LessThan, containingOperator, operatorNode, generator, semanticModel);

                case WellKnownMemberNames.GreaterThanOrEqualOperatorName:
                    return TypeImplementsComparableInterface(containingOperator.ContainingType, semanticModel.Compilation)
                        ? generator.OperatorLessThanOrEqualDeclaration(containingOperator.ContainingType, parameterNames[0], parameterNames[1], accessibility)
                        : GetThrowingOperatorImplementation(OperatorKind.LessThanOrEqual, containingOperator, operatorNode, generator, semanticModel);
            }

            throw new NotSupportedException("Unknown comparison operator.");
        }

        private bool TypeImplementsComparableInterface(INamedTypeSymbol typeSymbol, Compilation compilation)
        {
            INamedTypeSymbol comparableType = WellKnownTypes.IComparable(compilation);
            INamedTypeSymbol genericComparableType = WellKnownTypes.GenericIComparable(compilation);

            return typeSymbol.AllInterfaces.Any(t => t.Equals(comparableType) || t.Equals(genericComparableType));
        }

        private SyntaxNode GetThrowingOperatorImplementation(OperatorKind operatorKind, IMethodSymbol containingOperator, SyntaxNode operatorNode, SyntaxGenerator generator, SemanticModel semanticModel)
        {
            return generator.OperatorDeclaration(
                operatorKind,
                containingOperator.GetParameters().Select(p => generator.ParameterDeclaration(p)),
                generator.TypeExpression(containingOperator.ReturnType),
                containingOperator.DeclaredAccessibility,
                generator.GetModifiers(operatorNode),
                new[]
                {
                    generator.DefaultMethodStatement(semanticModel.Compilation)
                });
        }
    }
}