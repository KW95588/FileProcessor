using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileProcessor
{
    public partial class Form1 : Form
    {
        string fileName;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open File";
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
                MessageBox.Show(fileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string inputInvoiceNumber = InvoiceNumber.Text.Trim();
            MessageBox.Show("Invoice number: " + inputInvoiceNumber);
            //MessageBox.Show(fileName);
            //List<string> fileContentList = System.IO.File.ReadLines(fileName).Where(s => s.Trim() != string.Empty).ToList();
            List<string> fileContentList = new List<string>();

            if (!String.IsNullOrEmpty(fileName) && (this.HasDirectoryAccess(FileSystemRights.Read, fileName)))
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        using (TextReader tr = new StreamReader(new FileStream(fileName, FileMode.Open)))
                        {
                            string line;
                            while ((line = tr.ReadLine()) != null)
                            {
                                line = line.Trim();
                                if (line != string.Empty)
                                {
                                    fileContentList.Add(line);
                                }

                            }
                        }

                        List<FileIndexInfo> fileIndexInfo = new List<FileIndexInfo>();

                        int count = 0;
                        int fileStartLineIndex = 0;
                        int fileInvoiceLineIndex = 0;
                        int fileEndLineIndex = 0;
                        string invoiceNumber = "";
                        for (int i = 0; i < fileContentList.Count; i++)
                        {
                            string currentLine = fileContentList[i];
                            if (currentLine == "*")
                            {
                                fileStartLineIndex = i;
                                count++;
                            }
                            if (currentLine == "TAX INVOICE")
                            {
                                fileInvoiceLineIndex = i + 1;
                                invoiceNumber = fileContentList[fileInvoiceLineIndex];
                                count++;
                            }
                            if (currentLine == "&")
                            {
                                fileEndLineIndex = i;
                                count++;
                            }
                            if (count == 3)
                            {
                                FileIndexInfo fi = new FileIndexInfo()
                                {
                                    InvoiceNumber = invoiceNumber,
                                    FileStartLineIndex = fileStartLineIndex,
                                    FileInvoiceLineIndex = fileInvoiceLineIndex,
                                    FileEndLineIndex = fileEndLineIndex
                                };

                                fileIndexInfo.Add(fi);
                                count = 0;
                            }
                        }

                        var result = fileIndexInfo.Where(f => f.InvoiceNumber == inputInvoiceNumber).FirstOrDefault();

                        string directory = Path.GetDirectoryName(fileName);
                        int startIndex = result.FileStartLineIndex;
                        int endIndex = result.FileEndLineIndex;
                        int range = endIndex - startIndex + 1;
                        string invoiveNum = result.InvoiceNumber;
                        string newFilePath = directory + "\\" + invoiveNum + ".txt";
                        List<string> sublist = fileContentList.GetRange(startIndex, range);
                        //List<string> sublist = fileContentList.Skip(startIndex).Take(endIndex - startIndex);

                        using (TextWriter tw = new StreamWriter(newFilePath))
                        {
                            foreach (String s in sublist)
                                tw.WriteLine(s);
                        }

                        MessageBox.Show("The subtracted invoice has been saved to: " + newFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception message 1" + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Exception message 2");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid file");
            }
        }

        //https://stackoverflow.com/questions/9503884/best-way-to-handle-errors-when-opening-file
        private bool HasDirectoryAccess(FileSystemRights fileSystemRights, string directoryPath)
        {
            DirectorySecurity directorySecurity = Directory.GetAccessControl(directoryPath);

            foreach (FileSystemAccessRule rule in directorySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                if ((rule.FileSystemRights & fileSystemRights) != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class FileIndexInfo
    {
        public string InvoiceNumber { get; set; }

        public int FileStartLineIndex { get; set; }

        public int FileInvoiceLineIndex { get; set; }

        public int FileEndLineIndex { get; set; }
    }
}
