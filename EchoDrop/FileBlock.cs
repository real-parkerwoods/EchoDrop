using static EchoDrop.MainED;
namespace EchoDrop
{
    public class FileBlock
    {
        public long StartLine { get; set; }
        public long EndLine { get; set; }
        public long _BlockFileSize { get; set; }
        public string BlockFileSize
        {
            get
            {
                return FormatByteSize(_BlockFileSize);
            }
        }
        public bool BlockFileCompressed { get; set; }
        public string BlockFileName = string.Empty;
        public string BlockFileExtension = string.Empty;
        public string BlockFileEncoding = string.Empty;
        public string BlockFilePath = string.Empty;
        public string BlockFileChecksum = string.Empty;
        public int ContentStartLine { get; set; }
        public int ContentEndLine { get; set; }
        public string BlockFullFileName { get
            {
                if (!string.IsNullOrEmpty(BlockFileExtension))
                {
                    return BlockFileName + "." + BlockFileExtension;
                }
                else return BlockFileName;
            } }
    }
}
