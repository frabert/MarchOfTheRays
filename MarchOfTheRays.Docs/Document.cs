using System.IO;
using System.Text;
using System.Xml;

namespace MarchOfTheRays.Docs
{
    public class Document
    {
        XmlDocument doc = new XmlDocument();

        public Document(string file)
        {
            doc.Load(file);
        }

        public Document(Stream stream)
        {
            doc.Load(stream);
        }

        public Document(TextReader reader)
        {
            doc.Load(reader);
        }

        static void NodeToRtf(XmlNode node, StringBuilder builder)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var child = node.ChildNodes[i];
                switch (child.Name)
                {
                    case "#text":
                        {
                            string text = child.InnerText.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
                            if (i != 0 && char.IsWhiteSpace(text[0])) builder.Append(" ");
                            builder.Append(child.InnerText.Trim());
                        }
                        break;
                    case "br":
                        {
                            builder.Append(@"\line ");
                        }
                        break;
                    case "h1":
                        {
                            builder.Append(@"{\fs40 ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "h2":
                        {
                            builder.Append(@"{\fs30 ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "h3":
                        {
                            builder.Append(@"{\fs25 ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "i":
                        {
                            builder.Append(@"{\i ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "b":
                        {
                            builder.Append(@"{\b ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "ul":
                        {
                            builder.Append(@"{\ul ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "sub":
                        {
                            builder.Append(@"{\sub ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "sup":
                        {
                            builder.Append(@"{\sup ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "strike":
                        {
                            builder.Append(@"{\strike ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "scaps":
                        {
                            builder.Append(@"{\scaps ");
                            NodeToRtf(child, builder);
                            builder.Append("}");
                        }
                        break;
                    case "link":
                        {
                            var target = child.Attributes["target"];
                            if (target == null) throw new InvalidDataException();
                            builder.Append(@"{{\field{\*\fldinst {HYPERLINK ");
                            builder.Append('"');
                            builder.Append(target.Value);
                            builder.Append('"');
                            builder.Append(@" }}{\fldrslt {");
                            NodeToRtf(child, builder);
                            builder.Append("}}}}");
                        }
                        break;
                }
            }
        }

        public string ToRtf()
        {
            var builder = new StringBuilder();
            builder.Append(@"{\rtf1 ");
            if (doc.LastChild.Name != "document") throw new InvalidDataException();
            for (int i = 0; i < doc.LastChild.ChildNodes.Count; i++)
            {
                var child = doc.LastChild.ChildNodes[i];
                if (child.Name != "p") throw new InvalidDataException();
                builder.Append(@"{\pard ");
                var align = child.Attributes["align"];
                if (align != null)
                {
                    if (align.Value == "left")
                    {
                        builder.Append(@"\ql ");
                    }
                    else if (align.Value == "right")
                    {
                        builder.Append(@"\qr ");
                    }
                    else if (align.Value == "center")
                    {
                        builder.Append(@"\qc ");
                    }
                }
                NodeToRtf(child, builder);
                builder.Append(@"\line \par}");
            }
            builder.Append("}");
            return builder.ToString();
        }
    }
}
