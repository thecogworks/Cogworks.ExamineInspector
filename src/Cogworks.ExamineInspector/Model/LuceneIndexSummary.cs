using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cogworks.ExamineInspector.Model
{
    [DataContract]
    public class LuceneIndexSummary
    {
        [DataMember(Name = "noOfFields")]
        public int NumberOfFields { get; set; }

        [DataMember(Name = "noOfDocuments")]
        public int NoOfDocuments { get; set; }

        [DataMember(Name = "noOfTerms")]
        public int NoOfTerms { get; set; }

        [DataMember(Name = "dateLastModified")]
        public DateTime DateLastModified { get; set; }

        [DataMember(Name = "fields")]
        public IEnumerable<string> Fields { get; set; }

        [DataMember(Name = "indexFiles")]
        public IEnumerable<IndexFile> IndexFiles { get; set; }
    }
}