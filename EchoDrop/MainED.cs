using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Windows.Forms;
using static EchoDrop.FileBlock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EchoDrop
{
    public partial class MainED : Form
    {
        bool logFilePathChanged = false;
        private double _logFileLoading;
        static List<FileBlock>? loadedFileBlocks = null;
        public MainED()
        {
            InitializeComponent();
        }
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
            if (fileBlocks.Count == 0) tbStatus.Text += Environment.NewLine + ">No file blocks found in log file.";
            if (fileBlocks.Count == 1) tbStatus.Text += Environment.NewLine + ">" + fileBlocks.Count.ToString() + " file block found in log file.";
            else if (fileBlocks.Count > 1) tbStatus.Text += Environment.NewLine + ">" + fileBlocks.Count.ToString() + " file blocks found in log file.";
            if (fileBlocks.Count > 0)
            {
                loadedFileBlocks = fileBlocks;
                long totalFileSize = 0;
                foreach (FileBlock block in fileBlocks)
                {
                    totalFileSize += (block.ByteEnd - block.ByteStart);
                }
                tbStatus.Text += Environment.NewLine + ">Total block size: " + FormatByteSize(totalFileSize);
                DisplayFileBlocks();
            }
            btnLoadFile.Enabled = false;
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

        private void logFilePathChangedAction()
        {
            tbStatus.Text += Environment.NewLine + ">Log file path changed.";
            if (File.Exists(tbFileLocation.Text))
            {
                tbStatus.Text += Environment.NewLine + ">Good file path detected.";
                btnLoadFile.Enabled = true;
            }
            else
            {
                tbStatus.Text += Environment.NewLine + ">Bad file path detected.";
                btnLoadFile.Enabled = false;
            }
            logFilePathChanged = false;
        }
        private void DecodeFileBlock(FileBlock block, System.Windows.Forms.ProgressBar progressBar)
        {
            Invoke(() =>
            {
                progressBar.Value = 0;
                tbStatus.AppendText(Environment.NewLine + ">Decoding " + block.BlockFullFileName + "...");
            });
            Task.Run(() =>
            {
                try
                {
                    long totalBytes = block.ByteEnd - block.ByteStart;
                    long processedBytes = 0;
                    using var fs = new FileStream(block.BlockFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fs.Seek(block.ByteStart, SeekOrigin.Begin);
                    using var reader = new StreamReader(fs);
                    var outPath = System.IO.Path.GetDirectoryName(block.BlockFilePath) + "\\temp\\";
                    System.IO.Directory.CreateDirectory(outPath);
                    outPath = outPath + block.BlockFullFileName;
                    using var outFile = new FileStream(outPath, FileMode.Create);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        long lineBytes = reader.CurrentEncoding.GetByteCount(line) + reader.CurrentEncoding.GetByteCount(Environment.NewLine);
                        processedBytes += lineBytes;
                        if (processedBytes > (block.ByteEnd - block.ByteStart)) break;
                        byte[]? decoded = null;
                        if (block.BlockFileEncoding == "base64" || block.BlockFileEncoding == "openssl")
                        {
                            decoded = decodeBase64(line);
                        }
                        else if (block.BlockFileEncoding == "uuencode")
                        {
                            decoded = decodeUUEncode(line);
                        }
                        else
                        {
                            throw new Exception("Unknown File Block Encoding Type " + block.BlockFileEncoding);
                        }


                        if (decoded != null)
                        {
                            outFile.Write(decoded, 0, decoded.Length);
                            decoded = null;
                        }
                        else
                        {
                            throw new Exception("Unknown Decoding Error");
                        }
                        int percent = (int)((processedBytes / (double)totalBytes) * 100);
                        Invoke(() => progressBar.Value = Math.Min(percent, 100));
                    }
                    Invoke(() =>
                    {
                        progressBar.Value = 100;
                        tbStatus.AppendText(Environment.NewLine + ">Done decoding " + block.BlockFullFileName + ".");
                        tbStatus.AppendText(Environment.NewLine + ">Created " + outPath + ".");
                    });
                    fs.Dispose();
                }
                catch (Exception ex)
                {
                    Invoke(() =>
                    {
                        tbStatus.AppendText(Environment.NewLine + ">Failed to decode " + block.BlockFullFileName + ".");
                        tbStatus.AppendText(">>" + ex.Message);
                        progressBar.Value = 0;
                        progressBar.ForeColor = Color.Red;
                    });
                }
            });
        }
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
        public long CountLines(string filePath)
        {
            long lineCount = 0;
            const int bufferSize = 1024 * 1024; // 1 MB buffer

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == '\n') lineCount++;
                }
            }
            fs.Dispose();
            return lineCount;
        }
        public List<FileBlock> FindFileBlocks(string filePath)
        {
            var blocks = new List<FileBlock>();

            try
            {
                long totalLines = CountLines(filePath);
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(fs);
                long lineNumber = 0;
                long bytePosition = 0;
                FileBlock? currentBlock = null;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    long lineStart = bytePosition;
                    long lineLength = reader.CurrentEncoding.GetByteCount(line) + reader.CurrentEncoding.GetByteCount(Environment.NewLine);
                    if (line.Trim().Equals("{[=== ECHODROP FILE BEGIN ===]}", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock = new FileBlock
                        {
                            BlockFilePath = filePath,
                            StartLine = lineNumber,
                        };
                    }
                    else if (currentBlock != null && line.Contains("{[FILENAME: ", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileName = parts[1].Trim(']', '}');
                    }
                    else if (currentBlock != null && line.Contains("{[EXTENSION: ", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileExtension = parts[1].Trim(']', '}');
                    }
                    else if (currentBlock != null && line.Contains("{[ENCODING: ", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileEncoding = parts[1].Trim(']', '}');
                    }
                    else if (currentBlock != null && line.Contains("{[CHECKSUM: ", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileEncoding = parts[1].Trim(']','}');
                    }
                    else if (currentBlock != null && line.Trim().Equals("{[CONTENT:]}", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.ByteStart = bytePosition + lineLength;
                    }
                    else if (currentBlock != null && line.Trim().Equals("{[=== ECHODROP FILE END ===]}", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock.ByteEnd = lineStart;
                        currentBlock.EndLine = lineNumber;
                        blocks.Add(currentBlock);
                        currentBlock = null;
                    }
                    bytePosition += lineLength;
                }
            }
            catch (Exception ex)
            {
                tbStatus.Text += Environment.NewLine + ">[ERROR] " + ex.Message;
            }
            return blocks;
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
                        Text = block.BlockSize,
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
    }
}
