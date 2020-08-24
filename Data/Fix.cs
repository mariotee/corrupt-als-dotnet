using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BlazorInputFile;
using server.Data.Util;

namespace server.Data
{
    public class Fix
    {
        public async static Task<string> FixCorruptXmlAsync(MemoryStream file)
        {
            if(file is null)
            {
                throw new System.ArgumentException("no inmem");
            }

            file.Seek(0, SeekOrigin.Begin);
            using var sr = new StreamReader(file);

            var fileString = await sr.ReadToEndAsync();
            
            var zipped = await ZipTool.ZipString(fileString);
            var unzipped = await ZipTool.UnzipBytes(zipped);

            if (!string.IsNullOrWhiteSpace(unzipped)){
                var xmlDoc = new XmlDocument();
                
                xmlDoc.LoadXml(unzipped);

                var dupes = RunAlgorithm(xmlDoc.DocumentElement);
                Console.WriteLine(dupes.Count);

                using var stringWriter = new StringWriter();
                using var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings(){
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
            var i = 0;
            var dupes = new List<XmlNode>();

            var nodes = node.SelectNodes("//descendant::*[attribute::*[contains(name(), 'id') or contains(name(), 'Id')]]");
            Console.WriteLine("total: " + nodes.Count);

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

                if (i == 0){
                    Console.WriteLine(val);
                    ++i;
                }

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