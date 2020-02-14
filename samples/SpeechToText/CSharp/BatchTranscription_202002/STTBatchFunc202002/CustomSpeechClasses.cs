using System;
using System.Collections.Generic;
using System.Text;

namespace STTBatchFunc202002
{
    public class TranscriptRequest
    {
        public string RecordingsUrl { get; set; }
        public Model[] Models { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TranscriptResult
    {
        public string recordingsUrl { get; set; }
        public Resultsurls resultsUrls { get; set; }
        public Model[] models { get; set; }
        public string statusMessage { get; set; }
        public DateTime lastActionDateTime { get; set; }
        public string status { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public string locale { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Properties properties { get; set; }
    }

    public class Resultsurls
    {
        public string channel_0 { get; set; }
    }

    public class Properties
    {
        public string ProfanityFilterMode { get; set; }
        public string PunctuationMode { get; set; }
    }

    public class Model
    {
        public string modelKind { get; set; }
        public object[] datasets { get; set; }
        public DateTime lastActionDateTime { get; set; }
        public string status { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public string locale { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public ModelProperties properties { get; set; }
    }

    public class ModelProperties
    {
        public string Purpose { get; set; }
        public string ModelClass { get; set; }
        public string VadKind { get; set; }
        public string UsesHalide { get; set; }
        public string Deprecated { get; set; }
    }


}
