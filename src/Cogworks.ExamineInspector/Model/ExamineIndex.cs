using System.Runtime.Serialization;

namespace Cogworks.ExamineInspector.Model
{
    [DataContract]
    public class ExamineIndex
    {
        [DataMember(Name = "indexName")]
        public string Name { get; set; }

        [DataMember(Name = "indexPath")]
        public string IndexPath { get; set; }
    }
}