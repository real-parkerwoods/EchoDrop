using System.Diagnostics.Metrics;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static EchoDrop.FileBlock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EchoDrop
{
    public partial class MainED : Form
    {
        //Runtime Vars
        bool logFilePathChanged = false;
        private double _logFileLoading;
        static List<FileBlock>? loadedFileBlocks = null;
        string? localNewLine = null;
        readonly string newLineDelim = "~>";
        readonly string echoDropOutDirectory = "EchoDrop Output";

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
            localNewLine = DetectLineEnding(tbFileLocation.Text);
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
                    totalFileSize += (block.ByteEnd - block.ByteStart);
                }
                tbStatus.AppendText(Environment.NewLine + ">Total block size: " + FormatByteSize(totalFileSize));
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
            Invoke(() =>
            {
                progressBar.Value = 0;
                tbStatus.AppendText(Environment.NewLine + ">Decoding " + block.BlockFullFileName + "...");
            });

            Task.Run(() =>
            {
                try
                {
                    using var fs = new FileStream(block.BlockFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fs.Seek(block.ByteStart, SeekOrigin.Begin);
                    using var reader = new StreamReader(fs);
                    string outDir = Path.Combine(Path.GetDirectoryName(block.BlockFilePath)!, echoDropOutDirectory);
                    string outPath;
                    int dirSuffix = 1;
                    while (Directory.Exists(outDir))
                    {
                        outDir = Path.Combine(Path.GetDirectoryName(block.BlockFilePath)!, $"{echoDropOutDirectory}_{dirSuffix++}");
                    }
                    Directory.CreateDirectory(outDir);
                    outPath = Path.Combine(outDir, block.BlockFullFileName);
                    using var outFile = new FileStream(outPath, FileMode.Create);
                    string? line;
                    long decodedBytes = 0;
                    //debug
                    int whilecount = 0;
                    //debug
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (fs.Position > block.ByteEnd) break;

                        line = line.TrimStart();
                        if (!line.StartsWith(newLineDelim)) continue;
                        line = line.Substring(newLineDelim.Length).Trim();
                        //debug
                        if (whilecount < 3)
                        {
                            Invoke(() =>
                            {
                                tbStatus.AppendText(Environment.NewLine + ">>>Line #" + whilecount.ToString() + ": " + line);
                            });
                        }
                        whilecount++;
                        //debug
                        byte[]? decoded = block.BlockFileEncoding switch
                        {
                            "base64" or "openssl" => decodeBase64(line),
                            "uuencode" => decodeUUEncode(line),
                            _ => throw new Exception("Unknown File Block Encoding Type \"" + block.BlockFileEncoding + "\".")
                        };

                        if (decoded != null)
                        {
                            outFile.Write(decoded, 0, decoded.Length);
                            decodedBytes += decoded.Length;
                        }
                        else
                        {
                            throw new Exception("Unknown decoding error.");
                        }

                        int percent = (int)((decodedBytes / (double)(block.ByteEnd - block.ByteStart)) * 100);
                        Invoke(() => progressBar.Value = Math.Min(percent, 100));
                    }
                        Invoke(() =>
                        {
                            progressBar.Value = 100;
                            tbStatus.AppendText(Environment.NewLine + ">Done decoding " + block.BlockFullFileName + ".");
                            tbStatus.AppendText(Environment.NewLine + ">Created " + outPath + ".");
                        });
                        reader.Dispose();
                        fs.Dispose();
                        outFile.Dispose();
                        if (!string.IsNullOrEmpty(block.BlockFileChecksum) && System.IO.File.Exists(outPath))
                        {
                            string calculated = ComputeFileChecksum(outPath);
                            if (calculated == block.BlockFileChecksum.ToLowerInvariant())
                            {
                                Invoke(() => tbStatus.AppendText(Environment.NewLine + ">Checksum verified."));
                            }
                            else
                            {
                                //System.IO.File.Delete(outPath);
                                throw new Exception("Checksum mismatch. Bad file created (" + calculated + "::" + block.BlockFileChecksum + "). Removing File.");
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
                        progressBar.ForeColor = Color.Red;
                    });
                }
            });
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
                bool contentStart = false;
                while ((line = reader.ReadLine()) != null)
                {
                    long lineStart = bytePosition;
                    long lineLength = reader.CurrentEncoding.GetByteCount(line) + reader.CurrentEncoding.GetByteCount(localNewLine);

                    // Only operate on lines with the delim
                    if (!line.StartsWith(newLineDelim))
                    {
                        bytePosition += lineLength;
                        continue;
                    }
                    if (contentStart == true) currentBlock.ByteStart = bytePosition;
                    lineNumber++;

                    // Strip the delim so comparisons are simpler
                    string payload = line.Substring(newLineDelim.Length).Trim();

                    if (payload.Equals("=== ECHODROP FILE BEGIN ===", StringComparison.OrdinalIgnoreCase))
                    {
                        currentBlock = new FileBlock
                        {
                            BlockFilePath = filePath,
                            StartLine = lineNumber
                        };
                        contentStart = false;
                    }
                    else if (currentBlock != null && payload.StartsWith("FILENAME:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = payload.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileName = parts[1].Trim();
                    }
                    else if (currentBlock != null && payload.StartsWith("EXTENSION:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = payload.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileExtension = parts[1].Trim();
                    }
                    else if (currentBlock != null && payload.StartsWith("ENCODING:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = payload.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileEncoding = parts[1].Trim();
                    }
                    else if (currentBlock != null && payload.StartsWith("CHECKSUM:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = payload.Split(':', 2);
                        if (parts.Length == 2) currentBlock.BlockFileChecksum = parts[1].Trim();
                    }
                    else if (currentBlock != null && payload.Equals("CONTENT:", StringComparison.OrdinalIgnoreCase))
                    {
                        contentStart = true;
                    }
                    else if (currentBlock != null && payload.Equals("=== ECHODROP FILE END ===", StringComparison.OrdinalIgnoreCase))
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
                tbStatus.AppendText(Environment.NewLine + ">[ERROR] " + ex.Message);
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
        private string DetectLineEnding(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            int prevByte = -1;
            int currByte;
            while ((currByte = fs.ReadByte()) != -1)
            {
                if (prevByte == '\r' && currByte == '\n') return "\r\n";
                if (currByte == '\n') return "\n";
                if (currByte == '\r') return "\r";
                prevByte = currByte;
            }

            return Environment.NewLine;
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
