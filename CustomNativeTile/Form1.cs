using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MyTile
{
    public partial class Form1 : Form
    {
        List<String> startmenuEntries;
        public string tileName = string.Empty;

        //https://www.freeformatter.com/java-dotnet-escape.html#ad-output
        public string templateXML = "<Application xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n    <VisualElements\r\n        BackgroundColor=\"#000000\"\r\n        ShowNameOnSquare150x150Logo=\"off\"\r\n        ForegroundText=\"dark\"\r\n        Wide150x300Logo=\"tile.png\"\r\n        Wide150x310Logo=\"tile.png\"\r\n        Square300x300Logo=\"tile.png\"\r\n        Square310x310Logo=\"tile.png\"\r\n        Wide300x150Logo=\"tile.png\"\r\n        Wide310x150Logo=\"tile.png\"\r\n        Square150x150Logo=\"tile.png\"\r\n        Square70x70Logo=\"tile.png\"\r\n        />\r\n</Application>";

        public Form1()
        {
            InitializeComponent();
            startmenuEntries = new List<String>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //C:\ProgramData\Microsoft\Windows\Start Menu\Programs
            string[] potentialFolders = { Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + @"\Programs", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Programs" };
            foreach (string startmenufolder in potentialFolders)
            {
                foreach (string filename in Directory.GetFiles(startmenufolder, "*.lnk", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(filename) == ".lnk")
                    {
                        startmenuEntries.Add(filename);
                    }
                }
            }
            lstStartmenu.Items.AddRange(startmenuEntries.ToArray());
        }

        private void lstStartmenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            WshShell shell = new WshShell(); //Create a new WshShell Interface
            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(lstStartmenu.SelectedItem.ToString()); //Link the interface to our shortcut

            string targetExe = link.TargetPath;
            if (!System.IO.File.Exists(targetExe))
            {
                //the shortcut points to something invalid...
                targetExe = targetExe.Replace(" (x86)", "");
                if (!System.IO.File.Exists(targetExe))
                {
                    MessageBox.Show("Can not find the target this shortcut is pointing to. Please enter application target manually.");
                    return;
                }
            }

            txtTarget.Text = targetExe; //Show the target in a MessageBox using IWshShortcut

            String targetTemplate = getTemplatePath(targetExe);
            txtImageFile.Text = "";
            pictureBox1.ImageLocation = "";

            if (System.IO.File.Exists(targetTemplate))
            {
                //we already have a template here!
                string tilename = getTileNameFromXML(targetExe, targetTemplate);
                string smalltilename = getSmallTileNameFromXML(targetExe, targetTemplate);
                string tilePath = Path.GetDirectoryName(targetExe) + @"\" + tilename;
                string smalltilePath = Path.GetDirectoryName(targetExe) + @"\" + smalltilename;
                if (!tilename.Equals("") && System.IO.File.Exists(tilePath))
                {
                    pictureBox1.ImageLocation = tilePath;
                    txtImageFile.Text = tilePath;
                    //tileName = tilename;
                    //Uncomment above if you want the target name to absorb the other VisualElementsManifest.xml stuff.
                    //Honestly really annoying 'cause stuff like Chrome replaces the logo on every update (which is a lot).
                    tileName = getDefaultTilename(targetExe);
                }
                /* Unused for now. Checks the small tile of the program.
                if (!smalltilename.Equals("") && System.IO.File.Exists(smalltilePath))
                {
                    pictureBox2.ImageLocation = smalltilePath;
                    if (!tilePath.Equals(smalltilePath))
                    {
                        txtImageSmall.Text = smalltilePath;
                        txtSmallTilename.Text = smalltilename;
                    }
                }*/
            }
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            DialogResult userClickedOK = openFileDialog1.ShowDialog();

            if (userClickedOK != DialogResult.OK) return;

            if (System.IO.File.Exists(openFileDialog1.FileName))
            {
                txtImageFile.Text = openFileDialog1.FileName;
                pictureBox1.ImageLocation = txtImageFile.Text;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string sourceTile = txtImageFile.Text;
            if (!System.IO.File.Exists(sourceTile))
            {
                MessageBox.Show("No TileImage selected.");
                return;
            }
            string targetExe = txtTarget.Text;
            if (!System.IO.File.Exists(targetExe))
            {
                MessageBox.Show("No Target selected.");
                return;
            }

            //string sourceTileSmall = txtImageSmall.Text;
            /*
            if (sourceTileSmall.Length > 0)
            {
                if (!System.IO.File.Exists(sourceTileSmall))
                {
                    MessageBox.Show("Small Tile specified but does not exist.");
                    return;
                }
            }*/

            //                     Name of the placeholder image in the template.xml
            if (tileName.Equals("") || tileName.Equals("tile.png"))
            {
                tileName = getDefaultTilename(targetExe);
            }
            /*
            if (sourceTileSmall.Length > 0 && (txtSmallTilename.Text.Equals("") || txtSmallTilename.Text.Equals("tile-small.png")))
            {
                txtSmallTilename.Text = getDefaultSmalltileName(targetExe);
            }*/

            //copy the image.
            string targetTile = Path.GetDirectoryName(targetExe) + @"\" + tileName;
            if (!sourceTile.Equals(targetTile))
            {
                System.IO.File.Copy(sourceTile, targetTile, true);
            }
            /*
            if (sourceTileSmall.Length > 0)
            {
                //copy the image.
                string targetTileSmall = Path.GetDirectoryName(targetExe) + @"\" + txtSmallTilename.Text;
                if (!sourceTileSmall.Equals(targetTileSmall))
                    System.IO.File.Copy(sourceTileSmall, targetTileSmall, true);
            }*/

            //Old template copy method.
            /*string sourceTemplate = "template.xml";
            if (!System.IO.File.Exists(sourceTemplate))
            {
                MessageBox.Show("Our template xml is missing.");
                return;
            }
            string targetTemplate = getTemplatePath(targetExe);
            System.IO.File.Copy(sourceTemplate, targetTemplate, true);*/

            string targetTemplate = getTemplatePath(targetExe);
            System.IO.File.WriteAllText(targetTemplate, templateXML);

            //modify targetTemplate.
            XDocument targetXml = XDocument.Load(targetTemplate);
            XElement ve = targetXml.Element("Application").Element("VisualElements");
            //XmlNode ve = targetXml.SelectSingleNode("/Application/VisualElements");

            //show label?
            ve.Attribute("ShowNameOnSquare150x150Logo").Value = (checkShowlabel.Checked ? "on" : "off");
            ve.Attribute("ForegroundText").Value = (radioDark.Checked ? "dark" : "light");
            ve.Attribute("BackgroundColor").Value = txtBg.Text;
            ve.Attribute("Square150x150Logo").Value = tileName;
            ve.Attribute("Square70x70Logo").Value = tileName;
            /*if (sourceTileSmall.Length > 0)
            {
                ve.Attribute("Square70x70Logo").Value = txtSmallTilename.Text;
            }*/
            targetXml.Save(targetTemplate);


            string lnkFile = lstStartmenu.SelectedItem.ToString();
            refreshLnk(lnkFile);
        }

        private string getTemplatePath(String exePath)
        {
            string targetTemplate = Path.GetDirectoryName(exePath) + @"\" + Path.GetFileNameWithoutExtension(exePath) + @".visualelementsmanifest.xml";
            return targetTemplate;
        }

        private void refreshLnk(String lnkFile)
        {
            System.IO.File.SetCreationTime(lnkFile, DateTime.Now);
            System.IO.File.SetLastAccessTime(lnkFile, DateTime.Now);
            System.IO.File.SetLastWriteTime(lnkFile, DateTime.Now);

            lstStartmenu_SelectedIndexChanged(null, null);
        }

        private void txtBg_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBg_DoubleClick(object sender, EventArgs e)
        {
            DialogResult res = colorDialog1.ShowDialog();

            if (res != DialogResult.OK)
            {
                return;
            }

            txtBg.Text = System.Drawing.ColorTranslator.ToHtml(colorDialog1.Color);

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string targetExe = txtTarget.Text;
            if (!System.IO.File.Exists(targetExe))
            {
                MessageBox.Show("No Target selected.");
                return;
            }

            string targetTemplate = getTemplatePath(targetExe);
            //string targetTile = getTilePathFromXML(targetExe, targetTemplate);
            //string targetTileSmall = getSmallTilePathFromXML(targetExe, targetTemplate);

            System.IO.File.Delete(targetTemplate);
            //System.IO.File.Delete(targetTile);
            //System.IO.File.Delete(targetTileSmall);

            string lnkFile = lstStartmenu.SelectedItem.ToString();
            refreshLnk(lnkFile);
        }

        private String getDefaultSmalltileName(String targetExe)
        {
            return Path.GetFileNameWithoutExtension(targetExe) + "-customtile-small.png";
        }

        private string getDefaultTilename(String targetExe)
        {
            return Path.GetFileNameWithoutExtension(targetExe) + "-customtile.png";
        }

        private String getTileNameFromXML(String targetExe, String templatePath)
        {
            XDocument targetXml = XDocument.Load(templatePath);
            XElement ve = targetXml.Element("Application").Element("VisualElements");
            return ve.Attribute("Square150x150Logo").Value;
        }

        private String getSmallTileNameFromXML(String targetExe, String templatePath)
        {
            XDocument targetXml = XDocument.Load(templatePath);
            XElement ve = targetXml.Element("Application").Element("VisualElements");
            return ve.Attribute("Square70x70Logo").Value;
        }
        /*
        private String getTilePathFromXML(String targetExe, String templatePath)
        {
            String tilePathCandidate = Path.GetDirectoryName(targetExe) + @"\tile.png";
            if (System.IO.File.Exists(tilePathCandidate))
                return tilePathCandidate;
            return "";
        }*/
        /*
        private String getSmallTilePathFromXML(String targetExe, String templatePath)
        {
            String tilePathCandidate = Path.GetDirectoryName(targetExe) + @"\tile-small.png";
            if (System.IO.File.Exists(tilePathCandidate))
                return tilePathCandidate;
            return "";
        }*/

        /*
    private void btnSmallImage_Click(object sender, EventArgs e)
    {
        DialogResult userClickedOK = openFileDialog1.ShowDialog();

        if (userClickedOK != DialogResult.OK) return;

        if (System.IO.File.Exists(openFileDialog1.FileName))
        {
            txtImageSmall.Text = openFileDialog1.FileName;
        }
    }*/

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            string lnkFile = lstStartmenu.SelectedItem.ToString();
            if (System.IO.File.Exists(lnkFile))
                refreshLnk(lnkFile);
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            lstStartmenu.BeginUpdate();
            lstStartmenu.Items.Clear();

            lstStartmenu.Items.AddRange(startmenuEntries.Where(i => i.ToLower().Contains(txtFilter.Text.ToLower())).ToArray());

            lstStartmenu.EndUpdate();
        }

    }
}
