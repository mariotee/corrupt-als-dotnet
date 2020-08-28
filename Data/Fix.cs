using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace server.Data
{
    public class Fix
    {
        public async static Task<string> FixCorruptXmlAsync(string file)
        {
            //TODO: work on taking in .als and compress/decompress from there

            if (!string.IsNullOrWhiteSpace(file))
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(file);

                //saved here for debugging purposes
                var dupes = RunAlgorithm(xmlDoc.DocumentElement);

                using var stringWriter = new StringWriter();
                using var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings()
                {
                    Async = true,
                });

                xmlDoc.Save(xmlTextWriter);

                await xmlTextWriter.FlushAsync();

                return stringWriter.GetStringBuilder().ToString();
            }

            throw new InvalidDataException("file");
        }

        private static IReadOnlyList<XmlNode> RunAlgorithm(XmlNode node)
        {
            var dupes = new List<XmlNode>();

            var nodes = node.SelectNodes("//descendant::*[attribute::*[contains(name(), 'id') or contains(name(), 'Id')]]");

            foreach (XmlNode found in nodes)
            {
                var foundAttributes = found.Attributes;
                var filteredFoundAttr = new List<XmlAttribute>();
                foreach (XmlAttribute attr in foundAttributes)
                {
                    if (attr.Name.Contains("id", StringComparison.CurrentCultureIgnoreCase))
                    {
                        filteredFoundAttr.Add(attr);
                    }
                }

                if (filteredFoundAttr.Count > 1)
                {
                    throw new ArgumentException("too many IDs");
                }

                var val = filteredFoundAttr.FirstOrDefault()?.Value ?? "";

                foreach (XmlNode sibling in found.ParentNode?.ChildNodes ?? new XmlDocument().SelectNodes("/"))
                {
                    if (sibling == found) continue;

                    if (sibling.Name != found.Name) continue;

                    var siblingAttributes = sibling.Attributes;
                    var idAttributes = new List<XmlAttribute>();

                    foreach (XmlAttribute attr in siblingAttributes)
                    {
                        if (attr.Name.Contains("id", StringComparison.CurrentCultureIgnoreCase))
                        {
                            idAttributes.Add(attr);
                        }
                    }

                    if (idAttributes.Count > 1)
                    {
                        throw new ArgumentException("too many IDs on sibling");
                    }

                    if (idAttributes.FirstOrDefault()?.Value == val)
                    {
                        dupes.Add(sibling);
                        found.ParentNode.RemoveChild(sibling);
                    }
                }
            }

            return dupes;
        }
    }
}