using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UwinOpenDMT.Properties;

namespace UwinOpenDMT
{
    public partial class UwinOpenDMT : Form
    {
        #region Variables

        private String currentOpenDocumentPath = String.Empty;
        private bool newDocument = false;

        #endregion Variables

        #region Constructors

        public UwinOpenDMT()
        {
            InitializeComponent();
            enableCtrl(false); // Disable control since no xml doc is loaded
            this.Visible = true; // Make the form visible
        }

        #endregion Constructors

        #region Functions and Methods

        private void enableCtrl(bool status)
        {
            readOnly.Enabled = status;
            readOnlyToolStripMenuItem.Enabled = status;
            saveCurrent.Enabled = !readOnly.Checked;
        }

        private void openFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = Resources.supportedfiles_opendialogExtension;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (Regex.IsMatch(Path.GetExtension(openFileDialog.FileName)?.ToLower(), @"^.*\.(xml|xlsx|xls|csv)$"))
                    {
                        Stream openedDocument = null;
                        try
                        {
                            // Load the contents of the file into dataGrid
                            openedDocument = openFileDialog.OpenFile();
                            dataGrid.DataSource = (Path.GetExtension(openFileDialog.FileName)?.ToLower() == ".xml")
                                ? ImpExpUtilities.getXmlData(openedDocument).Tables[0] // if it's an xml document
                                : ImpExpUtilities.getSpreadSheetData(openedDocument as FileStream).Tables[0]; // else if (xlsx|xls|csv)
                            enableCtrl(true);

                            currentOpenDocumentPath = openFileDialog.FileName;
                            this.Text = Resources.title + '[' + openFileDialog.SafeFileName + ']'; // Change the Forms title to currentOpenDoc #10
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(Resources.OpenDoc_fail_msg, Resources.fail,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            openedDocument?.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.OpenDoc_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void saveFile()
        {
            if (currentOpenDocumentPath != String.Empty)
            {
                if (!readOnly.Checked && !newDocument)
                {
                    var fileSuccessfullyExported = false;
                    switch (Path.GetExtension(currentOpenDocumentPath)?.ToLower())
                    {
                        // Save the file depending on the chosen extension, !Export -> if export fails, return instead is to display err msg only
                        case ".xml":
                            fileSuccessfullyExported = ImpExpUtilities.ExportXML(dataGrid.DataSource as DataTable, currentOpenDocumentPath);
                            break;

                        case ".xlsx":
                            fileSuccessfullyExported = ImpExpUtilities.ExportXLS(dataGrid.DataSource as DataTable, currentOpenDocumentPath);
                            break;

                        case ".csv":
                            fileSuccessfullyExported = ImpExpUtilities.ExportCSV(dataGrid, currentOpenDocumentPath);
                            break;
                    }
                    if (fileSuccessfullyExported)
                        MessageBox.Show(Resources.saveCurrent_success + currentOpenDocumentPath, Resources.success, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBox.Show(string.Format(Resources.saveFile_fail_msg, currentOpenDocumentPath), Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (readOnly.Checked && !newDocument)
                {
                    MessageBox.Show(Resources.saveXml_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (newDocument || currentOpenDocumentPath == String.Empty)
            {
                exportFile();
            }
        }

        private void exportFile()
        {
            if (dataGrid.ColumnCount > 0)
            {
                using (var fileDialog = new SaveFileDialog())
                {
                    fileDialog.Filter = Resources.supportedFiles_extension;
                    fileDialog.FilterIndex = 1;
                    fileDialog.RestoreDirectory = true;

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var fileSuccessfullyExported = false;
                        switch (Path.GetExtension(fileDialog.FileName)?.ToLower())
                        {
                            // Export the file depending on the chosen extension, !Export -> if export fails, return instead is to display err msg only
                            case ".xml":
                                fileSuccessfullyExported = ImpExpUtilities.ExportXML(dataGrid.DataSource as DataTable, fileDialog.FileName);
                                break;

                            case ".xlsx":
                                fileSuccessfullyExported = ImpExpUtilities.ExportXLS(dataGrid.DataSource as DataTable, fileDialog.FileName);
                                break;

                            case ".csv":
                                fileSuccessfullyExported = ImpExpUtilities.ExportCSV(dataGrid, fileDialog.FileName);
                                break;
                        }

                        if (fileSuccessfullyExported)
                        {
                            currentOpenDocumentPath = fileDialog.FileName;
                            this.Text = Resources.title + '[' + Path.GetFileName(currentOpenDocumentPath) + ']'; // Change the Forms title to currentOpenDoc #10
                            MessageBox.Show(Resources.saveCurrent_success + currentOpenDocumentPath, Resources.success, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        else
                            MessageBox.Show(string.Format(Resources.saveFile_fail_msg, fileDialog.FileName), Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Resources.saveEmptyTable_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void exportXmlFile()
        {
            if (dataGrid.ColumnCount > 0)
            {
                using (var fileDialog = new SaveFileDialog())
                {
                    //openFileDialog.InitialDirectory = @"%HOMEPATH%";
                    fileDialog.Filter = Resources.xmlfile_dialogextension;
                    fileDialog.FilterIndex = 1;
                    fileDialog.RestoreDirectory = true;

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var ds = (DataTable)dataGrid.DataSource;
                        // Save the contents of the dataGrid into the chosen file
                        ImpExpUtilities.ExportXML(ds, fileDialog.FileName);
                        currentOpenDocumentPath = fileDialog.FileName; // Save to the exported file if future edits
                        newDocument = false; // Set flag to false since the document was being exported to the disk
                        this.Text = Resources.title + '[' + Path.GetFileName(fileDialog.FileName) + ']'; // Change the Forms title to currentOpenDoc #10
                        MessageBox.Show(Resources.saveCurrent_success + currentOpenDocumentPath, Resources.success, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
            else
            {
                MessageBox.Show(Resources.saveEmptyTable_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void exportXlsFile()
        {
            if (dataGrid.ColumnCount > 0)
            {
                using (var fileDialog = new SaveFileDialog())
                {
                    //openFileDialog.InitialDirectory = @"%HOMEPATH%";
                    fileDialog.Filter = Resources.xlsfile_dialogextension;
                    fileDialog.FilterIndex = 1;
                    fileDialog.RestoreDirectory = true;

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Save the contents of the dataGrid into the chosen file
                        if (ImpExpUtilities.ExportXLS(dataGrid.DataSource as DataTable, fileDialog.FileName))
                            MessageBox.Show(Resources.saveCurrent_success + fileDialog.FileName, Resources.success, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(Resources.saveCSV_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Resources.saveEmptyTable_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void exportCSVFile()
        {
            if (dataGrid.ColumnCount > 0)
            {
                using (var fileDialog = new SaveFileDialog())
                {
                    //openFileDialog.InitialDirectory = @"%HOMEPATH%";
                    fileDialog.Filter = Resources.csvfile_extension;
                    fileDialog.FilterIndex = 1;
                    fileDialog.RestoreDirectory = true;

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Save the contents of the dataGrid into the chosen file
                        if (ImpExpUtilities.ExportCSV(dataGrid, fileDialog.FileName))
                            MessageBox.Show(Resources.saveCurrent_success + fileDialog.FileName, Resources.success, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(Resources.saveCSV_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Resources.saveEmptyTable_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void openXlsFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = @"%HOMEPATH%";
                openFileDialog.Filter = Resources.xlsfile_dialogextension;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (Regex.IsMatch(Path.GetExtension(openFileDialog.FileName)?.ToLower(), @"^.*\.(xlsx|xls|csv)$"))
                    {
                        Stream xlsSheet = null;
                        try
                        {
                            // Load the contents of the file into dataGrid
                            xlsSheet = openFileDialog.OpenFile();
                            dataGrid.DataSource = ImpExpUtilities.getSpreadSheetData(xlsSheet as FileStream).Tables[0];
                            enableCtrl(true);
                            this.Text = Resources.title + '[' + openFileDialog.SafeFileName + ']'; // Change the Forms title to currentOpenDoc #10
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(Resources.OpenXlsSheet_fail_msg, Resources.fail,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            xlsSheet?.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.DragDrop_wrongExt_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        #endregion Functions and Methods

        #region Events Methods

        #region Form & Hotkeys

        private void UwinOpenDMT_Load(object sender, EventArgs e)
        {
        }

        private void UwinOpenDMT_FormClosing(object sender, FormClosingEventArgs e)
        {
            var exit = (e.CloseReason == CloseReason.UserClosing)
                ? MessageBox.Show(Resources.exit_msg, Resources.exit, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                : DialogResult.Yes;
            if (exit == DialogResult.Yes)
            {
                foreach (var form in OwnedForms)
                    form.Close();
            }
            else if (exit == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    saveCurrent.PerformClick();
                    return true;

                case Keys.Control | Keys.Alt | Keys.E:
                    exportToolStripMenuItem.PerformClick();
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        #endregion Form & Hotkeys

        #region Buttons & Controls

        private void readOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (newDocument || currentOpenDocumentPath != String.Empty)
            {
                saveCurrent.Enabled = !readOnly.Checked;
                dataGrid.ReadOnly = readOnly.Checked;
                dataGrid.AllowUserToDeleteRows = !readOnly.Checked;
            }
        }

        private void readOnly_CheckStateChanged(object sender, EventArgs e)
        {
            readOnlyToolStripMenuItem.CheckState = readOnly.CheckState;
        }

        private void saveCurrent_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        #endregion Buttons & Controls

        #region Menu

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile();
        }

        private void excelSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openXlsFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exit = MessageBox.Show(Resources.quit_msg, Resources.exit, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (exit == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.HideDropDown();
            exportFile();
        }

        private void toXMLDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportXmlFile();
        }

        private void toXLSXExcelSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportXlsFile();
        }

        private void toCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportCSVFile();
        }

        private void editToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            readOnly_CheckedChanged(sender, e);
        }

        private void readOnlyToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            readOnly.CheckState = readOnlyToolStripMenuItem.CheckState;
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/luv4ever2shail/UwinOpenData"); // Opens repo's homepage
        }

        #endregion Menu

        #region Drag&Drop Event

        private void xmlDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void xmlDataGrid_DragDrop(object sender, DragEventArgs e)
        {
            var droppedDocumentPath = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (droppedDocumentPath.Length > 1) MessageBox.Show(Resources.DragDrop_many_msg, Resources.warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (Regex.IsMatch(Path.GetExtension(droppedDocumentPath[0])?.ToLower(), @"^.*\.(xml|xlsx|xls|csv)$"))
            {
                FileStream droppedDocument = null;
                try
                {
                    // Load the contents of the file into dataGrid
                    droppedDocument = new FileStream(droppedDocumentPath[0], FileMode.Open, FileAccess.Read); // [0] => the first selected file
                    dataGrid.DataSource = (Path.GetExtension(droppedDocumentPath[0])?.ToLower() == ".xml")
                        ? ImpExpUtilities.getXmlData(droppedDocument).Tables[0] // if it's an xml document
                        : ImpExpUtilities.getSpreadSheetData(droppedDocument).Tables[0]; // else if (xlsx|xls|csv)
                    enableCtrl(true);
                    currentOpenDocumentPath = (Path.GetExtension(droppedDocumentPath[0])?.ToLower() == ".xml") ? droppedDocumentPath[0] : String.Empty; // Set the currentOpenPath variable to be used later for saving
                    this.Text = Resources.title + '[' + Path.GetFileName(droppedDocumentPath[0]) + ']'; // Change the Forms title to currentOpenDoc #10
                }
                catch (Exception)
                {
                    MessageBox.Show(Resources.OpenDoc_fail_msg, Resources.fail, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Fixes #29 file lock issue
                    droppedDocument?.Close();
                }
            }
            else
            {
                MessageBox.Show(Resources.DragDrop_wrongExt_msg, Resources.warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion Drag&Drop Event

        #endregion Events Methods
    }
}