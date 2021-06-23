using server.Data.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace server.Data
{
    public static class Fix
    {
        public async static Task<string> FixCorruptXmlAsync(byte[] file, CorruptionType corruptionType, bool ff_KeyTracks)
        {
            byte[] unzip;

            try
            {
                unzip = await ZipTool.DecompressAsync(file);
            }
            catch (UnsupportedCompressionAlgorithmException)
            {
                throw;
            }

            if (unzip is byte[] bytes && bytes.Length > 0)
            {
                var xmlDoc = XDocument.Parse(Encoding.UTF8.GetString(unzip));

                if (corruptionType == CorruptionType.DUPLICATE_NOTE_IDS)
                {
                    //saved here for debugging purposes
                    var dupes = RunDuplicateNoteIdsAlgorithm(xmlDoc, ff_KeyTracks);
                }

                return await SaveXmlAsync(xmlDoc);
            }

            throw new InvalidDataException("file");
        }

        private static async Task<string> SaveXmlAsync(XDocument xml)
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

        private static IReadOnlyList<XElement> RunDuplicateNoteIdsAlgorithm(XDocument root, bool ff_KeyTracks)
        {
            var dupes = new List<XElement>();

            var nodes = root.Descendants()
                .Where((d) => d.Attributes().Any((a) => a.Name == "NoteId"));

            foreach (var found in nodes)
            {
                var attrs = found.Attributes();

                if (found.Parent.Elements().Count() > 1)
                {
                    foreach (var sibling in found.Parent.Elements())
                    {
                        if (sibling == found) continue;

                        if (sibling.Name != found.Name) continue;

                        if (sibling.Attributes()
                            .Where((a) => a.Name == "NoteId")
                            .Any((a) => attrs
                                .Any((b) => b.Name == a.Name && b.Value == a.Value)))
                        {
                            dupes.Add(sibling);
                            sibling.Remove();
                        }
                    }
                }
            }

            if (ff_KeyTracks)
            {
                dupes.AddRange(TryKeyTracks(root));
            }

            return dupes;
        }

        private static IReadOnlyList<XElement> TryKeyTracks(XDocument root)
        {
            var dupes = new List<XElement>();

            var nodes = root.Descendants().Where((d) => d.Name == "Notes")
                    .Elements().Where((d) => d.Name == "KeyTracks");

            var fixer = 99990;

            foreach (var kt in nodes)
            {
                var midinoteevents = kt.Elements().Where((k) => k.Name == "KeyTrack")
                    .Elements().Where((t) => t.Name == "Notes")
                    .Elements().Where((n) => n.Name == "MidiNoteEvent");

                foreach (var mn in midinoteevents)
                {
                    var attr = mn.Attributes().First((a) => a.Name == "NoteId");
                    dupes.Add(mn);

                    int oldId = int.Parse(attr.Value);

                    attr.Value = fixer++.ToString();
                }
            }

            return dupes;
        }
    }
}