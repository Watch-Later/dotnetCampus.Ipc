﻿using System.Collections.Immutable;

using dotnetCampus.Ipc.DiagnosticAnalyzers.Compiling;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace dotnetCampus.Ipc.DiagnosticAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DefaultReturnDependsOnIgnoresIpcExceptionAnalyzer : DiagnosticAnalyzer
{
    public DefaultReturnDependsOnIgnoresIpcExceptionAnalyzer()
    {
        SupportedDiagnostics = ImmutableArray.Create(DIPC102_DefaultReturnDependsOnIgnoresIpcException);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeTypeIpcAttributes, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeTypeIpcAttributes(SyntaxNodeAnalysisContext context)
    {
        var classDeclarationNode = (ClassDeclarationSyntax) context.Node;
        var memberSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationNode);
        if (memberSymbol is null)
        {
            return;
        }

        foreach (var (attribute, namedValues) in IpcAttributeHelper.TryFindMemberAttributes(context.SemanticModel, classDeclarationNode))
        {
            // 设置了默认值却没有忽略异常。
            if (!namedValues.IgnoresIpcException && namedValues.DefaultReturn is not null)
            {
                if (attribute?.ArgumentList?.Arguments.FirstOrDefault(x =>
                    x.NameEquals?.Name.ToString() == nameof(IpcMemberAttribute.DefaultReturn)) is { } argument)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DIPC102_DefaultReturnDependsOnIgnoresIpcException, argument.GetLocation()));
                }
            }
        }
    }
}
