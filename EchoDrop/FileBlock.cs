using static EchoDrop.MainED;
namespace EchoDrop
{
    public class FileBlock
    {
        public long StartLine { get; set; }
        public long EndLine { get; set; }
        public long ByteStart { get; set; }
        public long ByteEnd { get; set; }
        public string BlockFileName = string.Empty;
        public string BlockFileExtension = string.Empty;
        public string BlockFileEncoding = string.Empty;
        public string BlockFilePath = string.Empty;
        public string BlockFileChecksum = string.Empty;
        public string BlockFullFileName { get
            {
                if (!string.IsNullOrEmpty(BlockFileExtension))
                {
                    return BlockFileName + "." + BlockFileExtension;
                }
                else return BlockFileName;
            } }
        public string BlockSize
        {
            get
            {
                return FormatByteSize(ByteEnd - ByteStart);
            }
        }
    }
}
