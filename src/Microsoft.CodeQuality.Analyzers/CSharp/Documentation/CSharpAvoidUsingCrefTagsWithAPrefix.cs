// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using XmlDocumentationComments.Analyzers;

namespace XmlDocumentationComments.CSharp.Analyzers
{
    /// <summary>
    /// RS0010: Avoid using cref tags with a prefix
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAvoidUsingCrefTagsWithAPrefixAnalyzer : AvoidUsingCrefTagsWithAPrefixAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeXmlAttribute, SyntaxKind.XmlTextAttribute);
        }

        private static void AnalyzeXmlAttribute(SyntaxNodeAnalysisContext context)
        {
            var textAttribute = (XmlTextAttributeSyntax)context.Node;

            if (textAttribute.Name.LocalName.Text == "cref")
            {
                ProcessAttribute(context, textAttribute.TextTokens);
            }
        }
    }
}