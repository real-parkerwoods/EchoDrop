using System.Diagnostics.Metrics;
using System.DirectoryServices.ActiveDirectory;
using System.Formats.Tar;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static EchoDrop.FileBlock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Compression;

namespace EchoDrop
{
    public partial class MainED : Form
    {
        //Runtime Vars
        bool logFilePathChanged = false;
        List<FileBlock>? loadedFileBlocks = null;

        readonly string echoDropOutDirectory = "EchoDrop Output";
        static string outputDirectory = string.Empty;
        private static readonly object decodeLock = new object();
        //Static Vars
        public static string newLineDelim = "~>";
        public MainED()
        {
            InitializeComponent();
        }
        //Direct Control Functions
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.Title = "Console Log Selection";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbFileLocation.Text = dlg.FileName;
                logFilePathChangedAction();
            }
        }
        private void tbFileLocation_TextChanged(object sender, EventArgs e)
        {
            logFilePathChanged = true;
        }
        private void tbFileLocation_Leave(object sender, EventArgs e)
        {
            if (logFilePathChanged) logFilePathChangedAction();
        }
        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            loadedFileBlocks = null;
            List<FileBlock> fileBlocks = FindFileBlocks(tbFileLocation.Text);
            if (fileBlocks.Count == 0) tbStatus.AppendText(Environment.NewLine + ">No file blocks found in log file.");
            if (fileBlocks.Count == 1) tbStatus.AppendText(Environment.NewLine + ">" + fileBlocks.Count.ToString() + " file block found in log file.");
            else if (fileBlocks.Count > 1) tbStatus.AppendText(Environment.NewLine + ">" + fileBlocks.Count.ToString() + " file blocks found in log file.");
            if (fileBlocks.Count > 0)
            {
                loadedFileBlocks = fileBlocks;
                long totalFileSize = 0;
                foreach (FileBlock block in fileBlocks)
                {
                    totalFileSize += block._BlockFileSize;
                }
                tbStatus.AppendText(Environment.NewLine + ">Total block size: " + FormatByteSize(totalFileSize));
                DisplayFileBlocks();
            }
            btnLoadFile.Enabled = false;
            string outputDirectoryName = echoDropOutDirectory + " - " + Path.GetFileNameWithoutExtension(tbFileLocation.Text);
            outputDirectory = Path.Combine(Path.GetDirectoryName(tbFileLocation.Text)!, outputDirectoryName);
            int dirSuffix = 1;
            while (Directory.Exists(outputDirectory))
                outputDirectory = Path.Combine(Path.GetDirectoryName(tbFileLocation.Text)!, $"{outputDirectoryName}_{dirSuffix++}");
            Directory.CreateDirectory(outputDirectory);
        }
        private void BtnDecode_Click(object? sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button btn && btn.Tag is ValueTuple<FileBlock, System.Windows.Forms.ProgressBar> tuple)
            {
                var (block, progressBar) = tuple;
                DecodeFileBlock(block, progressBar);
                btn.Enabled = false;
            }
        }
        private void btnDecodeAll_Click(object sender, EventArgs e)
        {
            var blockActions = new List<(FileBlock block, System.Windows.Forms.ProgressBar progress)>();
            foreach (Control ctrl in panelFileBlocks.Controls)
            {
                if (ctrl is System.Windows.Forms.Button btn && btn.Tag is ValueTuple<FileBlock, System.Windows.Forms.ProgressBar> tuple)
                {
                    blockActions.Add((tuple.Item1, tuple.Item2));
                }
            }
            Task.Run(() =>
            {
                foreach (var (block, progressBar) in blockActions)
                {
                    DecodeFileBlock(block, progressBar);
                }
            });
        }
        private void msMainGenerateBash_Click(object sender, EventArgs e)
        {
            using var saveBash = new SaveFileDialog()
            {
                Title = "Save Bash Script As…",
                Filter = "Bash script|*.sh",
                DefaultExt = "sh",
                AddExtension = true,
                FileName = "echodroptx.sh"
            };

            if (saveBash.ShowDialog() != DialogResult.OK)
                return;

            var asm = Assembly.GetExecutingAssembly();
            string resourceName = "EchoDrop.AdditionalResources.echodroptx.sh";
            using Stream? resStream = asm.GetManifestResourceStream(resourceName);

            if (resStream == null)
            {
                MessageBox.Show(
                    $"Internal error: resource '{resourceName}' not found.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            try
            {
                using var reader = new StreamReader(resStream);
                string content = reader.ReadToEnd();
                content = content.Replace("\r\n", "\n").Replace("\r", "\n");
                File.WriteAllText(saveBash.FileName, content, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to write script:\n{ex.Message}",
                    "Error Saving File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void msMainSettings_Click(object sender, EventArgs e)
        {
            using var dlg = new Settings();
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);
        }
        //Specific Design Methods
        private void logFilePathChangedAction()
        {
            panelFileBlocks.Controls.Clear();
            if (File.Exists(tbFileLocation.Text))
            {
                tbStatus.AppendText(Environment.NewLine + ">Good file path detected.");
                tbFileLocation.BackColor = Color.White;
                btnLoadFile.Enabled = true;
            }
            else
            {
                tbStatus.AppendText(Environment.NewLine + ">Bad file path detected.");
                tbFileLocation.BackColor = Color.Red;
                btnLoadFile.Enabled = false;
            }
            logFilePathChanged = false;
        }
        private void DecodeFileBlock(FileBlock block, System.Windows.Forms.ProgressBar progressBar)
        {
            Task.Run(() =>
            {
                lock (decodeLock)
                {
                    DecodeFileBlockInternal(block, progressBar);
                }
            });
        }
        private void DecodeFileBlockInternal(FileBlock block, System.Windows.Forms.ProgressBar progressBar)
        {
            Invoke(() =>
            {
                progressBar.Value = 0;
                tbStatus.AppendText(Environment.NewLine + ">Decoding " + block.BlockFullFileName + "...");
            });

            try
            {
                string outPath = Path.Combine(outputDirectory, block.BlockFullFileName);
                using var fs = new FileStream(block.BlockFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(fs);
                using var outFile = new FileStream(outPath, FileMode.Create);

                int currentLine = 0;
                int lineIndex = 0;
                bool checksumVerified = false;
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith(newLineDelim)) continue;

                    string trimmedLine = line.Substring(newLineDelim.Length).Trim();

                    if (currentLine >= block.ContentStartLine && currentLine < block.ContentEndLine)
                    {
                        byte[]? decoded = block.BlockFileEncoding switch
                        {
                            "base64" or "openssl" => decodeBase64(trimmedLine),
                            "uuencode" => decodeUUEncode(trimmedLine),
                            _ => throw new Exception("Unknown File Block Encoding Type \"" + block.BlockFileEncoding + "\".")
                        };

                        if (decoded != null)
                        {
                            outFile.Write(decoded, 0, decoded.Length);
                        }
                        else
                        {
                            throw new Exception("Unknown decoding error.");
                        }

                        int percent = (int)((lineIndex / (double)(block.ContentEndLine - block.ContentStartLine)) * 100);
                        Invoke(() => progressBar.Value = Math.Min(percent, 100));
                        lineIndex++;
                    }
                    currentLine++;
                    if (currentLine >= block.ContentEndLine)
                        break;
                }

                Invoke(() =>
                {
                    progressBar.Value = 100;
                    tbStatus.AppendText(Environment.NewLine + ">Done decoding " + block.BlockFullFileName + ".");
                });
                outFile.Dispose();
                fs.Dispose();
                reader.Dispose();
                // Checksum verification
                if (!string.IsNullOrEmpty(block.BlockFileChecksum) && File.Exists(outPath))
                {
                    string calculated = ComputeFileChecksum(outPath);
                    if (calculated == block.BlockFileChecksum.ToLowerInvariant())
                    {
                        Invoke(() => tbStatus.AppendText(Environment.NewLine + ">Checksum verified."));
                        checksumVerified = true;
                    }
                    if (block.BlockFileCompressed)
                    {
                        string tmpPath = Path.Combine(Path.GetDirectoryName(outPath), Path.GetFileNameWithoutExtension(outPath));
                        try
                        {
                            Directory.CreateDirectory(tmpPath);
                            TarFile.ExtractToDirectory(outPath, tmpPath, true);
                        }
                        catch
                        {
                            throw new Exception("Could not decompress directory.");
                        }
                        if (Path.Exists(tmpPath))
                        {
                            outPath = tmpPath;
                            Invoke(() => tbStatus.AppendText(Environment.NewLine + ">Successfully Unpacked Directory."));
                            File.Delete(outPath + ".tar");
                        }
                    }
                    if (checksumVerified)
                    {
                        Invoke(() => tbStatus.AppendText(Environment.NewLine + ">Successfully Created " + outPath + "."));
                    }
                    else
                    {
                        File.Delete(outPath);
                        throw new Exception("Checksum mismatch. Bad file created or data is corrupted. Removing File.");
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    tbStatus.AppendText(Environment.NewLine + ">Failed to decode " + block.BlockFullFileName + ".");
                    tbStatus.AppendText(Environment.NewLine + ">>" + ex.Message);
                    progressBar.Value = 0;
                });
            }
        }
        public List<FileBlock> FindFileBlocks(string filePath)
        {
            var blocks = new List<FileBlock>();
            try
            {
                using var reader = new StreamReader(filePath);
                int lineNumber = 0;
                FileBlock? currentBlock = null;
                string? line;
                bool contentStartNext = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith(newLineDelim)) continue;
                    string payload = line.Substring(newLineDelim.Length).Trim();
                    if (payload.Equals("=== ECHODROP FILE BEGIN ===", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock = new FileBlock
                        {
                            BlockFilePath = filePath,
                            StartLine = lineNumber
                        };
                        contentStartNext = false;
                    }
                    else if (currentBlock != null && payload.StartsWith("FILENAME:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.BlockFileName = payload.Split(':', 2)[1].Trim();
                    }
                    else if (currentBlock != null && payload.StartsWith("EXTENSION:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.BlockFileExtension = payload.Split(':', 2)[1].Trim();
                    }
                    else if (currentBlock != null && payload.StartsWith("ENCODING:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.BlockFileEncoding = payload.Split(':', 2)[1].Trim();
                    }
                    else if (currentBlock != null && payload.StartsWith("FILESIZE:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (long.TryParse(payload.Split(':', 2)[1].Trim(), out long fileSize))
                        {
                            currentBlock._BlockFileSize = fileSize;
                        }
                    }
                    else if (currentBlock != null && payload.StartsWith("COMPRESSED:", StringComparison.OrdinalIgnoreCase))
                    {
                        string tmp = payload.Split(':', 2)[1].Trim();
                        currentBlock.BlockFileCompressed = bool.Parse(tmp);
                    }
                    else if (currentBlock != null && payload.StartsWith("CHECKSUM:", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.BlockFileChecksum = payload.Split(':', 2)[1].Trim();
                    }
                    else if (currentBlock != null && payload.Equals("CONTENT:", StringComparison.OrdinalIgnoreCase))
                    {
                        contentStartNext = true;
                    }
                    else if (currentBlock != null && contentStartNext)
                    {
                        currentBlock.ContentStartLine = lineNumber;
                        contentStartNext = false;
                    }
                    else if (currentBlock != null && payload.Equals("=== ECHODROP FILE END ===", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.ContentEndLine = lineNumber;
                        currentBlock.EndLine = lineNumber + 1;
                        blocks.Add(currentBlock);
                        currentBlock = null;
                    }
                    lineNumber++;
                }
            }
            catch (Exception ex)
            {
                tbStatus.AppendText(Environment.NewLine + ">>Could not parse file block.");
                tbStatus.AppendText(Environment.NewLine + ">>" + ex.Message);
            }
            return blocks;
        }
        public void DisplayFileBlocks()
        {
            if (loadedFileBlocks != null)
            {
                panelFileBlocks.Controls.Clear();
                if (loadedFileBlocks.Count > 500)
                {
                    panelFileBlocks.Controls.Add(new Label
                    {
                        Text = "[ERROR] Too many file blocks (500+). Please split the log.",
                        ForeColor = Color.Red,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Location = new Point(10, 10),
                        Anchor = AnchorStyles.Right,
                    });
                    return;
                }

                int y = 10;
                int rowHeight = 40;
                int spacing = 10;

                if (loadedFileBlocks.Count > 1)
                {
                    // Decode All button
                    System.Windows.Forms.Button btnDecodeAll = new System.Windows.Forms.Button
                    {
                        Name = "btnDecodeAll",
                        Text = "Decode All",
                        Location = new Point(10, y),
                        Width = 100,
                        Height = 30,
                        BackColor = Color.White,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top
                    };
                    btnDecodeAll.Click += (sender, e) =>
                    {
                        foreach (Control c in panelFileBlocks.Controls)
                        {
                            if (c is System.Windows.Forms.Button btn && btn.Tag is ValueTuple<FileBlock, System.Windows.Forms.ProgressBar>)
                            {
                                btn.PerformClick();
                            }
                        }
                    };

                    panelFileBlocks.Controls.Add(btnDecodeAll);
                    y += 40;
                }

                foreach (var block in loadedFileBlocks)
                {
                    // Filename
                    var lblName = new Label
                    {
                        Text = block.BlockFullFileName,
                        Location = new Point(10, y + 10),
                        Width = 200,
                        AutoEllipsis = true,
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top
                    };

                    // Size
                    var lblSize = new Label
                    {
                        Text = block.BlockFileSize,
                        Location = new Point(220, y + 10),
                        Width = 80,
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Right | AnchorStyles.Top
                    };

                    // ProgressBar
                    var progressBar = new System.Windows.Forms.ProgressBar
                    {
                        Minimum = 0,
                        Maximum = 100,
                        Value = 0,
                        Location = new Point(310, y + 10),
                        Width = 200,
                        Height = 20,
                        BackColor = Color.White,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };

                    // Decode Button
                    var btnDecode = new System.Windows.Forms.Button
                    {
                        Text = "Decode",
                        Location = new Point(520, y + 7),
                        Width = 70,
                        Tag = (block, progressBar),
                        BackColor = Color.White,
                        Anchor = AnchorStyles.Right | AnchorStyles.Top
                    };
                    btnDecode.Click += BtnDecode_Click;
                    // Add controls
                    panelFileBlocks.Controls.Add(lblName);
                    panelFileBlocks.Controls.Add(lblSize);
                    panelFileBlocks.Controls.Add(progressBar);
                    panelFileBlocks.Controls.Add(btnDecode);

                    y += rowHeight + spacing;
                }
            }
        }
        //General Use Methods
        private byte[] decodeBase64(string line)
        {
            try
            {
                return Convert.FromBase64String(line);
            }
            catch
            {

                return Array.Empty<byte>();
            }
        }
        private byte[] decodeUUEncode(string line)
        {
            if (string.IsNullOrEmpty(line)) return Array.Empty<byte>();
            line = line.Trim();
            int len = (line[0] - 32) & 0x3F;
            var result = new List<byte>(len);
            int i = 1;
            while (i + 3 < line.Length)
            {
                byte a = (byte)((line[i] - 32) & 0x3F);
                byte b = (byte)((line[i + 1] - 32) & 0x3F);
                byte c = (byte)((line[i + 2] - 32) & 0x3F);
                byte d = (byte)((line[i + 3] - 32) & 0x3F);
                result.Add((byte)((a << 2) | (b >> 4)));
                if (result.Count < len) result.Add((byte)(((b & 0x0F) << 4) | (c >> 2)));
                if (result.Count < len) result.Add((byte)(((c & 0x03) << 6) | d));
                i += 4;
            }
            return result.ToArray();
        }
        public static string FormatByteSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return len.ToString("0.##") + " " + sizes[order];
        }
        public long CountLines(string filePath)
        {
            long lineCount = 0;
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fs);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(newLineDelim))
                    lineCount++;
            }
            fs.Dispose();
            return lineCount;
        }
        public static string ComputeFileChecksum(string filePath, string algorithm = "SHA256")
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
