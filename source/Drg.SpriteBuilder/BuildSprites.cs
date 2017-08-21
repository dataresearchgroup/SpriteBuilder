using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Drg.SpriteBuilder
{
    public class BuildSprites : Task
    {
        public string ContentPath { get; set; }

        [SecurityCritical]
        public override bool Execute()
        {
            this.Log.LogMessage(MessageImportance.High, "BuildSprites task started", new object[] { });
            string iconsPath = Path.Combine(ContentPath, "images\\icons");
            string cssPath = Path.Combine(ContentPath, "css");
            StringBuilder css = new StringBuilder();

            Dictionary<int, string> fileHashes = new Dictionary<int, string>();

            foreach (string sizeDir in Directory.GetDirectories(iconsPath).Where(f=>!f.EndsWith("_sgbak")))
            {
                int size = int.Parse(sizeDir.Split('\\').Last());

                var files = Directory.GetFiles(Path.Combine(iconsPath, sizeDir))
                    .Where(f => !f.EndsWith("thumbs.db", StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(f => f).ToList();

                var img = new Bitmap(size, size * files.Count);

                using (var g = Graphics.FromImage(img))
                {
                    int idx = 0;
                    foreach (var file in files)
                    {
                        try
                        {
                            var srcImage = Image.FromFile(file);
                            g.DrawImage(srcImage, 0, idx * size, size, size);

                            css.AppendFormat(".icon{0}.{1} {{background-position:0px -{2}px;}} ",
                                size,
                                Path.GetFileNameWithoutExtension(file),
                                idx * size);

                            css.AppendFormat(".icon{0}.{1} img {{top:-{2}px;}} ",
                                size,
                                Path.GetFileNameWithoutExtension(file),
                                idx * size);

                            idx++;
                        }
                        catch (OutOfMemoryException)
                        { 
                            // skip the file since it isn't an image
                        }
                    }
                }

                string fileName = Path.Combine(iconsPath, size + ".png");

                try
                {
                    img.Save(fileName, ImageFormat.Png);
                }
                catch
                {
                    this.Log.LogMessage(MessageImportance.High, "Could not write " + fileName, new object[] { });
                    //probably read-only (checked in). do nothing.
                }

                string v = string.Join("|", files.ToArray()).GetHashCode().ToString();
                css.AppendFormat(".icon{0} {{background-image:url(../images/icons/{0}.png?v={1});width:{0}px;height:{0}px;}} ", size, v);

                fileHashes.Add(size, v);
            }

            // store the css file
            string cssFileName = Path.Combine(cssPath, "icons.css");
            try
            {
                File.WriteAllText(cssFileName, css.ToString());
            }
            catch
            {
                this.Log.LogMessage(MessageImportance.High, "Could not write " + cssFileName, new object[] { });
                //probably read-only (checked in). do nothing.
            }

            // store the file uniqueness hashes in a separate file (allows the calling app to access the calculated hashes)
            string hashesFilename = Path.Combine(iconsPath, "hashes.json");
            try
            {
                File.WriteAllText(hashesFilename, "{ " + string.Join(", ", fileHashes.Select(x => "\"" + x.Key.ToString() + "\": \"" + x.Value + "\"").ToArray()) + " }");
                this.Log.LogMessage(MessageImportance.High, "BuildSprites hashes file created at " + hashesFilename, new object[] { });
            }
            catch
            {
                this.Log.LogMessage(MessageImportance.High, "Could not write " + hashesFilename, new object[] { });
            }

            this.Log.LogMessage(MessageImportance.High, "BuildSprites task completed", new object[] { });
            return true;
        } 
    }
}
