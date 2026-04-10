using Markdig;
using Microsoft.AspNetCore.Components;

namespace BlazorChat;

internal static class ChatMarkdown
{
    /// <summary>
    /// Renders chat markdown to HTML: GFM-style features, soft line breaks as &lt;br/&gt;, raw HTML disabled (mitigates XSS for user and model text).
    /// </summary>
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseSoftlineBreakAsHardlineBreak()
        .DisableHtml()
        .Build();

    public static MarkupString ToHtml(string? markdown) =>
        new(Markdown.ToHtml(markdown ?? string.Empty, Pipeline));
}
