using server.Data.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace server.Data
{
    public static class Fix
    {
        public async static Task<string> FixCorruptXmlAsync(byte[] file, CorruptionType corruptionType)
        {
            byte[] zipbytes;

            try
            {
                zipbytes = await ZipTool.DecompressAsync(file);
            }
            catch (UnsupportedCompressionAlgorithmException)
            {
                throw;
            }

            if (zipbytes is byte[] bytes && bytes.Length > 0)
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(Encoding.UTF8.GetString(zipbytes));

                if (corruptionType == CorruptionType.DUPLICATE_IDS)
                {
                    //saved here for debugging purposes
                    var dupes = RunDuplicateIdsAlgorithm(xmlDoc.DocumentElement);
                }

                return await SaveXmlAsync(xmlDoc);
            }

            throw new InvalidDataException("file");
        }

        private static async Task<string> SaveXmlAsync(XmlDocument xml)
        {
            using var stringWriter = new StringWriter();
            using var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings()
            {
                Async = true,
            });

            xml.Save(xmlTextWriter);

            await xmlTextWriter.FlushAsync();

            return stringWriter.GetStringBuilder().ToString();
        }

        private static IReadOnlyList<XmlNode> RunDuplicateIdsAlgorithm(XmlNode node)
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