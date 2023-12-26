using ReverseMarkdown;
using System.IO;
using HtmlAgilityPack;
using static System.Net.WebRequestMethods;

namespace MarkdownReaderConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FolderBrowserDialog folderdialog = new FolderBrowserDialog();
            var config = new ReverseMarkdown.Config
            {
                // Include the unknown tag completely in the result (default as well)
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                // generate GitHub flavoured markdown, supported for BR, PRE and table tags
                GithubFlavored = true,
                // will ignore all comments
                RemoveComments = true,
                // remove markdown output for links where appropriate
                SmartHrefHandling = true
            };

            var converter = new ReverseMarkdown.Converter(config);
        }

        Converter converter = new ReverseMarkdown.Converter();

        string folderPath = "";
        string[] files;
        string selectedPath;
        string drivePath = @"C:\WIKIFILES\";
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
            {
                selectedPath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = selectedPath;
                files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                label2.Text = "Files found: " + files.Length.ToString();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool exists = System.IO.Directory.Exists(drivePath);
            if (!exists)
                Directory.CreateDirectory(drivePath);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            string subPath = "";
            string folderPath = "";
            
            foreach (var file in files)
            {
                // loading html string
                string htmlstr = System.IO.File.ReadAllText(file);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlstr);
                //find footer and remove 
                var body = doc.DocumentNode.SelectSingleNode("//body");
                var footer = doc.DocumentNode.SelectNodes("//section");
               foreach (var node in footer) { node.Remove(); }
                 //find the breadcrumbs child nodes and 
                if (doc.DocumentNode.SelectSingleNode("//ol") != null)
                {
                    var ol = doc.DocumentNode.SelectSingleNode("//ol");
                    var li = ol.Descendants("a").ToList();

                    //Get Breadcrumbs and create folders
                    for (int i = 0; i < li.Count; i++)
                    {
                        //replace space with hypen on names 
                        subPath = li[i].InnerText.Replace(" ", "-");
                        folderPath += subPath + @"\";
                        //Checks the folder name with breadcrumbs name and create it not exist
                        bool exists = System.IO.Directory.Exists(folderPath);
                        if (!exists)
                            Directory.CreateDirectory(drivePath + folderPath);
                    }
                }
                else {
                    //if theres no breadcrumbs then create a folder and put files in it 
                    folderPath = "NOBreadCrumbsFiles";
                    bool exists = System.IO.Directory.Exists(folderPath);
                    if (!exists)
                        Directory.CreateDirectory(drivePath + folderPath);
                }
               
                string[] pathsplit = file.Split('\\');
                string fileName = pathsplit[pathsplit.Length - 1].Split('.')[0];
                string[] nameID = fileName.Split("_");
                //Convert HTML to MD file 
                string markdown = converter.Convert(body.OuterHtml);
                System.IO.File.WriteAllText(drivePath + folderPath + @"\" + fileName + ".md", markdown);
                listBox1.Items.Add(file);
                subPath = "";
                folderPath = "";

            }
        }
    }
}