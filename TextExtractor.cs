using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;

namespace HtmlToText {
    public class TextExtractor {
        private HashSet<string> FormElementNames = new HashSet<string> { "label", "textarea", "button", "option", "select" };
        private HashSet<string> ElementNamesThatImplyNewLine = new HashSet<string> { "li", "div", "ul", "p", "h1", "h2", "h3", "h4" };

        public void ExtractTextAndWrite(IEnumerable<HtmlNode> nodes, TextWriter writer) {
            foreach (var node in nodes) {
                ExtractTextAndWrite(node, writer);
            }
        }

        private bool ExtractTextAndWrite(HtmlNode node, TextWriter writer) {
            if (node.NodeType == HtmlNodeType.Comment)
                return false;

            if (node.NodeType == HtmlNodeType.Text) {
                var text = node.InnerText;
                text = HtmlEntity.DeEntitize(text);
                text = Regex.Replace(text.Trim(), @"\s+", " ");

                writer.Write(text);
                return true;
            }

            if (node.Name == "script" || node.Name == "style" || FormElementNames.Contains(node.Name))
                return false;

            if (node.Name == "a") {
                var siblings = node.ParentNode.ChildNodes;
                if (!siblings.Any(n => n.Name != "a" && !string.IsNullOrEmpty(node.InnerText)))
                    return false;
            }

            var written = false;
            foreach (var child in node.ChildNodes) {
                written = ExtractTextAndWrite(child, writer);
            }

            if (written && ElementNamesThatImplyNewLine.Contains(node.Name))
                writer.WriteLine();

            return written;
        }
    }
}
