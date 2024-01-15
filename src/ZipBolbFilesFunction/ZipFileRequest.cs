using System.Collections.Generic;

namespace ZipBlobFilesFunction
{
    public class ZipFileRequest
    {
        [StringArrayRequired(ErrorMessage = "FilePaths is required.")]
        public IReadOnlyCollection<string> FilePaths { get; set; }
    }
}
