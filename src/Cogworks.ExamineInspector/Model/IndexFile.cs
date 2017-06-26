using System.Runtime.Serialization;

namespace Cogworks.ExamineInspector.Model
{
    [DataContract]
    public class IndexFile
    {
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "fileSize")]
        public long FileSize { get; set; }
    }
}