// using System;
// using System.Linq;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
//
// namespace PipeLoom.Analyzers;
//
// // [Generator(LanguageNames.CSharp)]
// public class TypedValueGenerator: IIncrementalGenerator
// {
//     public void Initialize(IncrementalGeneratorInitializationContext context)
//     {
//         var typeTarget = context.SyntaxProvider.ForAttributeWithMetadataName(
//             "PipeLoom.Types.Abstractions.PlType",
//             static (node, _) => node is StructDeclarationSyntax sds && sds.Modifiers.Any(d => d.IsKind(SyntaxKind.PartialKeyword)),
//             static (context, _) => context.TargetSymbol
//         );
//         
//         context.RegisterSourceOutput(
//             typeTarget,
//             static (context, targetSymbol) =>
//             {
//                 if (targetSymbol is not INamedTypeSymbol nts || String.IsNullOrWhiteSpace(nts.Name))
//                 {
//                     return;
//                 }
//                 
//                 var genName = $"{nts.Name}.pltype.g.cs";
//                 context.AddSource(genName, Generate(nts));
//             }
//             );
//     }
//
//     private static string Generate(INamedTypeSymbol typeSymbol)
//     {
//         return String.Empty;
//         
//         // typeSymbol.AllInterfaces
//         //     .Where(candidate => candidate.Name != "PlType"
//         //                         && String.Equals(
//         //                             "PipeLoom.Types.Abstractions."
//         //                             candidate.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
//         //                         && candidate.AllInterfaces.Any(d => String.Equals()))
//         // throw new NotImplementedException();
//     }
//     
//     
// }